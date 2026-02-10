using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
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

        // GET: Gorevlendirme - Accordion based with filtering
        public async Task<IActionResult> Index(GorevlendirmeFilterViewModel filter, int page = 1)
        {
            await PrepareFilterDropdowns(filter);
            
            var query = _context.Gorevlendirme
                .Include(g => g.Gorevli)
                    .ThenInclude(gov => gov.GorevliDurumBilgisi)
                .Include(g => g.Gorevli)
                    .ThenInclude(gov => gov.GorevliNotlari)
                .Include(g => g.Kurum)
                .Include(g => g.YerineGelecekGorevli)
                .Include(g => g.GorevlendirmeNotlari)
                .AsQueryable();

            // Apply filters
            if (filter.GorevliId.HasValue)
            {
                query = query.Where(g => g.GorevliId == filter.GorevliId.Value);
            }

            if (filter.KurumId.HasValue)
            {
                query = query.Where(g => g.KurumId == filter.KurumId.Value);
            }

            if (filter.BaslangicTarihi.HasValue)
            {
                query = query.Where(g => g.Tarih >= filter.BaslangicTarihi.Value);
            }

            if (filter.BitisTarihi.HasValue)
            {
                query = query.Where(g => g.Tarih <= filter.BitisTarihi.Value);
            }

            if (!string.IsNullOrEmpty(filter.Sehir))
            {
                query = query.Where(g => g.Kurum.Sehir == filter.Sehir);
            }

            // DurumFilter: aktif, pasif, tümü
            var today = DateTime.Today;
            if (!string.IsNullOrEmpty(filter.DurumFilter))
            {
                if (filter.DurumFilter == "aktif")
                {
                    query = query.Where(g => g.Tarih <= today && (g.BitisTarihi == null || g.BitisTarihi >= today));
                }
                else if (filter.DurumFilter == "pasif")
                {
                    query = query.Where(g => g.BitisTarihi != null && g.BitisTarihi < today);
                }
                // "tümü" için filtre yok
            }

            query = query.OrderByDescending(g => g.Tarih);

            ViewData["Filter"] = filter;
            
            int pageSize = 20;
            var paginatedList = await PaginatedList<Gorevlendirme>.CreateAsync(query, page, pageSize);
            
            return View(paginatedList);
        }

        private async Task PrepareFilterDropdowns(GorevlendirmeFilterViewModel filter)
        {
            ViewBag.Gorevliler = await _context.Gorevli
                .OrderBy(g => g.Ad)
                .ThenBy(g => g.Soyad)
                .ToListAsync();
            
            ViewBag.Kurumlar = await _context.Kurum
                .OrderBy(k => k.Isim)
                .ToListAsync();
            
            ViewBag.Sehirler = await _context.Kurum
                .Where(k => k.Sehir != null)
                .Select(k => k.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
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
            ViewData["YerineGelecekGorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad");
            return View();
        }

        // POST: Gorevlendirme/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi,YerineGelecekGorevliId,YerineGelisPlanlananTarih,YerineGelisPlanlananBitisTarih")] Gorevlendirme gorevlendirme)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gorevlendirme);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.GorevliId);
            ViewData["KurumId"] = new SelectList(_context.Kurum, "Id", "Isim", gorevlendirme.KurumId);
            ViewData["YerineGelecekGorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.YerineGelecekGorevliId);
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
            ViewData["YerineGelecekGorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.YerineGelecekGorevliId);
            return View(gorevlendirme);
        }

        // POST: Gorevlendirme/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GorevliId,KurumId,Tarih,BitisTarihi,YerineGelecekGorevliId,YerineGelisPlanlananTarih,YerineGelisPlanlananBitisTarih")] Gorevlendirme gorevlendirme)
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
            ViewData["YerineGelecekGorevliId"] = new SelectList(_context.Gorevli, "Id", "AdSoyad", gorevlendirme.YerineGelecekGorevliId);
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

        // Note Management
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int gorevlendirmeId, string notIcerik)
        {
            try
            {
                var not = new GorevlendirmeNot
                {
                    GorevlendirmeId = gorevlendirmeId,
                    NotIcerik = notIcerik,
                    Tarih = DateTime.Now
                };

                _context.GorevlendirmeNotlari.Add(not);
                await _context.SaveChangesAsync();

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
                var not = await _context.GorevlendirmeNotlari.FindAsync(id);
                if (not == null) return Json(new { success = false });

                _context.GorevlendirmeNotlari.Remove(not);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
