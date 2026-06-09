using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Controllers
{
    public class DernekIslemleriController : Controller
    {
        private readonly IDernekIslemleriService _dernekService;
        private readonly IAssociationImportService _importService;

        public DernekIslemleriController(IDernekIslemleriService dernekService, IAssociationImportService importService)
        {
            _dernekService = dernekService;
            _importService = importService;
        }

        public async Task<IActionResult> Index()
        {
            var dernekler = await _dernekService.GetActiveDerneklerAsync();
            return View(dernekler);
        }

        public async Task<IActionResult> Create()
        {            
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kurum dernek)
        {
            ModelState.Remove("UstKurum");
            ModelState.Remove("Gorevlendirmeler");
            ModelState.Remove("DernekUyeleri");
            
            if (ModelState.IsValid)
            {
                try
                {
                    await _dernekService.CreateDernekAsync(dernek);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            return View(dernek);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.KurumTurleri = await _dernekService.GetUstKurumlarAsync();
            return View(dernek);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetay(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            ViewBag.KurumTurleri = await _dernekService.GetUstKurumlarAsync();
            return PartialView("_DernekDetayPartial", dernek);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBaskan(int id, string ad, string iletisim)
        {
            await _dernekService.UpdateBaskanAsync(id, ad, iletisim);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDinGorevlisi(int id, string ad, string iletisim)
        {
            await _dernekService.UpdateDinGorevlisiAsync(id, ad, iletisim);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUye(DernekUye uye)
        {
            ModelState.Remove("Kurum");

            if (ModelState.IsValid)
            {
                await _dernekService.AddUyeAsync(uye);
            }
            return RedirectToAction(nameof(GetDetay), new { id = uye.KurumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUye(int id, int kurumId)
        {
            await _dernekService.DeleteUyeAsync(id);
            return RedirectToAction(nameof(GetDetay), new { id = kurumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUye(int id, string adSoyad, string iletisim, int aileUyeSayisi, int kurumId)
        {
            var success = await _dernekService.UpdateUyeAsync(id, adSoyad, iletisim, aileUyeSayisi);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDernek(int id, string isim, string? sehir, string? adres, 
            string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId)
        {
            var success = await _dernekService.UpdateDernekAsync(id, isim, sehir, adres, kurulusKanunu, baskonsoloslukBolgesi, bolge, crmUyelikFormDurumu, ustKurumId);
            return Json(new { success });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _dernekService.SoftDeleteDernekAsync(id);
            if (!success) return NotFound();
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null)
            {
                ModelState.AddModelError("", "Lütfen bir Excel dosyası seçin.");
                return View();
            }

            var result = await _importService.ImportAssociationsAsync(file);
            return View("ImportResult", result);
        }
    }
}
