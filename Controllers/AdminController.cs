using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services.Implementations;
using System.Diagnostics;
// System.Net.Http — removed (unused import)
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;

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

        public AdminController(
            ApplicationDbContext context, 
            ISystemAuditLogService auditLogService, 
            IConfiguration configuration,
            IDataMaintenanceService maintenanceService,
            ImportProgressTracker progressTracker)
        {
            _context = context;
            _auditLogService = auditLogService;
            _configuration = configuration;
            _maintenanceService = maintenanceService;
            _progressTracker = progressTracker;
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

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // Belt-and-suspenders: action-level guard
        public async Task<IActionResult> PurgeTestData()
        {
            var username = User.Identity?.Name ?? "System_Daemon";
            await TestDataInitializer.PurgeMockDataAsync(_context);
            await _auditLogService.LogAsync(
                "Warning",
                username,
                "Veritabanındaki mock test verileri temizlendi.",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "AdminController");
            TempData["Success"] = "Mock test verileri başarıyla temizlendi.";
            return RedirectToAction(nameof(DataMaintenance));
        }

        [HttpPost]
        public async Task<IActionResult> SeedTestData()
        {
            var username = User.Identity?.Name ?? "System_Deamon";
            await TestDataInitializer.SeedMockDataAsync(_context);
            await _auditLogService.LogAsync("Information", username, "Veritabanına mock test verileri yüklendi.", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1", "AdminController");
            TempData["Success"] = "Mock test verileri başarıyla oluşturuldu.";
            return RedirectToAction("Index", "Home");
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

            var viewModel = new DataMaintenanceViewModel
            {
                HissedarDuplicatesCount = hissedarDupes.Count(),
                GorevliDuplicatesCount = gorevliDupes.Count(),
                DernekDuplicatesCount = dernekDupes.Count(),
                TotalPotentialBottlenecks = hissedarDupes.Count() + gorevliDupes.Count() + dernekDupes.Count(),
                FlaggedDuplicates = hissedarDupes.Concat(gorevliDupes).Concat(dernekDupes).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PurgeDuplicates(string module)
        {
            var count = await _maintenanceService.PurgeDuplicateEntriesAsync(module);

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
    }
}
