using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services;

namespace DitibStasbourg.Controllers
{
    public class GorevliController : Controller
    {
        private readonly IGorevliService _gorevliService;
        private readonly ILookupService _lookupService;
        private readonly DitibStasbourg.Services.Base.IBaseService<Kurum> _kurumService;
        private readonly IDynamicExportService _exportService;

        public GorevliController(IGorevliService gorevliService, ILookupService lookupService, 
            DitibStasbourg.Services.Base.IBaseService<Kurum> kurumService,
            IDynamicExportService exportService)
        {
            _gorevliService = gorevliService;
            _lookupService = lookupService;
            _kurumService = kurumService;
            _exportService = exportService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchStaff(string term)
        {
            var staff = await _gorevliService.SearchStaffAsync(term);
            return Json(staff);
        }

        public async Task<IActionResult> Index(GorevliFilterViewModel filter, int page = 1)
        {
            await PrepareFilterDropdowns();
            
            int pageSize = filter.PageSize ?? 20;
            filter.PageNumber = filter.PageNumber ?? page;
            
            ViewData["CurrentSort"] = filter.SortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(filter.SortOrder) || filter.SortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["StatusSortParm"] = filter.SortOrder == "Status" ? "status_desc" : "Status";
            ViewData["DateSortParm"] = filter.SortOrder == "Date" ? "date_desc" : "Date";
            ViewData["ActiveSortParm"] = filter.SortOrder == "Active" ? "active_desc" : "Active";
            ViewData["Filter"] = filter;
            
            var paginatedList = await _gorevliService.GetFilteredGorevlilerAsync(filter, pageSize);
            return View(paginatedList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetGorevliDetailsAsync(id.Value);
            if (gorevli == null) return NotFound();

            return View(gorevli);
        }

        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gorevli gorevli, string? IlkNot)
        {
            if (ModelState.IsValid)
            {
                await _gorevliService.AddAsync(gorevli);

                if (!string.IsNullOrWhiteSpace(IlkNot))
                {
                    await _gorevliService.AddNoteAsync(gorevli.Id, IlkNot, User.Identity?.Name);
                }

                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns();
            return View(gorevli);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetByIdAsync(id.Value);
            if (gorevli == null) return NotFound();
            
            await PrepareDropdowns();
            return View(gorevli);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Gorevli gorevli, string? YeniNot)
        {
            if (id != gorevli.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _gorevliService.UpdateAsync(gorevli);

                if (!string.IsNullOrWhiteSpace(YeniNot))
                {
                    await _gorevliService.AddNoteAsync(gorevli.Id, YeniNot, User.Identity?.Name);
                }
                
                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns();
            return View(gorevli);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetByIdAsync(id.Value);
            if (gorevli == null) return NotFound();

            return View(gorevli);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _gorevliService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Hiçbir görevli seçilmedi." });
            }

            int deletedCount = 0;
            foreach (var id in ids)
            {
                await _gorevliService.DeleteAsync(id);
                deletedCount++;
            }

            return Json(new { success = true, count = deletedCount });
        }

        public async Task<IActionResult> ExportToExcel(GorevliFilterViewModel filter)
        {
            var content = await _gorevliService.ExportToExcelAsync(filter);
            var fileName = $"Gorevliler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> CustomExport(GorevliFilterViewModel filter, List<string> columns)
        {
            if (columns == null || !columns.Any())
            {
                columns = new List<string> { "Ad", "Soyad", "Email", "TCKimlikNo" };
            }

            var query = _gorevliService.GetFilteredQueryable(filter);
            var content = await _exportService.ExportFilteredAsync(query, columns, "Görevliler");
            var fileName = $"Custom_Export_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int gorevliId, string notIcerik)
        {
            try
            {
                await _gorevliService.AddNoteAsync(gorevliId, notIcerik, User.Identity?.Name);
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
                await _gorevliService.DeleteNoteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Lütfen bir Excel dosyası seçin.";
                return View("Import");
            }

            try
            {
                var result = await _gorevliService.ImportFromExcelAsync(file);

                ViewBag.SuccessCount = result.SuccessCount;
                ViewBag.ErrorCount = result.ErrorCount;
                ViewBag.ImportResults = result.Results;
                ViewBag.Errors = result.Errors;
                ViewBag.Message = $"Import tamamlandı: {result.SuccessCount} başarılı, {result.ErrorCount} hata";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Hata: {ex.Message}";
            }

            return View("Import");
        }

        private async Task PrepareDropdowns()
        {
            ViewBag.Durumlar = await _lookupService.GetGorevliDurumlariAsync();
            ViewBag.SozlesmeTipleri = await _lookupService.GetSozlesmeTipleriAsync();
            ViewBag.Unvanlar = await _lookupService.GetUnvanlarAsync();
            ViewBag.EgitimDurumlari = await _lookupService.GetEgitimDurumlariAsync();
            ViewBag.HafizlikDurumlari = await _lookupService.GetHafizlikDurumlariAsync();
            ViewBag.KadroTurleri = await _lookupService.GetKadroTurleriAsync();
            ViewBag.AskerlikDurumlari = await _lookupService.GetAskerlikDurumlariAsync();
            ViewBag.KanGruplari = await _lookupService.GetKanGruplariAsync();
        }

        private async Task PrepareFilterDropdowns()
        {
            await PrepareDropdowns();
            var kurumlar = await _kurumService.GetAllAsync(orderBy: q => q.OrderBy(k => k.Isim));
            ViewBag.Kurumlar = kurumlar.ToList();
            ViewBag.Sehirler = kurumlar.Where(k => !string.IsNullOrEmpty(k.Sehir)).Select(k => k.Sehir).Distinct().OrderBy(s => s).ToList();
        }
    }
}
