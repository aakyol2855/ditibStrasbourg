using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using ClosedXML.Excel;

namespace DitibStasbourg.Controllers
{
    public class GorevlendirmeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GorevlendirmeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Gorevlendirme
        public async Task<IActionResult> Index(
            int? year, 
            KurumTip? tip, 
            int? gorevliId, 
            int? kurumId, 
            DateTime? startDate, 
            DateTime? endDate,
            string sortOrder,
            int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_asc" : "";
            
            var query = _context.Gorevlendirme
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .AsQueryable();

            if (year.HasValue) query = query.Where(g => g.Tarih.Year == year.Value);
            if (tip.HasValue) query = query.Where(g => g.Kurum.Tip == tip.Value);
            if (gorevliId.HasValue) query = query.Where(g => g.GorevliId == gorevliId.Value);
            if (kurumId.HasValue) query = query.Where(g => g.KurumId == kurumId.Value);
            if (startDate.HasValue) query = query.Where(g => g.Tarih >= startDate.Value);
            if (endDate.HasValue) query = query.Where(g => g.Tarih <= endDate.Value);

            switch (sortOrder)
            {
                case "date_asc":
                    query = query.OrderBy(g => g.Tarih);
                    break;
                default:
                    query = query.OrderByDescending(g => g.Tarih);
                    break;
            }

            ViewBag.Years = await _context.Gorevlendirme.Select(g => g.Tarih.Year).Distinct().OrderByDescending(y => y).ToListAsync();
            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevliId);
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim", kurumId);
            
            ViewData["CurrentYear"] = year;
            ViewData["CurrentTip"] = tip;
            ViewData["CurrentGorevliId"] = gorevliId;
            ViewData["CurrentKurumId"] = kurumId;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");

            int pageSize = 15;
            return View(await PaginatedList<Gorevlendirme>.CreateAsync(query.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Gorevlendirme/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _context.Gorevlendirme
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gorevlendirme == null) return NotFound();

            return View(gorevlendirme);
        }

        // GET: Gorevlendirme/Create
        public IActionResult Create()
        {
            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad");
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim");
            return View();
        }

        // POST: Gorevlendirme/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi")] Gorevlendirme gorevlendirme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gorevlendirme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.GorevliId);
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim", gorevlendirme.KurumId);
            return View(gorevlendirme);
        }

        // GET: Gorevlendirme/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _context.Gorevlendirme.FindAsync(id);
            if (gorevlendirme == null) return NotFound();

            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.GorevliId);
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim", gorevlendirme.KurumId);
            return View(gorevlendirme);
        }

        // POST: Gorevlendirme/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi")] Gorevlendirme gorevlendirme)
        {
            if (id != gorevlendirme.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gorevlendirme);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GorevlendirmeExists(gorevlendirme.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.GorevliId);
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim", gorevlendirme.KurumId);
            return View(gorevlendirme);
        }

        // GET: Gorevlendirme/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gorevlendirme = await _context.Gorevlendirme
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gorevlendirme == null) return NotFound();

            return View(gorevlendirme);
        }

        // POST: Gorevlendirme/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gorevlendirme = await _context.Gorevlendirme.FindAsync(id);
            if (gorevlendirme != null) _context.Gorevlendirme.Remove(gorevlendirme);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel(
            int? year, 
            KurumTip? tip, 
            int? gorevliId, 
            int? kurumId,
            DateTime? startDate, 
            DateTime? endDate,
            List<string> columns)
        {
            var query = _context.Gorevlendirme
                .Include(g => g.Gorevli)
                .Include(g => g.Kurum)
                .AsQueryable();

            if (year.HasValue) query = query.Where(g => g.Tarih.Year == year.Value);
            if (tip.HasValue) query = query.Where(g => g.Kurum.Tip == tip.Value);
            if (gorevliId.HasValue) query = query.Where(g => g.GorevliId == gorevliId.Value);
            if (kurumId.HasValue) query = query.Where(g => g.KurumId == kurumId.Value);
            if (startDate.HasValue) query = query.Where(g => g.Tarih >= startDate.Value);
            if (endDate.HasValue) query = query.Where(g => g.Tarih <= endDate.Value);

            var assignments = await query.OrderByDescending(g => g.Tarih).ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Görevlendirmeler");
                var currentRow = 1;

                // Headers
                int colIndex = 1;
                // If no columns selected, export all default
                if (columns == null || !columns.Any()) 
                {
                    columns = new List<string> { "BaslangicTarihi", "BitisTarihi", "Gorevli", "Kurum", "KurumTipi" };
                }

                if (columns.Contains("BaslangicTarihi")) worksheet.Cell(currentRow, colIndex++).Value = "Başlangıç Tarihi";
                if (columns.Contains("BitisTarihi")) worksheet.Cell(currentRow, colIndex++).Value = "Bitiş Tarihi";
                if (columns.Contains("Gorevli")) worksheet.Cell(currentRow, colIndex++).Value = "Görevli";
                if (columns.Contains("Kurum")) worksheet.Cell(currentRow, colIndex++).Value = "Kurum";
                if (columns.Contains("KurumTipi")) worksheet.Cell(currentRow, colIndex++).Value = "Kurum Tipi";
                if (columns.Contains("GorevliEmail")) worksheet.Cell(currentRow, colIndex++).Value = "Görevli Email";

                foreach (var item in assignments)
                {
                    currentRow++;
                    colIndex = 1;
                    if (columns.Contains("BaslangicTarihi")) worksheet.Cell(currentRow, colIndex++).Value = item.Tarih;
                    if (columns.Contains("BitisTarihi")) worksheet.Cell(currentRow, colIndex++).Value = item.BitisTarihi.HasValue ? item.BitisTarihi.Value : "Devam Ediyor"; // Or empty
                    if (columns.Contains("Gorevli")) worksheet.Cell(currentRow, colIndex++).Value = item.Gorevli?.AdSoyad;
                    if (columns.Contains("Kurum")) worksheet.Cell(currentRow, colIndex++).Value = item.Kurum?.Isim;
                    if (columns.Contains("KurumTipi")) worksheet.Cell(currentRow, colIndex++).Value = item.Kurum?.Tip.ToString();
                    if (columns.Contains("GorevliEmail")) worksheet.Cell(currentRow, colIndex++).Value = item.Gorevli?.Email;
                }
                
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Gorevlendirmeler_Ozel.xlsx");
                }
            }
        }

        private bool GorevlendirmeExists(int id)
        {
            return _context.Gorevlendirme.Any(e => e.Id == id);
        }
    }
}
