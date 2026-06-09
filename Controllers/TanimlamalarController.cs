using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Controllers
{
    public class TanimlamalarController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TanimlamalarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Gorevli Durum

        public async Task<IActionResult> GorevliDurumList()
        {
            return View(await _context.Ref_GorevliDurums
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.Sira)
                .ToListAsync());
        }

        public IActionResult GorevliDurumCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevliDurumCreate(Ref_GorevliDurum model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GorevliDurumList));
            }
            return View(model);
        }

        public async Task<IActionResult> GorevliDurumEdit(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Ref_GorevliDurums.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevliDurumEdit(int id, Ref_GorevliDurum model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Ref_GorevliDurums.AnyAsync(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(GorevliDurumList));
            }
            return View(model);
        }

        public async Task<IActionResult> GorevliDurumDelete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Ref_GorevliDurums.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("GorevliDurumDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevliDurumDeleteConfirmed(int id)
        {
            var model = await _context.Ref_GorevliDurums.FindAsync(id);
            if (model != null)
            {
                model.IsDeleted = true; // Soft Delete
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(GorevliDurumList));
        }

        #endregion

        #region Sozlesme Tip
        
        public async Task<IActionResult> SozlesmeTipList()
        {
            return View(await _context.Ref_SozlesmeTips
                .Where(x => x.IsDeleted == false)
                .OrderBy(x => x.Ad)
                .ToListAsync());
        }

        public IActionResult SozlesmeTipCreate() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SozlesmeTipCreate(Ref_SozlesmeTip model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(SozlesmeTipList));
            }
            return View(model);
        }

        public async Task<IActionResult> SozlesmeTipEdit(int? id)
        {
             if (id == null) return NotFound();
            var model = await _context.Ref_SozlesmeTips.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> SozlesmeTipEdit(int id, Ref_SozlesmeTip model)
        {
            if (id != model.Id) return NotFound();
             if (ModelState.IsValid)
            {
                 _context.Update(model);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(SozlesmeTipList));
            }
            return View(model);
        }
        
         public async Task<IActionResult> SozlesmeTipDelete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Ref_SozlesmeTips.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("SozlesmeTipDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SozlesmeTipDeleteConfirmed(int id)
        {
            var model = await _context.Ref_SozlesmeTips.FindAsync(id);
            if (model != null)
            {
                model.IsDeleted = true;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(SozlesmeTipList));
        }

        #endregion

        #region Kurum Turu
        public async Task<IActionResult> KurumTuruList()
        {
             return View(await _context.Ref_KurumTurus
                 .Where(x => x.IsDeleted == false)
                 .OrderBy(x => x.Ad)
                .ToListAsync());
        }
        
        public IActionResult KurumTuruCreate() => View();
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurumTuruCreate(Ref_KurumTuru model)
        {
             if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(KurumTuruList));
            }
            return View(model);
        }
        
        public async Task<IActionResult> KurumTuruEdit(int? id)
        {
             if (id == null) return NotFound();
            var model = await _context.Ref_KurumTurus.FindAsync(id);
             if (model == null) return NotFound();
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
         public async Task<IActionResult> KurumTuruEdit(int id, Ref_KurumTuru model)
        {
             if (id != model.Id) return NotFound();
             if (ModelState.IsValid)
            {
                 _context.Update(model);
                 await _context.SaveChangesAsync();
                 return RedirectToAction(nameof(KurumTuruList));
            }
            return View(model);
        }
        
        public async Task<IActionResult> KurumTuruDelete(int? id)
        {
            if (id == null) return NotFound();
            var model = await _context.Ref_KurumTurus.FindAsync(id);
             if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost, ActionName("KurumTuruDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurumTuruDeleteConfirmed(int id)
        {
             var model = await _context.Ref_KurumTurus.FindAsync(id);
            if (model != null)
            {
                model.IsDeleted = true;
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(KurumTuruList));
        }

        #endregion

        #region Unvan
        public async Task<IActionResult> UnvanList() => View(await _context.Ref_Unvans.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult UnvanCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> UnvanCreate(Ref_Unvan model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(UnvanList)); } return View(model); }
        public async Task<IActionResult> UnvanEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_Unvans.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> UnvanEdit(int id, Ref_Unvan model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(UnvanList)); } return View(model); }
        public async Task<IActionResult> UnvanDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_Unvans.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("UnvanDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> UnvanDeleteConfirmed(int id) { var model = await _context.Ref_Unvans.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(UnvanList)); }
        #endregion

        #region EgitimDurumu
        public async Task<IActionResult> EgitimDurumuList() => View(await _context.Ref_EgitimDurumlari.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult EgitimDurumuCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> EgitimDurumuCreate(Ref_EgitimDurumu model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(EgitimDurumuList)); } return View(model); }
        public async Task<IActionResult> EgitimDurumuEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_EgitimDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> EgitimDurumuEdit(int id, Ref_EgitimDurumu model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(EgitimDurumuList)); } return View(model); }
        public async Task<IActionResult> EgitimDurumuDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_EgitimDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("EgitimDurumuDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> EgitimDurumuDeleteConfirmed(int id) { var model = await _context.Ref_EgitimDurumlari.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(EgitimDurumuList)); }
        #endregion

        #region HafizlikDurumu
        public async Task<IActionResult> HafizlikDurumuList() => View(await _context.Ref_HafizlikDurumlari.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult HafizlikDurumuCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> HafizlikDurumuCreate(Ref_HafizlikDurumu model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(HafizlikDurumuList)); } return View(model); }
        public async Task<IActionResult> HafizlikDurumuEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_HafizlikDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> HafizlikDurumuEdit(int id, Ref_HafizlikDurumu model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(HafizlikDurumuList)); } return View(model); }
        public async Task<IActionResult> HafizlikDurumuDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_HafizlikDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("HafizlikDurumuDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> HafizlikDurumuDeleteConfirmed(int id) { var model = await _context.Ref_HafizlikDurumlari.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(HafizlikDurumuList)); }
        #endregion

        #region KadroTuru
        public async Task<IActionResult> KadroTuruList() => View(await _context.Ref_KadroTurleri.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult KadroTuruCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> KadroTuruCreate(Ref_KadroTuru model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(KadroTuruList)); } return View(model); }
        public async Task<IActionResult> KadroTuruEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_KadroTurleri.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> KadroTuruEdit(int id, Ref_KadroTuru model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(KadroTuruList)); } return View(model); }
        public async Task<IActionResult> KadroTuruDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_KadroTurleri.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("KadroTuruDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> KadroTuruDeleteConfirmed(int id) { var model = await _context.Ref_KadroTurleri.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(KadroTuruList)); }
        #endregion

        #region AskerlikDurumu
        public async Task<IActionResult> AskerlikDurumuList() => View(await _context.Ref_AskerlikDurumlari.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult AskerlikDurumuCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> AskerlikDurumuCreate(Ref_AskerlikDurumu model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(AskerlikDurumuList)); } return View(model); }
        public async Task<IActionResult> AskerlikDurumuEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_AskerlikDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> AskerlikDurumuEdit(int id, Ref_AskerlikDurumu model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(AskerlikDurumuList)); } return View(model); }
        public async Task<IActionResult> AskerlikDurumuDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_AskerlikDurumlari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("AskerlikDurumuDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> AskerlikDurumuDeleteConfirmed(int id) { var model = await _context.Ref_AskerlikDurumlari.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(AskerlikDurumuList)); }
        #endregion

        #region KanGrubu
        public async Task<IActionResult> KanGrubuList() => View(await _context.Ref_KanGruplari.Where(x => x.IsDeleted == false).OrderBy(x => x.Ad).ToListAsync());
        public IActionResult KanGrubuCreate() => View();
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> KanGrubuCreate(Ref_KanGrubu model) { if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(KanGrubuList)); } return View(model); }
        public async Task<IActionResult> KanGrubuEdit(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_KanGruplari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost] [ValidateAntiForgeryToken] public async Task<IActionResult> KanGrubuEdit(int id, Ref_KanGrubu model) { if (id != model.Id) return NotFound(); if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(KanGrubuList)); } return View(model); }
        public async Task<IActionResult> KanGrubuDelete(int? id) { if (id == null) return NotFound(); var model = await _context.Ref_KanGruplari.FindAsync(id); if (model == null) return NotFound(); return View(model); }
        [HttpPost, ActionName("KanGrubuDelete")] [ValidateAntiForgeryToken] public async Task<IActionResult> KanGrubuDeleteConfirmed(int id) { var model = await _context.Ref_KanGruplari.FindAsync(id); if (model != null) { model.IsDeleted = true; _context.Update(model); await _context.SaveChangesAsync(); } return RedirectToAction(nameof(KanGrubuList)); }
        #endregion
    }
}
