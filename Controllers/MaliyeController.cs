using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Controllers
{
    [Authorize(Policy = "MaliyeStaffOnly")]
    public class MaliyeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MaliyeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? selectedYear, List<int>? selectedKurumIds)
        {
            var query = _context.KurumButceler
                .Include(b => b.Kurum)
                .Include(b => b.Periods)
                .AsQueryable();

            if (selectedKurumIds != null && selectedKurumIds.Any())
            {
                query = query.Where(b => selectedKurumIds.Contains(b.Id));
            }
            else if (selectedYear.HasValue && selectedYear.Value > 0)
            {
                query = query.Where(b => b.Yil == selectedYear.Value);
            }

            var budgets = await query
                .OrderByDescending(b => b.Yil)
                .ThenBy(b => b.Kurum.Isim)
                .ToListAsync();

            var notifications = await _context.OverdueNotifications
                .Where(n => !n.IsResolved)
                .OrderByDescending(n => n.Severity == "Critical" ? 0 : n.Severity == "Warning" ? 1 : 2)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.SelectedKurumIds = selectedKurumIds ?? new List<int>();
            ViewBag.Notifications = notifications;

            return View(budgets);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Kurumlar = await _context.Kurum.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int KurumId, int Yil, decimal TotalBudget)
        {
            var exists = await _context.KurumButceler.AnyAsync(b => b.KurumId == KurumId && b.Yil == Yil);
            if (exists)
            {
                ModelState.AddModelError("", "Bu kurum için seçilen yılda zaten bütçe tanımlanmış.");
                ViewBag.Kurumlar = await _context.Kurum.ToListAsync();
                return View();
            }

            var budget = new KurumButce
            {
                KurumId = KurumId,
                Yil = Yil,
                TotalBudget = TotalBudget,
                DitibContribution = TotalBudget * 0.80m,
                DernekContribution = TotalBudget * 0.20m,
                Periods = new List<KurumButcePeriod>()
            };

            decimal quarterAmount = (TotalBudget * 0.80m) / 4m;
            var targetDates = new Dictionary<int, DateTime>
            {
                { 1, new DateTime(Yil, 1, 20) },
                { 2, new DateTime(Yil, 3, 5) },
                { 3, new DateTime(Yil, 7, 6) },
                { 4, new DateTime(Yil, 10, 10) }
            };

            for (int i = 1; i <= 4; i++)
            {
                budget.Periods.Add(new KurumButcePeriod
                {
                    PeriodNumber = i,
                    TargetDate = targetDates[i],
                    ScheduledAmount = quarterAmount,
                    IsPaid = false
                });
            }

            _context.KurumButceler.Add(budget);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemainingBalance(int id)
        {
            var budget = await _context.KurumButceler
                .Include(b => b.Periods)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (budget == null) return NotFound();

            var paidSum = budget.Periods
                .Where(p => p.IsPaid)
                .Sum(p => p.ScheduledAmount);

            var remaining = budget.DitibContribution - paidSum;
            return Json(new { remaining });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportExcel(string paymentFilter, List<int> selectedKurumIds, int? selectedYear)
        {
            var query = _context.KurumButceler
                .Include(b => b.Kurum)
                .Include(b => b.Periods)
                .AsQueryable();

            // Overriding Priority logic matching UX Kural A & Kural B
            if (selectedKurumIds != null && selectedKurumIds.Any())
            {
                // Kural A: Bypass year filter entirely, use selected budgets only
                query = query.Where(b => selectedKurumIds.Contains(b.Id));
            }
            else if (selectedYear.HasValue && selectedYear.Value > 0)
            {
                // Kural B: If no specific budgets are selected, filter by year
                query = query.Where(b => b.Yil == selectedYear.Value);
            }

            var rawData = await query.ToListAsync();
            
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Finansal Analiz");
            
            var headerRow = ws.Row(1);
            headerRow.Cell(1).Value = "Yıl";
            headerRow.Cell(2).Value = "Bölge";
            headerRow.Cell(3).Value = "Dernek / Cami İsmi";
            headerRow.Cell(4).Value = "Dönem / Taksit No";
            headerRow.Cell(5).Value = "Planlanan Tutar (€)";
            headerRow.Cell(6).Value = "Ödeme Durumu";
            headerRow.Cell(7).Value = "Ödeme Tarihi";
            
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#0D6EFD");
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            int currentRow = 2;
            if (!selectedYear.HasValue || selectedYear.Value == 0)
            {
                // All Years selected -> partition row blocks by absolute Yil metadata value
                var partitionedData = rawData
                    .GroupBy(b => b.Yil)
                    .OrderByDescending(g => g.Key);

                foreach (var group in partitionedData)
                {
                    // Visual partition header row
                    ws.Cell(currentRow, 1).Value = $"{group.Key} Yılı Bütçe Raporu";
                    ws.Range(currentRow, 1, currentRow, 7).Merge().Style.Font.Bold = true;
                    ws.Range(currentRow, 1, currentRow, 7).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E2E8F0");
                    currentRow++;

                    foreach (var budget in group.OrderBy(b => b.Kurum.Isim))
                    {
                        foreach (var period in budget.Periods.OrderBy(p => p.PeriodNumber))
                        {
                            if (paymentFilter == "PaidOnly" && !period.IsPaid) continue;
                            if (paymentFilter == "UnpaidOnly" && period.IsPaid) continue;

                            ws.Cell(currentRow, 1).Value = budget.Yil;
                            ws.Cell(currentRow, 2).Value = budget.Kurum.Bolge;
                            ws.Cell(currentRow, 3).Value = budget.Kurum.Isim;
                            ws.Cell(currentRow, 4).Value = $"{period.PeriodNumber}. Dönem";
                            ws.Cell(currentRow, 5).Value = period.ScheduledAmount;
                            ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00 €";
                            ws.Cell(currentRow, 6).Value = period.IsPaid ? "Ödendi" : "Bekliyor";
                            ws.Cell(currentRow, 7).Value = period.PaymentDate.HasValue ? period.PaymentDate.Value.ToString("dd/MM/yyyy") : "-";
                            
                            currentRow++;
                        }
                    }
                    currentRow++; // Empty separator row
                }
            }
            else
            {
                // Specific year selected, normal output
                foreach (var budget in rawData.OrderBy(b => b.Kurum.Isim))
                {
                    foreach (var period in budget.Periods.OrderBy(p => p.PeriodNumber))
                    {
                        if (paymentFilter == "PaidOnly" && !period.IsPaid) continue;
                        if (paymentFilter == "UnpaidOnly" && period.IsPaid) continue;

                        ws.Cell(currentRow, 1).Value = budget.Yil;
                        ws.Cell(currentRow, 2).Value = budget.Kurum.Bolge;
                        ws.Cell(currentRow, 3).Value = budget.Kurum.Isim;
                        ws.Cell(currentRow, 4).Value = $"{period.PeriodNumber}. Dönem";
                        ws.Cell(currentRow, 5).Value = period.ScheduledAmount;
                        ws.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00 €";
                        ws.Cell(currentRow, 6).Value = period.IsPaid ? "Ödendi" : "Bekliyor";
                        ws.Cell(currentRow, 7).Value = period.PaymentDate.HasValue ? period.PaymentDate.Value.ToString("dd/MM/yyyy") : "-";
                        
                        currentRow++;
                    }
                }
            }
            
            ws.Columns().AdjustToContents();
            using var ms = new System.IO.MemoryStream();
            workbook.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Maliye_Senkronize_Rapor.xlsx");
        }

        [HttpPost]
        [Route("Maliye/MarkPaid/{periodId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "maliyeWrite")]
        public async Task<IActionResult> MarkPaid(
            [FromRoute] int periodId,
            [FromForm] string? paidFromSource,
            [FromForm] string? tutanakNo,
            [FromForm] IFormFile? attachment)
        {
            var period = await _context.KurumButcePeriods.FindAsync(periodId);
            if (period == null) return Json(new { success = false, message = "Hedef dönem kaydı bulunamadı." });

            period.IsPaid = !period.IsPaid;
            period.PaymentDate = period.IsPaid ? DateTime.UtcNow : null;

            var resolvedNotiIds = new List<int>();

            if (period.IsPaid)
            {
                period.PaidFromSource = paidFromSource;
                if (!string.IsNullOrEmpty(tutanakNo))
                {
                    period.TransactionTutanakNo = tutanakNo;
                }
                else if (string.IsNullOrEmpty(period.TransactionTutanakNo))
                {
                    period.TransactionTutanakNo = $"TRX-{Guid.NewGuid():N}";
                }

                // Resolve associated notifications
                var activeNotifications = await _context.OverdueNotifications
                    .Where(n => n.KurumButcePeriodId == periodId && !n.IsResolved)
                    .ToListAsync();

                foreach (var notification in activeNotifications)
                {
                    notification.IsResolved = true;
                    notification.ResolvedAt = DateTime.UtcNow;
                    notification.ResolutionNotes = "Manuel ödeme tablosu veya modal üzerinden otomatik mutabakat ile kapatıldı.";
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    resolvedNotiIds.Add(notification.Id);
                }

                if (attachment != null && attachment.Length > 0)
                {
                    try
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "receipts");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(attachment.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await attachment.CopyToAsync(stream);
                        }
                        period.AttachmentPath = "/uploads/receipts/" + fileName;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Dosya yükleme hatası: " + ex.Message });
                    }
                }
            }
            else
            {
                period.PaidFromSource = null;
                period.AttachmentPath = null;

                // Rollback historical notifications for this period back to unresolved
                var historicalNotifications = await _context.OverdueNotifications
                    .Where(n => n.KurumButcePeriodId == periodId && n.IsResolved)
                    .ToListAsync();

                foreach (var notification in historicalNotifications)
                {
                    notification.IsResolved = false;
                    notification.ResolvedAt = null;
                    notification.ResolutionNotes = null;
                }
            }

            await _context.SaveChangesAsync();

            var allBudgets = await _context.KurumButceler.Include(b => b.Periods).ToListAsync();
            var totalBudget = allBudgets.Sum(b => b.TotalBudget);
            var totalDitib = allBudgets.Sum(b => b.DitibContribution);
            var totalPaid = allBudgets.SelectMany(b => b.Periods).Where(p => p.IsPaid).Sum(p => p.ScheduledAmount);
            var totalRemaining = totalDitib - totalPaid;

            return Json(new { 
                success = true, 
                isPaid = period.IsPaid,
                paymentDate = period.PaymentDate?.ToString("dd.MM.yyyy"),
                transactionNo = period.TransactionTutanakNo,
                paidFromSource = period.PaidFromSource,
                attachmentPath = period.AttachmentPath,
                resolvedNotificationIds = resolvedNotiIds,
                stats = new {
                    totalBudget = totalBudget.ToString("C2"),
                    totalDitib = totalDitib.ToString("C2"),
                    totalPaid = totalPaid.ToString("C2"),
                    totalRemaining = totalRemaining.ToString("C2")
                }
            });
        }



        // ── Budget Revision / Ek Ödenek Workflow ──────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestBudgetRevision(int TargetBudgetId, decimal RequestedAmount, string RevisionReason, string RevisionType)
        {
            var budget = await _context.KurumButceler.FindAsync(TargetBudgetId);
            if (budget == null) return Json(new { success = false, message = "Bütçe kaydı bulunamadı." });

            var revision = new BudgetRevision
            {
                KurumButceId = TargetBudgetId,
                AdditionalAmount = RequestedAmount,
                Reason = RevisionReason,
                RevisionType = string.IsNullOrEmpty(RevisionType) ? "EkOdenek" : RevisionType,
                RequestedBy = User.Identity?.Name ?? "System",
                ApprovalStatus = "Beklemede"
            };

            _context.BudgetRevisions.Add(revision);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Bütçe revizyonu talebi oluşturuldu. Onay bekleniyor." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBudgetRevision(int revisionId, bool approve)
        {
            var revision = await _context.BudgetRevisions
                .Include(r => r.KurumButce)
                .ThenInclude(b => b.Periods)
                .FirstOrDefaultAsync(r => r.Id == revisionId);

            if (revision == null) return Json(new { success = false, message = "Revizyon kaydı bulunamadı." });

            revision.ApprovalStatus = approve ? "Onaylandı" : "Reddedildi";
            revision.ApprovedBy = User.Identity?.Name ?? "System";
            revision.ApprovedAt = DateTime.UtcNow;

            if (approve)
            {
                // Apply revision to budget
                revision.KurumButce.TotalBudget += revision.AdditionalAmount;
                revision.KurumButce.DitibContribution = revision.KurumButce.TotalBudget * 0.80m;
                revision.KurumButce.DernekContribution = revision.KurumButce.TotalBudget * 0.20m;

                decimal quarterAmount = (revision.KurumButce.TotalBudget * 0.80m) / 4m;
                foreach (var period in revision.KurumButce.Periods)
                {
                    period.ScheduledAmount = quarterAmount;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = approve ? "Revizyon onaylandı ve bütçeye yansıtıldı." : "Revizyon reddedildi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetBudgetRevisions(int kurumButceId)
        {
            var revisions = await _context.BudgetRevisions
                .Where(r => r.KurumButceId == kurumButceId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new
                {
                    r.Id,
                    r.AdditionalAmount,
                    r.Reason,
                    r.RevisionType,
                    r.ApprovalStatus,
                    r.RequestedBy,
                    RequestedAt = r.RequestedAt.ToString("dd.MM.yyyy HH:mm"),
                    r.ApprovedBy,
                    ApprovedAt = r.ApprovedAt.HasValue ? r.ApprovedAt.Value.ToString("dd.MM.yyyy HH:mm") : null
                })
                .ToListAsync();
            return Json(revisions);
        }

        // ── Overdue Notification System ───────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetOverdueNotifications(bool unreadOnly = true)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Json(new List<object>());
            }

            var userId = _userManager.GetUserId(User);
            string roleTemplateName = "";
            if (!string.IsNullOrEmpty(userId))
            {
                var userTemplate = await _context.UserRoleTemplates
                    .Include(urt => urt.RoleTemplate)
                    .FirstOrDefaultAsync(urt => urt.UserId == userId);
                roleTemplateName = userTemplate?.RoleTemplate?.Name ?? "";
            }

            bool isSuperAdminOrAdmin = User.IsInRole("SuperAdmin") || 
                                       roleTemplateName == "SuperAdmin Template" || 
                                       roleTemplateName.Contains("Admin") || 
                                       roleTemplateName.Contains("SüperAdmin");

            bool isMaliye = roleTemplateName.Contains("Maliye") || roleTemplateName.Contains("Finance");
            bool isHR = roleTemplateName.Contains("İnsan Kaynakları") || roleTemplateName.Contains("HR") || roleTemplateName.Contains("Personnel");

            int? userKurumId = null;
            if (!isSuperAdminOrAdmin && !isMaliye && !isHR && !string.IsNullOrEmpty(userId))
            {
                var gorevli = await _context.Gorevli.FirstOrDefaultAsync(g => g.LinkedUserId == userId);
                if (gorevli != null)
                {
                    var activeGorevlendirme = await _context.Gorevlendirme
                        .Where(gv => gv.GorevliId == gorevli.Id && !gv.IsDeleted)
                        .OrderByDescending(gv => gv.Tarih)
                        .FirstOrDefaultAsync();
                    userKurumId = activeGorevlendirme?.KurumId;
                }
                if (userKurumId == null)
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    if (appUser != null && !string.IsNullOrEmpty(appUser.Email))
                    {
                        var userKurum = await _context.Kurum.FirstOrDefaultAsync(k => k.BaskanMail == appUser.Email || k.Maili == appUser.Email);
                        userKurumId = userKurum?.Id;
                    }
                }
            }

            var query = _context.OverdueNotifications
                .Include(n => n.KurumButcePeriod)
                .Include(n => n.RelatedBudgetPeriod)
                .Where(n => !n.IsResolved);

            // Re-engineering (temporal restrictions bypass):
            // Show only payments that are absolutely unpaid and unresolved
            query = query.Where(n => 
                (n.KurumButcePeriod == null || !n.KurumButcePeriod.IsPaid) &&
                (n.RelatedBudgetPeriod == null || !n.RelatedBudgetPeriod.IsPaid)
            );

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            // Apply RBAC filters
            if (isSuperAdminOrAdmin)
            {
                // Access to all notifications
            }
            else if (isMaliye)
            {
                query = query.Where(n => n.NotificationType == "OdemeGecikme" || n.NotificationType == "GlobalBroadcast");
            }
            else if (isHR)
            {
                query = query.Where(n => n.NotificationType == "VizeSuresi" || 
                                         n.NotificationType == "PasaportSuresi" || 
                                         n.NotificationType == "OturumIzni" || 
                                         n.NotificationType == "GlobalBroadcast");
            }
            else
            {
                query = query.Where(n => 
                    n.NotificationType == "GlobalBroadcast" ||
                    (userKurumId.HasValue && (
                        n.RelatedKurumId == userKurumId.Value ||
                        (n.RelatedGorevliId != null && _context.Gorevlendirme.Any(gv => gv.GorevliId == n.RelatedGorevliId && gv.KurumId == userKurumId.Value))
                    ))
                );
            }

            var notifications = await query
                .OrderByDescending(n => n.Severity == "Critical" ? 0 : n.Severity == "Warning" ? 1 : 2)
                .ThenByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new
                {
                    n.Id,
                    n.NotificationType,
                    n.Severity,
                    n.Title,
                    n.Message,
                    n.IsRead,
                    n.DueDate,
                    CreatedAt = n.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                    n.RelatedKurumId,
                    n.RelatedGorevliId,
                    n.KurumButcePeriodId
                })
                .ToListAsync();

            return Json(notifications);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkNotificationRead(int notificationId)
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

            var notification = await _context.OverdueNotifications.FindAsync(notificationId);
            if (notification == null) return Json(new { success = false });

            var userId = _userManager.GetUserId(User);
            string roleTemplateName = "";
            if (!string.IsNullOrEmpty(userId))
            {
                var userTemplate = await _context.UserRoleTemplates
                    .Include(urt => urt.RoleTemplate)
                    .FirstOrDefaultAsync(urt => urt.UserId == userId);
                roleTemplateName = userTemplate?.RoleTemplate?.Name ?? "";
            }

            if (!await IsUserAuthorizedForNotification(userId, roleTemplateName, notification))
            {
                return Forbid();
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

            var userId = _userManager.GetUserId(User);
            string roleTemplateName = "";
            if (!string.IsNullOrEmpty(userId))
            {
                var userTemplate = await _context.UserRoleTemplates
                    .Include(urt => urt.RoleTemplate)
                    .FirstOrDefaultAsync(urt => urt.UserId == userId);
                roleTemplateName = userTemplate?.RoleTemplate?.Name ?? "";
            }

            bool isSuperAdminOrAdmin = User.IsInRole("SuperAdmin") || 
                                       roleTemplateName == "SuperAdmin Template" || 
                                       roleTemplateName.Contains("Admin") || 
                                       roleTemplateName.Contains("SüperAdmin");

            bool isMaliye = roleTemplateName.Contains("Maliye") || roleTemplateName.Contains("Finance");
            bool isHR = roleTemplateName.Contains("İnsan Kaynakları") || roleTemplateName.Contains("HR") || roleTemplateName.Contains("Personnel");

            int? userKurumId = null;
            if (!isSuperAdminOrAdmin && !isMaliye && !isHR && !string.IsNullOrEmpty(userId))
            {
                var gorevli = await _context.Gorevli.FirstOrDefaultAsync(g => g.LinkedUserId == userId);
                if (gorevli != null)
                {
                    var activeGorevlendirme = await _context.Gorevlendirme
                        .Where(gv => gv.GorevliId == gorevli.Id && !gv.IsDeleted)
                        .OrderByDescending(gv => gv.Tarih)
                        .FirstOrDefaultAsync();
                    userKurumId = activeGorevlendirme?.KurumId;
                }
                if (userKurumId == null)
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    if (appUser != null && !string.IsNullOrEmpty(appUser.Email))
                    {
                        var userKurum = await _context.Kurum.FirstOrDefaultAsync(k => k.BaskanMail == appUser.Email || k.Maili == appUser.Email);
                        userKurumId = userKurum?.Id;
                    }
                }
            }

            var query = _context.OverdueNotifications.Where(n => !n.IsRead);

            if (isSuperAdminOrAdmin)
            {
                // No additional filters
            }
            else if (isMaliye)
            {
                query = query.Where(n => n.NotificationType == "OdemeGecikme" || n.NotificationType == "GlobalBroadcast");
            }
            else if (isHR)
            {
                query = query.Where(n => n.NotificationType == "VizeSuresi" || 
                                         n.NotificationType == "PasaportSuresi" || 
                                         n.NotificationType == "OturumIzni" || 
                                         n.NotificationType == "GlobalBroadcast");
            }
            else
            {
                query = query.Where(n => 
                    n.NotificationType == "GlobalBroadcast" ||
                    (userKurumId.HasValue && (
                        n.RelatedKurumId == userKurumId.Value ||
                        (n.RelatedGorevliId != null && _context.Gorevlendirme.Any(gv => gv.GorevliId == n.RelatedGorevliId && gv.KurumId == userKurumId.Value))
                    ))
                );
            }

            var unread = await query.ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = unread.Count });
        }

        [HttpPost]
        [Route("Maliye/DismissNotification/{id}")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DismissNotification(int id)
        {
            if (!User.Identity?.IsAuthenticated ?? true) return Challenge();

            var notification = await _context.OverdueNotifications.FindAsync(id);
            if (notification == null) return Json(new { success = false, message = "Bildirim bulunamadı." });

            var userId = _userManager.GetUserId(User);
            string roleTemplateName = "";
            if (!string.IsNullOrEmpty(userId))
            {
                var userTemplate = await _context.UserRoleTemplates
                    .Include(urt => urt.RoleTemplate)
                    .FirstOrDefaultAsync(urt => urt.UserId == userId);
                roleTemplateName = userTemplate?.RoleTemplate?.Name ?? "";
            }

            if (!await IsUserAuthorizedForNotification(userId, roleTemplateName, notification))
            {
                return Forbid();
            }

            notification.IsResolved = true;
            notification.ResolvedAt = DateTime.UtcNow;
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Policy = "MaliyeStaffOnly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGlobalBroadcast(string message, string title = "Sistem Duyurusu")
        {
            if (string.IsNullOrEmpty(message))
            {
                return Json(new { success = false, message = "Duyuru mesajı boş olamaz." });
            }

            var broadcast = new OverdueNotification
            {
                NotificationType = "GlobalBroadcast",
                Severity = "Info",
                Title = title,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                IsResolved = false,
                IsRead = false
            };

            _context.OverdueNotifications.Add(broadcast);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Sistem duyurusu başarıyla yayınlandı." });
        }

        private async Task<bool> IsUserAuthorizedForNotification(string userId, string roleTemplateName, OverdueNotification notification)
        {
            bool isSuperAdminOrAdmin = User.IsInRole("SuperAdmin") || 
                                       roleTemplateName == "SuperAdmin Template" || 
                                       roleTemplateName.Contains("Admin") || 
                                       roleTemplateName.Contains("SüperAdmin");

            if (isSuperAdminOrAdmin) return true;

            bool isMaliye = roleTemplateName.Contains("Maliye") || roleTemplateName.Contains("Finance");
            if (isMaliye)
            {
                return notification.NotificationType == "OdemeGecikme" || notification.NotificationType == "GlobalBroadcast";
            }

            bool isHR = roleTemplateName.Contains("İnsan Kaynakları") || roleTemplateName.Contains("HR") || roleTemplateName.Contains("Personnel");
            if (isHR)
            {
                return notification.NotificationType == "VizeSuresi" || 
                       notification.NotificationType == "PasaportSuresi" || 
                       notification.NotificationType == "OturumIzni" || 
                       notification.NotificationType == "GlobalBroadcast";
            }

            // Dernek Başkanları / Görevliler
            if (notification.NotificationType == "GlobalBroadcast") return true;

            int? userKurumId = null;
            if (!string.IsNullOrEmpty(userId))
            {
                var gorevli = await _context.Gorevli.FirstOrDefaultAsync(g => g.LinkedUserId == userId);
                if (gorevli != null)
                {
                    var activeGorevlendirme = await _context.Gorevlendirme
                        .Where(gv => gv.GorevliId == gorevli.Id && !gv.IsDeleted)
                        .OrderByDescending(gv => gv.Tarih)
                        .FirstOrDefaultAsync();
                    userKurumId = activeGorevlendirme?.KurumId;
                }
                if (userKurumId == null)
                {
                    var appUser = await _userManager.FindByIdAsync(userId);
                    if (appUser != null && !string.IsNullOrEmpty(appUser.Email))
                    {
                        var userKurum = await _context.Kurum.FirstOrDefaultAsync(k => k.BaskanMail == appUser.Email || k.Maili == appUser.Email);
                        userKurumId = userKurum?.Id;
                    }
                }
            }

            if (userKurumId.HasValue)
            {
                if (notification.RelatedKurumId == userKurumId.Value) return true;
                if (notification.RelatedGorevliId != null)
                {
                    return await _context.Gorevlendirme.AnyAsync(gv => gv.GorevliId == notification.RelatedGorevliId && gv.KurumId == userKurumId.Value);
                }
            }

            return false;
        }
    }
}
