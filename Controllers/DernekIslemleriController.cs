using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using System.IO;
using MiniExcelLibs;

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

        public async Task<IActionResult> Index(string? search, string? sehir, string? bolge, int pageNumber = 1, int pageSize = 20)
        {
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSehir = sehir;
            ViewBag.CurrentBolge = bolge;
            ViewBag.PageSize = pageSize;

            var paginatedList = await _dernekService.GetPaginatedDerneklerAsync(search, sehir, bolge, pageNumber, pageSize);
            return View(paginatedList);
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            return View(dernek);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Kurum dernek)
        {
            if (id != dernek.Id) return BadRequest();

            ModelState.Remove("UstKurum");
            ModelState.Remove("Gorevlendirmeler");
            ModelState.Remove("DernekUyeleri");

            if (ModelState.IsValid)
            {
                await _dernekService.UpdateDernekAsync(
                    dernek.Id, dernek.Isim, dernek.Sehir, dernek.Adres, 
                    dernek.KurulusKanunu, dernek.BaskonsoloslukBolgesi, dernek.Bolge, 
                    dernek.CrmUyelikFormDurumu, dernek.UstKurumId, dernek.IletisimNumarasi, 
                    dernek.Maili, dernek.Latitude, dernek.Longitude);

                await _dernekService.UpdateBaskanAsync(dernek.Id, dernek.DernekBaskaniAd ?? "", dernek.DernekBaskaniIletisim ?? "", dernek.BaskanMail);
                await _dernekService.UpdateDinGorevlisiAsync(dernek.Id, dernek.DinGorevlisiAd ?? "", dernek.DinGorevlisiIletisim ?? "");

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            return View(dernek);
        }

        [HttpGet]
        public async Task<IActionResult> ExportSingleToExcel(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();

            var rowData = new[]
            {
                new
                {
                    ResmiAdi = dernek.Isim,
                    Sehir = dernek.Sehir ?? "",
                    Adres = dernek.Adres ?? "",
                    UstKurum = dernek.UstKurum?.Ad ?? "Bağımsız",
                    IletisimNumarasi = dernek.IletisimNumarasi ?? "",
                    DernekMaili = dernek.Maili ?? "",
                    DernekBaskani = dernek.DernekBaskaniAd ?? "",
                    DernekBaskaniIletisim = dernek.DernekBaskaniIletisim ?? "",
                    BaskanMail = dernek.BaskanMail ?? "",
                    DinGorevlisi = dernek.DinGorevlisiAd ?? "",
                    DinGorevlisiIletisim = dernek.DinGorevlisiIletisim ?? "",
                    BaskonsoloslukBolgesi = dernek.BaskonsoloslukBolgesi ?? "",
                    Bolge = dernek.Bolge ?? "",
                    KurulusKanunu = dernek.KurulusKanunu ?? "",
                    CrmFormDurumu = dernek.CrmUyelikFormDurumu ?? "",
                    Latitude = dernek.Latitude?.ToString() ?? "",
                    Longitude = dernek.Longitude?.ToString() ?? ""
                }
            };

            using var memoryStream = new MemoryStream();
            await MiniExcel.SaveAsAsync(memoryStream, rowData);
            var content = memoryStream.ToArray();

            var fileName = $"{dernek.Isim.Replace(" ", "_")}_detay.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBaskan(int id, string ad, string iletisim, string? baskanMail)
        {
            await _dernekService.UpdateBaskanAsync(id, ad, iletisim, baskanMail);
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
            string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId,
            string? iletisimNumarasi, string? maili, double? latitude, double? longitude)
        {
            var success = await _dernekService.UpdateDernekAsync(id, isim, sehir, adres, kurulusKanunu, baskonsoloslukBolgesi, bolge, crmUyelikFormDurumu, ustKurumId, iletisimNumarasi, maili, latitude, longitude);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Hiçbir dernek seçilmedi." });
            }

            int deletedCount = 0;
            foreach (var id in ids)
            {
                var success = await _dernekService.SoftDeleteDernekAsync(id);
                if (success) deletedCount++;
            }

            return Json(new { success = true, count = deletedCount });
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
