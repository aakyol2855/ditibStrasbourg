using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services.Implementations;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemAuditLogService _auditLogService;
        private readonly IConfiguration _configuration;
        private readonly IDataMaintenanceService _maintenanceService;
        private readonly ImportProgressTracker _progressTracker;
        private readonly IMemoryCache _cache;

        public AdminController(
            ApplicationDbContext context, 
            ISystemAuditLogService auditLogService, 
            IConfiguration configuration,
            IDataMaintenanceService maintenanceService,
            ImportProgressTracker progressTracker,
            IMemoryCache cache)
        {
            _context = context;
            _auditLogService = auditLogService;
            _configuration = configuration;
            _maintenanceService = maintenanceService;
            _progressTracker = progressTracker;
            _cache = cache;
        }

        /// <summary>
        /// Landing page for the SuperAdmin workspace.
        /// Defense-in-depth: re-validates the role claim inside the action body
        /// in addition to the class-level [Authorize(Roles = "SuperAdmin")] filter.
        /// If a misconfigured middleware stack ever let a non-SuperAdmin reach here,
        /// this guard logs the attempt and redirects them safely to DataMaintenance.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // ── Defense-in-depth role check ──────────────────────────────────
            if (!User.IsInRole("SuperAdmin"))
            {
                var intruder = User.Identity?.Name ?? "anonymous";
                await _auditLogService.LogAsync(
                    "Warning",
                    intruder,
                    $"Yetkisiz /Admin erişim denemesi tespit edildi. Kullanıcı DataMaintenance sayfasına yönlendirildi.",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "AdminController");

                return RedirectToAction("DataMaintenance", "Admin");
            }

            return RedirectToAction(nameof(DataMaintenance));
        }



        public async Task<IActionResult> SystemLog()
        {
            bool dbHealthy = false;
            try
            {
                dbHealthy = await _context.Database.CanConnectAsync();
            }
            catch {}

            var proc = Process.GetCurrentProcess();
            var memoryUsage = proc.WorkingSet64 / (1024.0 * 1024.0);
            var threadCount = proc.Threads.Count;

            var model = new SystemLogViewModel
            {
                DatabaseHealthy = dbHealthy,
                DatabaseStatus = dbHealthy ? "Active/Healthy" : "Offline",
                MemoryUsageMB = Math.Round(memoryUsage, 2),
                ThreadCount = threadCount,
                RecentLogs = await _auditLogService.GetLogsAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(string? logLevel, string? search)
        {
            var logs = await _auditLogService.GetLogsAsync(logLevel, search);
            return Json(logs.Select(l => new {
                id = l.Id,
                timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                logLevel = l.LogLevel,
                username = l.Username,
                action = l.Action,
                ipAddress = l.IpAddress,
                component = l.Component
            }));
        }

        [HttpPost]
        public async Task<IActionResult> PurgeLogs()
        {
            var username = User.Identity?.Name ?? "System_Deamon";
            await _auditLogService.ClearLogsAsync();
            await _auditLogService.LogAsync("Warning", username, "Sistem Günlüğü (System Audit Logs) veritabanından tamamen temizlendi.", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1", "AdminController");
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> BatchDelete(string module, [FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Silinecek kayıt seçilmedi." });
            }

            var username = User.Identity?.Name ?? "System_Deamon";

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (string.Equals(module, "Hissedar", StringComparison.OrdinalIgnoreCase))
                    {
                        var shareholders = await _context.Hissedarlar
                            .Where(h => ids.Contains(h.Id))
                            .ToListAsync();

                        var kurbanlikIdsToRecalculate = shareholders
                            .Where(h => h.KurbanlikId != null)
                            .Select(h => h.KurbanlikId!.Value)
                            .Distinct()
                            .ToList();

                        _context.Hissedarlar.RemoveRange(shareholders);
                        await _context.SaveChangesAsync();

                        foreach (var kId in kurbanlikIdsToRecalculate)
                        {
                            var kurbanlik = await _context.Kurbanliklar.FindAsync(kId);
                            if (kurbanlik != null)
                            {
                                var currentShareholdersCount = await _context.Hissedarlar.CountAsync(h => h.KurbanlikId == kId);
                                kurbanlik.RemainingShares = kurbanlik.TotalShares - currentShareholdersCount;
                                if (kurbanlik.RemainingShares > 0)
                                {
                                    kurbanlik.Status = "Available";
                                }
                                else
                                {
                                    kurbanlik.Status = "Full";
                                }
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                    else if (string.Equals(module, "Gorevli", StringComparison.OrdinalIgnoreCase))
                    {
                        var assignments = await _context.Gorevlendirme.Where(g => ids.Contains(g.GorevliId)).ToListAsync();
                        _context.Gorevlendirme.RemoveRange(assignments);

                        // FK safety: nullify YazanKisiId on GorevliNotlari to avoid FK_GorevliNotlari_AspNetUsers_YazanKisiId conflict
                        var orphanNotes = await _context.GorevliNotlari
                            .IgnoreQueryFilters()
                            .Where(n => ids.Contains(n.GorevliId))
                            .ToListAsync();
                        foreach (var note in orphanNotes)
                        {
                            note.YazanKisiId = null;
                        }
                        await _context.SaveChangesAsync();

                        var gorevli = await _context.Gorevli.Where(g => ids.Contains(g.Id)).ToListAsync();
                        _context.Gorevli.RemoveRange(gorevli);
                        await _context.SaveChangesAsync();
                    }
                    else if (string.Equals(module, "Dernek", StringComparison.OrdinalIgnoreCase))
                    {
                        var members = await _context.DernekUyeleri.Where(m => ids.Contains(m.KurumId)).ToListAsync();
                        _context.DernekUyeleri.RemoveRange(members);

                        var dernek = await _context.Kurum.Where(k => ids.Contains(k.Id) && k.Tip == KurumTip.Dernek).ToListAsync();
                        _context.Kurum.RemoveRange(dernek);
                        await _context.SaveChangesAsync();
                    }
                    else if (string.Equals(module, "SystemAuditLog", StringComparison.OrdinalIgnoreCase))
                    {
                        var logs = await _context.SystemAuditLogs.Where(l => ids.Contains(l.Id)).ToListAsync();
                        _context.SystemAuditLogs.RemoveRange(logs);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return Json(new { success = false, message = "Bilinmeyen modül." });
                    }

                    await transaction.CommitAsync();

                    if (_cache is MemoryCache concreteCache)
                    {
                        concreteCache.Clear();
                    }

                    await _auditLogService.LogAsync(
                        "Warning",
                        username,
                        $"{module} modülünden seçilen {ids.Count} adet kayıt toplu silme protokolü ile temizlendi.",
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                        "AdminController"
                    );

                    return Json(new { success = true, count = ids.Count });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = $"Toplu silme sırasında bir hata oluştu: {ex.Message}" });
                }
            }
        }

        public async Task<IActionResult> DataMaintenance()
        {
            var hissedarDupes = await _maintenanceService.GetDuplicateEntriesAsync("Hissedar");
            var gorevliDupes = await _maintenanceService.GetDuplicateEntriesAsync("Gorevli");
            var dernekDupes = await _maintenanceService.GetDuplicateEntriesAsync("Dernek");
            var kurbanDupes = await _maintenanceService.GetDuplicateEntriesAsync("Kurban");

            var viewModel = new DataMaintenanceViewModel
            {
                HissedarDuplicatesCount = hissedarDupes.Count(),
                GorevliDuplicatesCount = gorevliDupes.Count(),
                DernekDuplicatesCount = dernekDupes.Count(),
                TotalPotentialBottlenecks = hissedarDupes.Count() + gorevliDupes.Count() + dernekDupes.Count() + kurbanDupes.Count(),
                FlaggedDuplicates = hissedarDupes.Concat(gorevliDupes).Concat(dernekDupes).Concat(kurbanDupes).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PurgeDuplicates(string module)
        {
            var count = await _maintenanceService.PurgeDuplicateEntriesAsync(module);

            if (_cache is MemoryCache concreteCache)
            {
                concreteCache.Clear();
            }

            var username = User.Identity?.Name ?? "System_Deamon";
            await _auditLogService.LogAsync(
                "Warning",
                username,
                $"Kullanıcı {username} {module} modülündeki {count} adet mükerrer kaydı temizledi.",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "AdminController"
            );

            return Json(new { success = true, count = count });
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file, string module, string progressKey)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "Dosya yüklenemedi veya boş." });
            }

            if (string.IsNullOrEmpty(progressKey))
            {
                progressKey = Guid.NewGuid().ToString();
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await _maintenanceService.ImportExcelStreamAsync(stream, module, progressKey);
                }

                var username = User.Identity?.Name ?? "System_Deamon";
                await _auditLogService.LogAsync(
                    "Information",
                    username,
                    $"Kullanıcı {username} Excel dosyasından {module} listesini içe aktardı. (Anahtar: {progressKey})",
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                    "AdminController"
                );

                return Json(new { success = true, message = "İçe aktarma başarıyla tamamlandı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult DownloadSampleExcel(string module)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sample");
                if (string.Equals(module, "Dernek", StringComparison.OrdinalIgnoreCase))
                {
                    worksheet.Cell(1, 1).Value = "Şehir";
                    worksheet.Cell(1, 2).Value = "Derneğin resmi adı";
                    worksheet.Cell(1, 3).Value = "Adres";
                    worksheet.Cell(1, 4).Value = "Başkan ad soyad";
                    worksheet.Cell(1, 5).Value = "İletişim numarası";
                    worksheet.Cell(1, 6).Value = "Maili / Başkan mail";

                    worksheet.Cell(2, 1).Value = "Strasbourg";
                    worksheet.Cell(2, 2).Value = "Strasbourg Yunus Emre Camii Derneği";
                    worksheet.Cell(2, 3).Value = "12 Rue de la Musau";
                    worksheet.Cell(2, 4).Value = "Ali Yılmaz";
                    worksheet.Cell(2, 5).Value = "+33 6 12 34 56 78";
                    worksheet.Cell(2, 6).Value = "strasbourg.dernek@ditib.org";
                }
                else if (string.Equals(module, "Gorevli", StringComparison.OrdinalIgnoreCase))
                {
                    worksheet.Cell(1, 1).Value = "Ad";
                    worksheet.Cell(1, 2).Value = "Soyad";
                    worksheet.Cell(1, 3).Value = "E-posta";
                    worksheet.Cell(1, 4).Value = "Cep Telefonu";
                    worksheet.Cell(1, 5).Value = "TC Kimlik No";

                    worksheet.Cell(2, 1).Value = "Ahmet";
                    worksheet.Cell(2, 2).Value = "Yıldız";
                    worksheet.Cell(2, 3).Value = "ahmet.yildiz@example.com";
                    worksheet.Cell(2, 4).Value = "05321112233";
                    worksheet.Cell(2, 5).Value = "12345678901";
                }
                else if (string.Equals(module, "Kurban", StringComparison.OrdinalIgnoreCase))
                {
                    worksheet.Cell(1, 1).Value = "Küpe Numarası";
                    worksheet.Cell(1, 2).Value = "Tür";
                    worksheet.Cell(1, 3).Value = "Kilo";
                    worksheet.Cell(1, 4).Value = "Fiyat";
                    worksheet.Cell(1, 5).Value = "Hisse Sayısı";

                    worksheet.Cell(2, 1).Value = "TR-34-556677";
                    worksheet.Cell(2, 2).Value = "Büyükbaş";
                    worksheet.Cell(2, 3).Value = "450";
                    worksheet.Cell(2, 4).Value = "15000";
                    worksheet.Cell(2, 5).Value = "7";
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"sample_{module.ToLower()}.xlsx");
                }
            }
        }

        [HttpGet]
        public IActionResult GetImportProgress(string key)
        {
            if (string.IsNullOrEmpty(key)) return BadRequest();
            var progress = _progressTracker.GetProgress(key);
            return Json(new { progress = progress });
        }

        [HttpGet]
        [Route("Admin/TrashBin")]
        public async Task<IActionResult> TrashBin()
        {
            var retentionSetting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "SoftDeleteRetentionDays");
            int retentionDays = 30;
            if (retentionSetting != null && int.TryParse(retentionSetting.Value, out var parsedDays))
            {
                retentionDays = parsedDays;
            }

            var model = new TrashBinViewModel
            {
                DeletedAssociations = await _context.Kurum
                    .IgnoreQueryFilters()
                    .Where(k => k.IsDeleted)
                    .ToListAsync(),

                DeletedPersonnel = await _context.Gorevli
                    .IgnoreQueryFilters()
                    .Where(g => g.IsDeleted)
                    .ToListAsync(),

                DeletedAssignments = await _context.Gorevlendirme
                    .IgnoreQueryFilters()
                    .Where(a => a.IsDeleted)
                    .Include(a => a.Gorevli)
                    .Include(a => a.Kurum)
                    .ToListAsync(),

                SoftDeleteRetentionDays = retentionDays
            };

            return View(model);
        }

        [HttpPost]
        [Route("Admin/TrashBin/UpdateRetentionDays")]
        public async Task<IActionResult> UpdateRetentionDays(int days)
        {
            if (days < 1)
            {
                TempData["Error"] = "Saklama süresi en az 1 gün olmalıdır.";
                return RedirectToAction(nameof(TrashBin));
            }

            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == "SoftDeleteRetentionDays");
            if (setting == null)
            {
                setting = new AppSetting { Key = "SoftDeleteRetentionDays", Value = days.ToString() };
                _context.AppSettings.Add(setting);
            }
            else
            {
                setting.Value = days.ToString();
            }

            await _context.SaveChangesAsync();

            var username = User.Identity?.Name ?? "System_Daemon";
            await _auditLogService.LogAsync(
                "Information",
                username,
                $"Veri saklama süresi (SoftDeleteRetentionDays) güncellendi: {days} gün.",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "AdminController"
            );

            TempData["Success"] = "Veri saklama süresi başarıyla güncellendi.";
            return RedirectToAction(nameof(TrashBin));
        }

        [HttpGet, HttpPost]
        [Route("Admin/TrashBin/Restore")]
        public async Task<IActionResult> Restore(string type, int id)
        {
            if (!User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }
            var username = User.Identity?.Name ?? "System_Daemon";

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    bool success = false;
                    string name = "";

                    if (string.Equals(type, "Association", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "Dernek", StringComparison.OrdinalIgnoreCase))
                    {
                        var association = await _context.Kurum
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(k => k.Id == id);
                        
                        if (association != null)
                        {
                            association.IsDeleted = false;
                            association.DeletedAt = null;
                            name = association.Isim;
                            success = true;
                        }
                    }
                    else if (string.Equals(type, "Personnel", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "Gorevli", StringComparison.OrdinalIgnoreCase))
                    {
                        var personnel = await _context.Gorevli
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(g => g.Id == id);

                        if (personnel != null)
                        {
                            personnel.IsDeleted = false;
                            personnel.DeletedAt = null;
                            name = $"{personnel.Ad} {personnel.Soyad}";
                            success = true;
                        }
                    }
                    else if (string.Equals(type, "Assignment", StringComparison.OrdinalIgnoreCase) || string.Equals(type, "Gorevlendirme", StringComparison.OrdinalIgnoreCase))
                    {
                        var assignment = await _context.Gorevlendirme
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(a => a.Id == id);

                        if (assignment != null)
                        {
                            assignment.IsDeleted = false;
                            assignment.DeletedAt = null;
                            name = $"Görevlendirme #{assignment.Id}";
                            success = true;
                        }
                    }

                    if (success)
                    {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (_cache is MemoryCache concreteCache)
                        {
                            concreteCache.Clear();
                        }

                        await _auditLogService.LogAsync(
                            "Information",
                            username,
                            $"{type} kaydı çöp kutusundan geri yüklendi: {name} (ID: {id})",
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                            "AdminController"
                        );

                        TempData["Success"] = "Kayıt başarıyla geri yüklendi.";
                    }
                    else
                    {
                        TempData["Error"] = "Kayıt bulunamadı veya geri yüklenemedi.";
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Hata oluştu: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(TrashBin));
        }

        /// <summary>
        /// Permanently hard-deletes a single soft-deleted record from the Trash Bin.
        /// Protected by mandatory confirmation modal in TrashBin.cshtml.
        /// </summary>
        [HttpPost]
        [Route("Admin/TrashBin/HardDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteFromTrashBin(string type, int id)
        {
            if (!User.IsInRole("SuperAdmin")) return Forbid();

            var username = User.Identity?.Name ?? "System_Daemon";

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool found = false;
                string label = string.Empty;

                if (string.Equals(type, "Association", StringComparison.OrdinalIgnoreCase))
                {
                    var entity = await _context.Kurum.IgnoreQueryFilters().FirstOrDefaultAsync(k => k.Id == id && k.IsDeleted);
                    if (entity != null)
                    {
                        label = entity.Isim;
                        _context.Kurum.Remove(entity);
                        found = true;
                    }
                }
                else if (string.Equals(type, "Personnel", StringComparison.OrdinalIgnoreCase))
                {
                    var entity = await _context.Gorevli.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.Id == id && g.IsDeleted);
                    if (entity != null)
                    {
                        label = entity.AdSoyad;
                        // FK safety: nullify note author references before removal
                        var notes = await _context.GorevliNotlari.IgnoreQueryFilters().Where(n => n.GorevliId == id).ToListAsync();
                        foreach (var note in notes) { note.YazanKisiId = null; }
                        await _context.SaveChangesAsync();

                        var assignments = await _context.Gorevlendirme.IgnoreQueryFilters().Where(g => g.GorevliId == id).ToListAsync();
                        _context.Gorevlendirme.RemoveRange(assignments);
                        _context.GorevliNotlari.RemoveRange(notes);
                        _context.Gorevli.Remove(entity);
                        found = true;
                    }
                }
                else if (string.Equals(type, "Assignment", StringComparison.OrdinalIgnoreCase))
                {
                    var entity = await _context.Gorevlendirme.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id && a.IsDeleted);
                    if (entity != null)
                    {
                        label = $"Görevlendirme #{entity.Id}";
                        _context.Gorevlendirme.Remove(entity);
                        found = true;
                    }
                }

                if (found)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (_cache is MemoryCache concreteCache) concreteCache.Clear();

                    await _auditLogService.LogAsync(
                        "Warning", username,
                        $"KESİN SİLME (Hard Delete): {type} — {label} (ID: {id}) kalıcı olarak silindi.",
                        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                        "AdminController");

                    TempData["Success"] = $"Kayıt ({label}) veritabanından kalıcı olarak silindi.";
                }
                else
                {
                    TempData["Error"] = "Kayıt bulunamadı veya zaten silinmiş.";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Kalıcı silme işlemi başarısız: {ex.Message}";
            }

            return RedirectToAction(nameof(TrashBin));
        }
    }
}
