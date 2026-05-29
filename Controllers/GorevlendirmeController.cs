using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<IActionResult> Index(GorevlendirmeFilterViewModel filter, int page = 1)
        {
            await PrepareFilterDropdowns();
            
            filter.PageNumber = page;
            ViewData["Filter"] = filter;
            
            int pageSize = 20;
            var paginatedList = await _gorevlendirmeService.GetFilteredGorevlendirmelerAsync(filter, pageSize);
            
            return View(paginatedList);
        }

        private async Task PrepareFilterDropdowns()
        {
            var gorevliler = await _gorevliService.GetAllAsync(orderBy: q => q.OrderBy(g => g.Ad).ThenBy(g => g.Soyad));
            var kurumlar = await _kurumService.GetAllAsync(orderBy: q => q.OrderBy(k => k.Isim));

            ViewBag.Gorevliler = gorevliler.ToList();
            ViewBag.Kurumlar = kurumlar.ToList();
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

            ViewData["GorevliId"] = new SelectList(gorevliler, "Id", "AdSoyad", gorevlendirme?.GorevliId);
            ViewData["KurumId"] = new SelectList(kurumlar, "Id", "Isim", gorevlendirme?.KurumId);
            ViewData["YerineGelecekGorevliId"] = new SelectList(gorevliler, "Id", "AdSoyad", gorevlendirme?.YerineGelecekGorevliId);
        }
    }
}
