using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Controllers
{
    public class KurumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KurumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Kurum
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kurum.ToListAsync());
        }

        // GET: Kurum/Details/5
        public async Task<IActionResult> Details(int? id)
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

        // GET: Kurum/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kurum/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Isim,Adres,Tip")] Kurum kurum)
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
            return View(kurum);
        }

        // POST: Kurum/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Isim,Adres,Tip")] Kurum kurum)
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
