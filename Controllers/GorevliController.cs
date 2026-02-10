using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Controllers
{
    public class GorevliController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GorevliController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gorevli
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var gorevliler = _context.Gorevli
                .Include(g => g.Gorevlendirmeler)
                .ThenInclude(gr => gr.Kurum)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                gorevliler = gorevliler.Where(s => s.Ad.Contains(searchString)
                                       || s.Soyad.Contains(searchString)
                                       || s.Email.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    gorevliler = gorevliler.OrderByDescending(s => s.Ad);
                    break;
                case "Status":
                    gorevliler = gorevliler.OrderBy(s => s.Durum);
                    break;
                case "status_desc":
                    gorevliler = gorevliler.OrderByDescending(s => s.Durum);
                    break;
                case "Date":
                case "date_desc":
                     // Sorting by date is temporarily disabled or needs to be based on subquery if critical
                     // For now, default to name sort to prevent error
                    gorevliler = gorevliler.OrderBy(s => s.Ad);
                   break;
                default:
                    gorevliler = gorevliler.OrderBy(s => s.Ad);
                    break;
            }

            int pageSize = 10;
            return View(await PaginatedList<Gorevli>.CreateAsync(gorevliler.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Gorevli/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorevli = await _context.Gorevli
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gorevli == null)
            {
                return NotFound();
            }

            return View(gorevli);
        }

        // GET: Gorevli/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Gorevli/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Soyad,Email,Durum")] Gorevli gorevli)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gorevli);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(gorevli);
        }

        // GET: Gorevli/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorevli = await _context.Gorevli.FindAsync(id);
            if (gorevli == null)
            {
                return NotFound();
            }
            return View(gorevli);
        }

        // POST: Gorevli/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Email,Durum")] Gorevli gorevli)
        {
            if (id != gorevli.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gorevli);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GorevliExists(gorevli.Id))
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
            return View(gorevli);
        }

        // GET: Gorevli/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorevli = await _context.Gorevli
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gorevli == null)
            {
                return NotFound();
            }

            return View(gorevli);
        }

        // POST: Gorevli/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gorevli = await _context.Gorevli.FindAsync(id);
            if (gorevli != null)
            {
                _context.Gorevli.Remove(gorevli);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GorevliExists(int id)
        {
            return _context.Gorevli.Any(e => e.Id == id);
        }
    }
}
