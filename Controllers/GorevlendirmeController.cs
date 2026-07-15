using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services.Base;

namespace DitibStasbourg.Controllers
{
    public class GorevlendirmeController : Controller
    {
        private readonly IGorevlendirmeService _gorevlendirmeService;
        private readonly IBaseService<Gorevli> _gorevliService;
        private readonly IBaseService<Kurum> _kurumService;

        public GorevlendirmeController(
            IGorevlendirmeService gorevlendirmeService,
            IBaseService<Gorevli> gorevliService,
            IBaseService<Kurum> kurumService)
        {
            _gorevlendirmeService = gorevlendirmeService;
            _gorevliService = gorevliService;
            _kurumService = kurumService;
        }

        public async Task<IActionResult> Index(GorevlendirmeFilterViewModel filter, int page = 1, int? targetKurumId = null)
        {
            await PrepareFilterDropdowns();
            
            filter.PageNumber = page;
            ViewData["Filter"] = filter;

            // Pass sorting params for sort-link generation in the view
            ViewData["CurrentSortBy"] = filter.SortBy;
            ViewData["IsDescending"] = filter.IsDescending;

            // If deep-linked from dashboard vacancy grid, expose the target institution
            // so the view can auto-open the new-assignment modal with it pre-selected.
            if (targetKurumId.HasValue)
            {
                ViewBag.TargetKurumId = targetKurumId.Value;
                ViewBag.AutoOpenNewAssignment = true;
            }
            
            int pageSize = 20;
            var paginatedList = await _gorevlendirmeService.GetFilteredGorevlendirmelerAsync(filter, pageSize);
            
            return View(paginatedList);
        }

        private async Task PrepareFilterDropdowns()
        {
            var today = DateTime.Today;
            var gorevliler = await _gorevliService.GetAllAsync(orderBy: q => q.OrderBy(g => g.Ad).ThenBy(g => g.Soyad));
            var kurumlar = await _kurumService.GetAllAsync(orderBy: q => q.OrderBy(k => k.Isim));

            // Build a lookup of GorevliId -> active Kurum name for contextual dropdown labels
            var activeKurumByGorevli = await _gorevlendirmeService.GetActiveAssignmentsLookupAsync();

            // Contextual select items: "Ad Soyad (Şu Anda: Kurum'da Görevli)" or "Ad Soyad (Müsait - Boşta)"
            var gorevlilerWithStatus = gorevliler.Select(g => new
            {
                Id = g.Id,
                Label = activeKurumByGorevli.TryGetValue(g.Id, out var kurumAdi) && !string.IsNullOrEmpty(kurumAdi)
                    ? $"{g.AdSoyad} (Şu Anda: {kurumAdi}'da Görevli)"
                    : $"{g.AdSoyad} (Müsait - Boşta)"
            }).ToList();

            ViewBag.Gorevliler = gorevliler.ToList();
            ViewBag.GorevlilerWithStatus = gorevlilerWithStatus;
            ViewBag.Kurumlar = kurumlar.Select(k => new {
                Id = k.Id,
                Isim = string.IsNullOrEmpty(k.Sehir) ? k.Isim : $"[{k.Sehir}] - {k.Isim}"
            }).OrderBy(x => x.Isim).ToList();
            ViewBag.Sehirler = kurumlar.Where(k => !string.IsNullOrEmpty(k.Sehir)).Select(k => k.Sehir).Distinct().OrderBy(s => s).ToList();
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _gorevlendirmeService.GetGorevlendirmeDetailsAsync(id.Value);
            if (gorevlendirme == null) return NotFound();

            return View(gorevlendirme);
        }

        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi,YerineGelecekGorevliId,YerineGelisPlanlananTarih,YerineGelisPlanlananBitisTarih")] Gorevlendirme gorevlendirme)
        {
            if (ModelState.IsValid)
            {
                // ── OVERLAP GUARD: prevent concurrent multi-institution assignments ──
                var conflictingKurum = await _gorevlendirmeService.CheckOverlapAsync(
                    gorevlendirme.GorevliId,
                    gorevlendirme.Tarih,
                    gorevlendirme.BitisTarihi);

                if (conflictingKurum != null)
                {
                    ModelState.AddModelError(string.Empty,
                        $"HATA: Seçilen görevli belirtilen tarih aralığında zaten başka bir kurumda ({conflictingKurum}) aktif görevdedir! Mükerrer görevlendirme yapılamaz.");
                    await PrepareDropdowns(gorevlendirme);
                    await PrepareFilterDropdowns();
                    // Return JSON error for modal AJAX submissions, HTML for full-page form
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = ModelState.Values
                            .SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage });
                    return View(gorevlendirme);
                }

                await _gorevlendirmeService.AddAsync(gorevlendirme);
                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns(gorevlendirme);
            return View(gorevlendirme);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _gorevlendirmeService.GetByIdAsync(id.Value);
            if (gorevlendirme == null) return NotFound();
            
            await PrepareDropdowns(gorevlendirme);
            return View(gorevlendirme);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi,YerineGelecekGorevliId,YerineGelisPlanlananTarih,YerineGelisPlanlananBitisTarih")] Gorevlendirme gorevlendirme)
        {
            if (id != gorevlendirme.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _gorevlendirmeService.UpdateAsync(gorevlendirme);
                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns(gorevlendirme);
            return View(gorevlendirme);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _gorevlendirmeService.GetGorevlendirmeDetailsAsync(id.Value);
            if (gorevlendirme == null) return NotFound();

            return View(gorevlendirme);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _gorevlendirmeService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportToExcel(int? year, KurumTip? tip, int? gorevliId, int? kurumId, DateTime? startDate, DateTime? endDate, List<string> columns)
        {
            var content = await _gorevlendirmeService.ExportToExcelAsync(year, tip, gorevliId, kurumId, startDate, endDate, columns);
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Gorevlendirmeler_Ozel.xlsx");
        }

        [HttpGet]
        public async Task<IActionResult> ExportFilteredExcel(GorevlendirmeFilterViewModel filter)
        {
            var query = _gorevlendirmeService.GetFilteredQueryable(filter);
            var assignments = await query
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Görevlendirmeler");

            worksheet.Cell(1, 1).Value = "Görevli Adı-Soyadı";
            worksheet.Cell(1, 2).Value = "Personel Tipi";
            worksheet.Cell(1, 3).Value = "Atandığı Dernek";
            worksheet.Cell(1, 4).Value = "Başlangıç Tarihi";
            worksheet.Cell(1, 5).Value = "Bitiş Tarihi";

            var headerRange = worksheet.Range("A1:E1");
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#F2F2F2");

            int row = 2;
            foreach (var item in assignments)
            {
                worksheet.Cell(row, 1).Value = item.Gorevli?.AdSoyad ?? "";
                worksheet.Cell(row, 2).Value = (item.Gorevli?.IsMerkezPersoneli == true) ? "Merkez Personeli" : "İmam";
                
                string dernekStr = "";
                if (item.Kurum != null)
                {
                    dernekStr = string.IsNullOrEmpty(item.Kurum.Sehir) 
                        ? item.Kurum.Isim 
                        : $"{item.Kurum.Sehir} - {item.Kurum.Isim}";
                }
                worksheet.Cell(row, 3).Value = dernekStr;
                worksheet.Cell(row, 4).Value = item.Tarih.ToString("dd.MM.yyyy");
                worksheet.Cell(row, 5).Value = item.BitisTarihi.HasValue ? item.BitisTarihi.Value.ToString("dd.MM.yyyy") : "Devam Ediyor";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            var fileName = $"Gorevlendirmeler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Exports only the rows with matching IDs to an Excel file.
        /// Columns parameter is a comma-separated list of column keys.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportSelectedExcel(string ids, string? columns)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return BadRequest("Hiçbir kayıt seçilmemiş.");

            var idArray = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => int.TryParse(s.Trim(), out var n) ? n : 0)
                             .Where(n => n > 0)
                             .ToArray();

            var columnArray = string.IsNullOrWhiteSpace(columns)
                ? null
                : columns.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(c => c.Trim())
                         .ToArray();

            var fileBytes = await _gorevlendirmeService.ExportSelectedPlacementsAsync(idArray, columnArray);
            var fileName = $"Gorevlendirmeler_Secili_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        /// <summary>
        /// Soft-deletes the specified placement records (IsDeleted = true) without physical removal.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BulkSoftDelete([FromBody] int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return Json(new { success = false, message = "Silinecek kayıt seçilmedi." });

            try
            {
                var result = await _gorevlendirmeService.BulkSoftDeletePlacementsAsync(ids);
                return Json(new { success = result, message = result ? $"{ids.Length} görevlendirme başarıyla silindi." : "Silinebilecek kayıt bulunamadı." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int gorevlendirmeId, string notIcerik)
        {
            try
            {
                await _gorevlendirmeService.AddNoteAsync(gorevlendirmeId, notIcerik, User.Identity?.Name);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                await _gorevlendirmeService.DeleteNoteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task PrepareDropdowns(Gorevlendirme? gorevlendirme = null)
        {
            var gorevliler = await _gorevliService.GetAllAsync(orderBy: q => q.OrderBy(g => g.Ad).ThenBy(g => g.Soyad));
            var kurumlar = await _kurumService.GetAllAsync(orderBy: q => q.OrderBy(k => k.Isim));
            var kurumList = kurumlar.Select(k => new {
                k.Id,
                DisplayName = string.IsNullOrEmpty(k.Sehir) ? k.Isim : $"[{k.Sehir}] - {k.Isim}"
            }).OrderBy(x => x.DisplayName).ToList();

            ViewData["GorevliId"] = new SelectList(gorevliler, "Id", "AdSoyad", gorevlendirme?.GorevliId);
            ViewData["KurumId"] = new SelectList(kurumList, "Id", "DisplayName", gorevlendirme?.KurumId);
            ViewData["YerineGelecekGorevliId"] = new SelectList(gorevliler, "Id", "AdSoyad", gorevlendirme?.YerineGelecekGorevliId);
        }
    }
}
