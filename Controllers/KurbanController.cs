using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DitibStasbourg.Controllers
{
    public class KurbanController : Controller
    {
        private readonly IKurbanService _kurbanService;
        private readonly ISystemAuditLogService _auditLogService;
        private readonly IMemoryCache _cache;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KurbanController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public KurbanController(
            IKurbanService kurbanService,
            ISystemAuditLogService auditLogService,
            IMemoryCache cache,
            ApplicationDbContext context,
            ILogger<KurbanController> logger,
            UserManager<IdentityUser> userManager)
        {
            _kurbanService = kurbanService;
            _auditLogService = auditLogService;
            _cache = cache;
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        // ── Kurban Campaign Index ──────────────────────────────────────────────

        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            // Resolve dynamic share price from AppSettings (fallback: 125 €)
            decimal hisseFiyati = 125m;
            var priceSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "KurbanHisseFiyati");
            if (priceSetting != null && decimal.TryParse(priceSetting.Value, out var parsed))
                hisseFiyati = parsed;

            int totalCount = await _context.KurbanCampaignRecords.Where(r => r.IsApproved).CountAsync();
            var records = await _context.KurbanCampaignRecords
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.Yil)
                .ThenBy(r => r.Cami)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Aggregate stats across ALL approved records (not just current page)
            var totalDigerAdet   = await _context.KurbanCampaignRecords.Where(r => r.IsApproved && r.Yil == 2026).SumAsync(r => r.DigerAdet);
            var totalTrAdet      = await _context.KurbanCampaignRecords.Where(r => r.IsApproved && r.Yil == 2026).SumAsync(r => r.TrAdet);
            var totalDigerMiktar = await _context.KurbanCampaignRecords.Where(r => r.IsApproved && r.Yil == 2026).SumAsync(r => r.DigerMiktar);
            var totalTrMiktar    = await _context.KurbanCampaignRecords.Where(r => r.IsApproved && r.Yil == 2026).SumAsync(r => r.TrMiktar);

            ViewBag.TotalDigerAdet   = totalDigerAdet;
            ViewBag.TotalTrAdet      = totalTrAdet;
            ViewBag.TotalDigerMiktar = totalDigerMiktar;
            ViewBag.TotalTrMiktar    = totalTrMiktar;
            ViewBag.HisseFiyati      = hisseFiyati;

            // Pagination metadata
            ViewBag.CurrentPage = page;
            ViewBag.PageSize    = pageSize;
            ViewBag.TotalPages  = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount  = totalCount;

            return View(records);
        }

        // ── Kurban Campaign Create (GET) ───────────────────────────────────────

        public async Task<IActionResult> Create()
        {
            var activeKurums = await _context.Kurum.Where(k => k.AktifMi).OrderBy(k => k.Isim).ToListAsync();
            ViewBag.Camiler = activeKurums.Select(k => new SelectListItem
            {
                Value = k.Id.ToString(),
                Text = $"{k.Isim} ({k.Sehir})"
            }).ToList();

            // Resolve dynamic share price for display in view
            decimal hisseFiyati = 125m;
            var priceSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "KurbanHisseFiyati");
            if (priceSetting != null && decimal.TryParse(priceSetting.Value, out var parsed))
                hisseFiyati = parsed;
            ViewBag.HisseFiyati = hisseFiyati;

            return View();
        }

        // ── Kurban Campaign Create (POST) ──────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KurbanCampaignRecord record, int KurumId)
        {
            // Resolve dynamic share price from AppSettings (fallback: 125 €)
            decimal hisseFiyati = 125m;
            var priceSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "KurbanHisseFiyati");
            if (priceSetting != null && decimal.TryParse(priceSetting.Value, out var parsed))
                hisseFiyati = parsed;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    record.KurumId = KurumId;
                    var cami = await _context.Kurum.FindAsync(KurumId);
                    if (cami == null)
                    {
                        ModelState.AddModelError("", "Seçilen cami bulunamadı.");
                        goto ReturnView;
                    }

                    var username = User.Identity?.Name ?? "System_Daemon";

                    // 1. Absolute Collision Rule: TutanakNo
                    if (!string.IsNullOrWhiteSpace(record.TutanakNo))
                    {
                        bool absoluteDuplicate = await _context.KurbanCampaignRecords
                            .AnyAsync(r => r.TutanakNo == record.TutanakNo);
                        if (absoluteDuplicate)
                        {
                            ModelState.AddModelError("", $"Tutanak Numarası ({record.TutanakNo}) olan kayıt sistemde zaten mevcuttur. Mükerrer giriş engellendi.");
                            goto ReturnView;
                        }
                    }

                    // 2. Permitted Multi-Tenant Exception (Warning Logic)
                    bool proximityMatch = await _context.KurbanCampaignRecords
                        .AnyAsync(r => r.KurumId == KurumId && r.FysSorumlusu == record.FysSorumlusu && r.Yil == record.Yil);
                    
                    if (proximityMatch)
                    {
                        var logAlert = new SystemAuditLog {
                            UserId = _userManager.GetUserId(User) ?? "system",
                            LogType = "MükerrerTespiti_Uyarı",
                            Message = $"AKILLI UYARI: {username} tarafından girilen veri kümesinde benzerlik saptandı. Detay: Kurban kaydı mevcut veri yapılarıyla %80 üzerinde benzerlik gösteriyor. Lütfen Veri Bakım panelinden doğruluğunu inceleyin.",
                            Timestamp = DateTime.UtcNow,
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                        };
                        _context.SystemAuditLogs.Add(logAlert);
                        // Do not block. The log will be saved when transaction commits.
                    }

                    record.Cami  = cami.Isim;
                    record.Bolge = cami.Bolge ?? "67-STRASB";

                    // Calculate values using dynamic share price
                    record.DigerMiktar    = record.DigerAdet * hisseFiyati;
                    record.TrMiktar       = record.TrAdet    * hisseFiyati;
                    record.ToplamOdenen   = record.Havale + record.Cek + record.Nakit + record.Stripe + record.Cihaz;
                    record.KalanBakiye    = (record.DigerMiktar + record.TrMiktar) - record.ToplamOdenen;

                    _context.KurbanCampaignRecords.Add(record);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    username = User.Identity?.Name ?? "System_Daemon";
                    await _auditLogService.LogAsync(
                        "Information", username,
                        $"Kullanıcı {username} '{record.Cami}' için yeni bir kurban kampanyası kaydı ekledi (Hisse Fiyatı: {hisseFiyati} €).",
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                        "KurbanController");

                    TempData["Success"] = $"'{record.Cami}' için kampanya kaydı başarıyla eklendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Kurban campaign record insertion failed.");
                    ModelState.AddModelError("", $"Kayıt işlemi sırasında bir hata oldu: {ex.Message}");
                }
            }

            ReturnView:
            {
                var activeKurums = await _context.Kurum.Where(k => k.AktifMi).OrderBy(k => k.Isim).ToListAsync();
                ViewBag.Camiler = activeKurums.Select(k => new SelectListItem
                {
                    Value = k.Id.ToString(),
                    Text = $"{k.Isim} ({k.Sehir})"
                }).ToList();
                ViewBag.HisseFiyati = hisseFiyati;
                return View(record);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CheckDuplicate([FromBody] KurbanCampaignRecord record)
        {
            if (!string.IsNullOrWhiteSpace(record.TutanakNo))
            {
                bool absoluteDuplicate = await _context.KurbanCampaignRecords
                    .AnyAsync(r => r.TutanakNo == record.TutanakNo);
                if (absoluteDuplicate)
                    return Json(new { isDuplicate = true, type = "absolute", message = "Bu tutanak numarası ile bir kayıt zaten mevcut. İşlem engellenecektir." });
            }

            bool proximityMatch = await _context.KurbanCampaignRecords
                .AnyAsync(r => r.KurumId == record.KurumId && r.FysSorumlusu == record.FysSorumlusu && r.Yil == record.Yil);

            if (proximityMatch)
            {
                return Json(new { isDuplicate = true, type = "warning", message = "⚠️ DİKKAT: Girmekte olduğunuz bilgilere benzer kayıtlar sistemde mevcut. Eğer eminseniz işleme devam edebilirsiniz, sistem yöneticiye doğrulama bildirimi gönderecektir." });
            }

            return Json(new { isDuplicate = false });
        }

        // ── Legacy Kurbanlik CRUD ──────────────────────────────────────────────

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var kurbanlik = await _kurbanService.GetKurbanlikByIdAsync(id.Value);
            if (kurbanlik == null) return NotFound();
            return View(kurbanlik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kurbanlik kurbanlik)
        {
            if (id != kurbanlik.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _kurbanService.UpdateKurbanlikAsync(kurbanlik);

                await _auditLogService.LogAsync(
                    "Information",
                    User.Identity?.Name ?? "System",
                    $"'{kurbanlik.TagNumber}' küpeli kurbanlık güncellendi.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "KurbanController");

                TempData["Success"] = "Kurbanlık başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            return View(kurbanlik);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _kurbanService.SoftDeleteKurbanlikAsync(id);
            TempData["Success"] = "Kurbanlık pasife alındı.";
            return RedirectToAction(nameof(Index));
        }

        // ── Hissedar (Shareholder) CRUD ────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHissedar(Hissedar hissedar)
        {
            ModelState.Remove("Kurbanlik");

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Form verileri hatalı." });

            var (success, errorReason) = await _kurbanService.AddHissedarAsync(hissedar, _cache);

            if (!success)
                return Json(new { success = false, message = errorReason });

            await _auditLogService.LogAsync(
                "Information",
                User.Identity?.Name ?? "System",
                $"Yeni hissedar eklendi: '{hissedar.Name}' → Kurbanlık ID: {hissedar.KurbanlikId}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "KurbanController");

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetHissedarlar(int kurbanlikId)
        {
            var hissedarlar = await _kurbanService.GetHissedarlarAsync(kurbanlikId);
            return Json(hissedarlar.Select(h => new
            {
                h.Id,
                h.Name,
                h.Phone,
                h.PaymentStatus,
                h.PaymentMethod,
                PaymentMethodName = h.PaymentMethod.ToString(),
                h.TotalPaid,
                h.RemainingBalance,
                h.IsVekaletTaken,
                joinedAt = h.JoinedAt.ToString("dd.MM.yyyy HH:mm")
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateHissedar(int id, string name, string phone, string paymentStatus, bool isVekaletTaken, PaymentMethod paymentMethod, decimal totalPaid, decimal remainingBalance)
        {
            var success = await _kurbanService.UpdateHissedarAsync(new Hissedar
            {
                Id              = id,
                Name            = name,
                Phone           = phone,
                PaymentStatus   = paymentStatus,
                IsVekaletTaken  = isVekaletTaken,
                PaymentMethod   = paymentMethod,
                TotalPaid       = totalPaid,
                RemainingBalance = remainingBalance
            });
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHissedar(int id)
        {
            var success = await _kurbanService.DeleteHissedarAsync(id);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AutoAssign(int shareholderId)
        {
            var success = await _kurbanService.AutoAssignShareholderAsync(shareholderId);
            return Json(new { success, message = success ? "Otomatik atama başarılı." : "Atama yapılamadı: uygun kurbanlık bulunamadı." });
        }

        // ── Campaign Record Payment (Tahsil Et) ────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int id, decimal amount)
        {
            if (amount <= 0)
                return Json(new { success = false, message = "Geçersiz ödeme miktarı." });

            var record = await _context.KurbanCampaignRecords.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Kampanya kaydı bulunamadı." });

            record.ToplamOdenen += amount;
            record.KalanBakiye   = (record.DigerMiktar + record.TrMiktar) - record.ToplamOdenen;
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var username = User.Identity?.Name ?? "System_Daemon";
            await _auditLogService.LogAsync(
                "Information", username,
                $"Ödeme kaydedildi: KampanyaID={id}, Tutar={amount} €, YeniToplam={record.ToplamOdenen} €, Kalan={record.KalanBakiye} €",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "KurbanController");

            return Json(new { success = true, toplamOdenen = record.ToplamOdenen, kalanBakiye = record.KalanBakiye });
        }

        [Authorize(Roles = "SuperAdmin,KurbanOnayYetkilisi")]
        public async Task<IActionResult> PendingApprovals()
        {
            var pending = await _context.KurbanCampaignRecords
                .Include(r => r.Kurum)
                .Where(r => !r.IsApproved)
                .OrderByDescending(r => r.Yil)
                .ThenBy(r => r.Cami)
                .ToListAsync();

            return View(pending);
        }

        [Authorize(Roles = "SuperAdmin,KurbanOnayYetkilisi")]
        public async Task<IActionResult> Approve(int id)
        {
            var record = await _context.KurbanCampaignRecords.FindAsync(id);
            if (record == null) return NotFound();

            record.IsApproved = true;
            _context.Entry(record).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var username = User.Identity?.Name ?? "System_Daemon";
            await _auditLogService.LogAsync(
                "Information", username,
                $"Kurban kampanyası onaylandı: ID={id}, Cami={record.Cami}, Yil={record.Yil}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "KurbanController");

            TempData["Success"] = $"'{record.Cami}' ({record.Yil}) kurban kampanyası kaydı onaylandı.";
            return RedirectToAction(nameof(PendingApprovals));
        }
    }
}
