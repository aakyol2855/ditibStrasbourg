using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Controllers
{
    public class DernekIslemleriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DernekIslemleriController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dernekler = await _context.Kurum
                .Where(k => (int)k.Tip == 1 && k.AktifMi == true) // 1 = Dernek, only active
                .Include(k => k.UstKurum)
                .OrderBy(k => k.Isim)
                .ToListAsync();
            return View(dernekler);
        }

        // GET: DernekIslemleri/Create
        public async Task<IActionResult> Create()
        {            
            // Get distinct cities from existing records
            ViewBag.Sehirler = await _context.Kurum
                .Where(k => !string.IsNullOrEmpty(k.Sehir))
                .Select(k => k.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            
            // Get parent associations (Ref_KurumTuru)
            ViewBag.UstKurumlar = await _context.Ref_KurumTurus
                .OrderBy(k => k.Ad)
                .ToListAsync();
            
            return View();
        }

        // POST: DernekIslemleri/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kurum dernek)
        {
            // Remove navigation property validation
            ModelState.Remove("UstKurum");
            ModelState.Remove("Gorevlendirmeler");
            ModelState.Remove("DernekUyeleri");
            
            if (ModelState.IsValid)
            {
                // Set type to Dernek
                dernek.Tip = KurumTip.Dernek;
                dernek.AktifMi = true;
                
                _context.Add(dernek);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Reload dropdowns on error
            ViewBag.Sehirler = await _context.Kurum
                .Where(k => !string.IsNullOrEmpty(k.Sehir))
                .Select(k => k.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            ViewBag.UstKurumlar = await _context.Ref_KurumTurus
                .OrderBy(k => k.Ad)
                .ToListAsync();
            
            return View(dernek);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetay(int id)
        {
            var dernek = await _context.Kurum
                .Include(k => k.UstKurum)
                .Include(k => k.DernekUyeleri)
                .FirstOrDefaultAsync(k => k.Id == id);
                
            if (dernek == null) return NotFound();
            
            ViewBag.KurumTurleri = await _context.Ref_KurumTurus
                .Where(x => !x.IsDeleted)
                .OrderBy(k => k.Ad)
                .ToListAsync();
            
            return PartialView("_DernekDetayPartial", dernek);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBaskan(int id, string ad, string iletisim)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return NotFound();

            dernek.DernekBaskaniAd = ad;
            dernek.DernekBaskaniIletisim = iletisim;

            _context.Update(dernek);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDinGorevlisi(int id, string ad, string iletisim)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return NotFound();

            dernek.DinGorevlisiAd = ad;
            dernek.DinGorevlisiIletisim = iletisim;

            _context.Update(dernek);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUye(DernekUye uye)
        {
             // Remove validation binding for "Kurum" nav prop
             ModelState.Remove("Kurum");

            if (ModelState.IsValid)
            {
                _context.DernekUyeleri.Add(uye);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GetDetay), new { id = uye.KurumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUye(int id)
        {
            var uye = await _context.DernekUyeleri.FindAsync(id);
            if (uye != null)
            {
                var kurumId = uye.KurumId;
                _context.DernekUyeleri.Remove(uye);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GetDetay), new { id = kurumId });
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUye(int id, string adSoyad, string iletisim, int aileUyeSayisi, int kurumId)
        {
            var uye = await _context.DernekUyeleri.FindAsync(id);
            if (uye == null) return Json(new { success = false });

            uye.AdSoyad = adSoyad;
            uye.Iletisim = iletisim;
            uye.AileUyeSayisi = aileUyeSayisi;

            _context.Update(uye);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDernek(int id, string isim, string? sehir, string? adres, 
            string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return Json(new { success = false });

            dernek.Isim = isim;
            dernek.Sehir = sehir;
            dernek.Adres = adres;
            dernek.KurulusKanunu = kurulusKanunu;
            dernek.BaskonsoloslukBolgesi = baskonsoloslukBolgesi;
            dernek.Bolge = bolge;
            dernek.CrmUyelikFormDurumu = crmUyelikFormDurumu;
            dernek.UstKurumId = ustKurumId;

            _context.Update(dernek);
            await _context.SaveChangesAsync();
            
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return NotFound();

            // Soft delete - mark as inactive
            dernek.AktifMi = false;
            _context.Update(dernek);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }
    }
}
