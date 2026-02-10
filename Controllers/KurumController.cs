using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace DitibStasbourg.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class KurumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KurumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kurum
        public async Task<IActionResult> Index(KurumFilterViewModel filter, int page = 1)
        {
            await PrepareFilterDropdowns(filter);
            
            var query = _context.Kurum
                .Include(k => k.UstKurum)
                .Include(k => k.Gorevlendirmeler)
                    .ThenInclude(g => g.Gorevli)
                    .ThenInclude(gov => gov.GorevliDurumBilgisi)
                .Include(k => k.Gorevlendirmeler)
                    .ThenInclude(g => g.Gorevli)
                    .ThenInclude(gov => gov.GorevliNotlari)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(k => k.Isim.Contains(filter.SearchString) || 
                                        (k.Adres != null && k.Adres.Contains(filter.SearchString)));
            }

            if (filter.Tip.HasValue)
            {
                query = query.Where(k => k.Tip == filter.Tip.Value);
            }

            if (!string.IsNullOrEmpty(filter.Sehir))
            {
                query = query.Where(k => k.Sehir == filter.Sehir);
            }

            if (filter.AktifMi.HasValue)
            {
                query = query.Where(k => k.AktifMi == filter.AktifMi.Value);
            }

            if (filter.UstKurumId.HasValue)
            {
                query = query.Where(k => k.UstKurumId == filter.UstKurumId.Value);
            }

            query = query.OrderBy(k => k.Isim);

            ViewData["Filter"] = filter;
            
            int pageSize = 20;
            var paginatedList = await PaginatedList<Kurum>.CreateAsync(query, page, pageSize);
            
            return View(paginatedList);
        }

        private async Task PrepareFilterDropdowns(KurumFilterViewModel filter)
        {
            ViewBag.Sehirler = await _context.Kurum
                .Where(k => k.Sehir != null)
                .Select(k => k.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
            
            ViewBag.UstKurumlar = await _context.Ref_KurumTurus
                .Where(k => !k.IsDeleted)
                .OrderBy(k => k.Ad)
                .ToListAsync();
        }

        // GET: Kurum/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurum
                .Include(k => k.UstKurum)
                .Include(k => k.Gorevlendirmeler)
                .ThenInclude(g => g.Gorevli)
                .ThenInclude(gov => gov.GorevliDurumBilgisi)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kurum == null)
            {
                return NotFound();
            }

            return View(kurum);
        }

        // GET: Kurum/Create
        public IActionResult Create()
        {
            ViewData["UstKurumId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Ref_KurumTurus, "Id", "Ad");
            return View();
        }

        // POST: Kurum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Isim,Adres,Tip,Sehir,AktifMi,UstKurumId")] Kurum kurum)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kurum);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kurum);
        }

        // GET: Kurum/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurum.FindAsync(id);
            if (kurum == null)
            {
                return NotFound();
            }
            ViewData["UstKurumId"] = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Ref_KurumTurus, "Id", "Ad", kurum.UstKurumId);
            return View(kurum);
        }

        // POST: Kurum/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Isim,Adres,Tip,Sehir,AktifMi,UstKurumId")] Kurum kurum)
        {
            if (id != kurum.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kurum);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KurumExists(kurum.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(kurum);
        }

        // GET: Kurum/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kurum = await _context.Kurum
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kurum == null)
            {
                return NotFound();
            }

            return View(kurum);
        }

        // POST: Kurum/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kurum = await _context.Kurum.FindAsync(id);
            if (kurum != null)
            {
                _context.Kurum.Remove(kurum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KurumExists(int id)
        {
            return _context.Kurum.Any(e => e.Id == id);
        }
    }
}
