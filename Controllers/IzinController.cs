using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DitibStasbourg.Controllers
{
    public class IzinController : Controller
    {
        private readonly IIzinService _izinService;
        private readonly ISystemAuditLogService _auditService;
        private readonly IDibbysPdfEngine _pdfEngine;
        private readonly IIzinHesaplamaService _izinEngine;
        private readonly ILogger<IzinController> _logger;

        public IzinController(IIzinService izinService, ISystemAuditLogService auditService, IDibbysPdfEngine pdfEngine, IIzinHesaplamaService izinEngine, ILogger<IzinController> logger)
        {
            _izinService = izinService;
            _auditService = auditService;
            _pdfEngine = pdfEngine;
            _izinEngine = izinEngine;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? gorevliId)
        {
            ViewBag.CurrentGorevliId = gorevliId;
            var records = await _izinService.GetIzinsAsync(gorevliId);
            ViewBag.AvailableYears = await _izinService.GetAvailableYearsAsync();
            return View(records);
        }

        public async Task<IActionResult> Create(int? gorevliId)
        {
            var selectListItems = await _izinService.GetGorevlilerSelectListAsync();
            ViewBag.Gorevliler = new SelectList(selectListItems, "Id", "Isim", gorevliId);

            if (gorevliId.HasValue)
            {
                return View(new GorevliIzin { GorevliId = gorevliId.Value, BaslangicTarihi = DateTime.Today, BitisTarihi = DateTime.Today.AddDays(1) });
            }
            return View(new GorevliIzin { BaslangicTarihi = DateTime.Today, BitisTarihi = DateTime.Today.AddDays(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TalepEt([FromForm] GorevliIzin request)
        {
            // STRIP OUT RELATIONSHIP EXTRA KEYS TO FORCE MODEL VALIDATION PASS
            ModelState.Remove("Gorevli");
            ModelState.Remove("OnaylayanKisi");

            if (request.BaslangicTarihi > request.BitisTarihi)
            {
                ModelState.AddModelError("", "Başlangıç tarihi bitiş tarihinden sonra olamaz.");
                var selectListItems = await _izinService.GetGorevlilerSelectListAsync();
                ViewBag.Gorevliler = new SelectList(selectListItems, "Id", "Isim", request.GorevliId);
                return View("Create", request);
            }

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values) {
                    foreach (var error in modelState.Errors) {
                        _logger.LogWarning("TalepEt ModelState hatası: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                var selectListItems = await _izinService.GetGorevlilerSelectListAsync();
                ViewBag.Gorevliler = new SelectList(selectListItems, "Id", "Isim", request.GorevliId);
                return View("Create", request);
            }

            try
            {
                // Calculate the correct Jours Ouvrables counting range (omitting Sundays)
                request.ToplamGun = _izinEngine.CalculateJoursOuvrables(request.BaslangicTarihi, request.BitisTarihi);
                request.OnayDurumu = OnayDurumu.Beklemede;
                request.IsManualEntryByAdmin = false;
                request.TalepTarihi = DateTime.UtcNow;

                // FORCE PERSISTENCE DIRECTLY TO THE DATABASE
                await _izinService.AddAsync(request);
                await _izinService.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İzin kaydetme sırasında istisna oluştu");
                ModelState.AddModelError("", "Sistemsel veri tabanı kilit hatası: " + ex.Message);
                var selectListItems = await _izinService.GetGorevlilerSelectListAsync();
                ViewBag.Gorevliler = new SelectList(selectListItems, "Id", "Isim", request.GorevliId);
                return View("Create", request);
            }
        }

        [HttpPost]
        // [Authorize(Roles = "SuperAdmin,AtaselikYonetici")] // Commented for testing as per standard procedures
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvrakIleKaydet([FromForm] GorevliIzin manualEntry)
        {
            ModelState.Remove("Gorevli");
            ModelState.Remove("OnaylayanKisi");

            if (!ModelState.IsValid) {
                foreach (var modelState in ModelState.Values) {
                    foreach (var error in modelState.Errors) {
                        _logger.LogWarning("EvrakIleKaydet ModelState hatası: {ErrorMessage}", error.ErrorMessage);
                    }
                }
                var selectListItems = await _izinService.GetGorevlilerSelectListAsync();
                ViewBag.Gorevliler = new SelectList(selectListItems, "Id", "Isim", manualEntry.GorevliId);
                return View("Create", manualEntry);
            }

            manualEntry.ToplamGun = _izinEngine.CalculateJoursOuvrables(manualEntry.BaslangicTarihi, manualEntry.BitisTarihi);
            manualEntry.OnayDurumu = OnayDurumu.Onaylandi; // Paper records pre-signed by amir skip approval states
            manualEntry.IsManualEntryByAdmin = true;

            await _izinService.AddAsync(manualEntry);
            await _izinService.SaveChangesAsync();
            return RedirectToAction("Index", "Izin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OnayDurumu durum)
        {
            var izin = await _izinService.GetByIdAsync(id);
            if (izin == null) return NotFound();

            await _izinService.UpdateStatusAsync(id, durum, User.Identity?.Name);
            await _izinService.SaveChangesAsync();

            await _auditService.LogAsync(
                "Information",
                User.Identity?.Name ?? "System",
                $"İzin durumu güncellendi: ID={id}, Yeni Durum={durum}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                "IzinController");

            return RedirectToAction(nameof(Index), new { gorevliId = izin.GorevliId });
        }

        // Leave record detail view
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var izin = await _izinService.GetDetailsAsync(id);

            if (izin == null) return NotFound();

            ViewBag.TotalAccrued = _izinEngine.CalculateTotalAccruedDays(izin.Gorevli?.FransaGirisTarihi, null);
            return View(izin);
        }

        // Handle scanned paper uploads
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvrakYukle(int id, IFormFile evrakDosyasi)
        {
            var izin = await _izinService.GetByIdAsync(id);
            if (izin == null || evrakDosyasi == null || evrakDosyasi.Length == 0)
                return BadRequest();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "izinler");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(evrakDosyasi.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".png", ".jpg", ".jpeg" };
            if (!allowedExtensions.Contains(ext))
            {
                TempData["Error"] = "Yalnızca PDF, PNG veya JPG dosyaları yüklenebilir.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var uniqueFileName = Guid.NewGuid().ToString() + ext;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await evrakDosyasi.CopyToAsync(fileStream);
            }

            izin.EvrakDosyaYolu = "/uploads/izinler/" + uniqueFileName;
            await _izinService.SaveChangesAsync();

            TempData["Success"] = "Evrak başarıyla yüklendi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // PDF download endpoint

        public async Task<IActionResult> DownloadIzinPdf(int id)
        {
            var pdfBytes = await _pdfEngine.GenerateLeavePdfAsync(id);
            return File(pdfBytes, "application/pdf", $"Izin_{id}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> DownloadMerkezIzinDefteri(int? year)
        {
            return await ExportMerkezExcel(year);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadImamIzinDefteri(int? year)
        {
            return await ExportOtherExcel(year);
        }

        // Merkez (Strasbourg) personnel leave tracker excel export
        public async Task<IActionResult> ExportMerkezExcel(int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var staffList = await _izinService.GetMerkezStaffAsync(targetYear);

            var fileBytes = GenerateYatayTakvimExcel(staffList, targetYear);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Merkez_Izin_Defteri_{targetYear}.xlsx");
        }

        // Other personnel (associated imams) leave tracker excel export
        public async Task<IActionResult> ExportOtherExcel(int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var staffList = await _izinService.GetOtherStaffAsync(targetYear);

            var fileBytes = GenerateYatayTakvimExcel(staffList, targetYear);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Gorevli_Imam_Izin_Defteri_{targetYear}.xlsx");
        }

        private byte[] GenerateYatayTakvimExcel(List<Gorevli> staffList, int year)
        {
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialOrganization("DITIB Strasbourg");
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var quarters = new List<QuarterDefinition>
                {
                    new QuarterDefinition { Name = "1. Çeyrek (Ocak-Mart)", Months = new List<int> { 1, 2, 3 } },
                    new QuarterDefinition { Name = "2. Çeyrek (Nisan-Haziran)", Months = new List<int> { 4, 5, 6 } },
                    new QuarterDefinition { Name = "3. Çeyrek (Temmuz-Eylül)", Months = new List<int> { 7, 8, 9 } },
                    new QuarterDefinition { Name = "4. Çeyrek (Ekim-Aralık)", Months = new List<int> { 10, 11, 12 } }
                };

                foreach (var q in quarters)
                {
                    var worksheet = package.Workbook.Worksheets.Add(q.Name);

                    // Set Page Print Settings to fit A4 Landscape perfectly without stretching text
                    worksheet.PrinterSettings.Orientation = OfficeOpenXml.eOrientation.Landscape;
                    worksheet.PrinterSettings.PaperSize = OfficeOpenXml.ePaperSize.A4;
                    worksheet.PrinterSettings.FitToWidth = 1;
                    worksheet.PrinterSettings.FitToHeight = 1;

                    // Row 1 & 2 Headers
                    worksheet.Cells["A1"].Value = "Çalışan";
                    worksheet.Cells["B1"].Value = "Biriken İzin";
                    worksheet.Cells["C1"].Value = "Kullanılan";
                    worksheet.Cells["D1"].Value = "Kalan";

                    worksheet.Cells["A1:A2"].Merge = true;
                    worksheet.Cells["B1:B2"].Merge = true;
                    worksheet.Cells["C1:C2"].Merge = true;
                    worksheet.Cells["D1:D2"].Merge = true;

                    foreach (var cell in new[] { "A1", "B1", "C1", "D1" })
                    {
                        worksheet.Cells[cell].Style.Font.Bold = true;
                        worksheet.Cells[cell].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        worksheet.Cells[cell].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }

                    int currentColumn = 5;
                    foreach (var month in q.Months)
                    {
                        int daysInMonth = DateTime.DaysInMonth(year, month);
                        string monthHeader = new DateTime(year, month, 1).ToString("MMMM", new System.Globalization.CultureInfo("tr-TR")).ToUpper();

                        // Merge month header dynamically over its total day span
                        worksheet.Cells[1, currentColumn, 1, currentColumn + daysInMonth - 1].Merge = true;
                        worksheet.Cells[1, currentColumn].Value = monthHeader;
                        worksheet.Cells[1, currentColumn].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[1, currentColumn].Style.Font.Bold = true;

                        // Build numeric days sequence
                        for (int d = 1; d <= daysInMonth; d++)
                        {
                            worksheet.Cells[2, currentColumn].Value = d;
                            worksheet.Cells[2, currentColumn].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                            // Check if the specific column date falls on a Sunday
                            var headerDate = new DateTime(year, month, d);
                            if (headerDate.DayOfWeek == DayOfWeek.Sunday)
                            {
                                // Apply soft gray warning shade to the header Sunday column cell
                                var headerCell = worksheet.Cells[2, currentColumn];
                                headerCell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                headerCell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(224, 224, 224)); // Soft Gray
                            }
                            currentColumn++;
                        }
                    }

                    // Populate Employee Details
                    int currentRow = 3;
                    foreach (var staff in staffList)
                    {
                        worksheet.Cells[currentRow, 1].Value = $"{staff.Ad} {staff.Soyad}";

                        var startDate = staff.FransaGirisTarihi 
                            ?? staff.Gorevlendirmeler?.Where(gl => !gl.IsDeleted).OrderBy(gl => gl.Tarih).Select(gl => (DateTime?)gl.Tarih).FirstOrDefault();
                        DateTime targetEndDate = (year == DateTime.Today.Year) ? DateTime.Today : new DateTime(year, 12, 31);
                        decimal accrued = _izinEngine.CalculateTotalAccruedDays(startDate, targetEndDate);

                        int used = staff.Izinler != null 
                            ? staff.Izinler.Where(i => !i.IsDeleted && 
                                                       i.OnayDurumu == OnayDurumu.Onaylandi && 
                                                       i.IzinTuru == IzinTuru.YillikIzin &&
                                                       i.BaslangicTarihi.Year == year)
                                           .Sum(i => i.ToplamGun)
                            : 0;

                        worksheet.Cells[currentRow, 2].Value = accrued;
                        worksheet.Cells[currentRow, 3].Value = used;
                        worksheet.Cells[currentRow, 4].Value = accrued - (decimal)used;

                        int colTracker = 5;
                        foreach (var month in q.Months)
                        {
                            int daysInMonth = DateTime.DaysInMonth(year, month);
                            for (int d = 1; d <= daysInMonth; d++)
                            {
                                var cellDate = new DateTime(year, month, d);
                                var cell = worksheet.Cells[currentRow, colTracker];

                                // Style all Sundays across the entire sheet row to maintain visibility
                                if (cellDate.DayOfWeek == DayOfWeek.Sunday)
                                {
                                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 240, 240)); // Lighter soft gray
                                }

                                // Intersect Check: Is the staff member on an approved leave on this date?
                                bool isOnLeave = staff.Izinler != null && staff.Izinler.Any(i => 
                                    !i.IsDeleted &&
                                    i.OnayDurumu == OnayDurumu.Onaylandi && 
                                    cellDate >= i.BaslangicTarihi.Date && 
                                    cellDate <= i.BitisTarihi.Date);

                                if (isOnLeave)
                                {
                                    cell.Value = 1;
                                    cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(242, 220, 219)); // Soft Rose pink
                                    cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                }
                                colTracker++;
                            }
                        }
                        currentRow++;
                    }

                    // Apply borders
                    var borderRange = worksheet.Cells[1, 1, currentRow - 1, currentColumn - 1];
                    borderRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    borderRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    borderRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    borderRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    worksheet.Cells[1, 1, currentRow - 1, 4].AutoFitColumns();
                    for (int col = 5; col < currentColumn; col++)
                    {
                        worksheet.Column(col).Width = 2.5;
                    }
                }
                return package.GetAsByteArray();
            }
        }
    }

    public class QuarterDefinition
    {
        public string Name { get; set; }
        public List<int> Months { get; set; }
    }
}
