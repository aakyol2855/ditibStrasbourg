using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace DitibStasbourg.Controllers
{
    public class GorevliController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Services.ILookupService _lookupService;

        public GorevliController(ApplicationDbContext context, Services.ILookupService lookupService)
        {
            _context = context;
            _lookupService = lookupService;
        }

        // AJAX endpoint for staff autocomplete search
        [HttpGet]
        public async Task<IActionResult> SearchStaff(string term)
        {
            if (string.IsNullOrEmpty(term))
                return Json(new List<object>());

            var staff = await _context.Gorevli
                .Where(g => g.Ad.Contains(term) || g.Soyad.Contains(term) || (g.Email != null && g.Email.Contains(term)))
                .OrderBy(g => g.Ad)
                .ThenBy(g => g.Soyad)
                .Take(20)
                .Select(g => new {
                    id = g.Id,
                    text = $"{g.Ad} {g.Soyad}" + (string.IsNullOrEmpty(g.Email) ? "" : $" ({g.Email})")
                })
                .ToListAsync();

            return Json(staff);
        }

        public async Task<IActionResult> Index(GorevliFilterViewModel filter, int page = 1)
        {
            // Populate dropdowns for filter
            await PrepareFilterDropdowns(filter);
            
            var query = _context.Gorevli
                .Include(g => g.Gorevlendirmeler)
                .ThenInclude(gr => gr.Kurum)
                .Include(g => g.GorevliDurumBilgisi)
                .Include(g => g.SozlesmeTip)
                .Include(g => g.Unvan)
                .Include(g => g.EgitimDurumu)
                .Include(g => g.HafizlikDurumu)
                .Include(g => g.KadroTuru)
                .Include(g => g.AskerlikDurumu)
                .Include(g => g.KanGrubu)
                .Include(g => g.GorevliNotlari) // Include Notes
                .AsQueryable();

            // Filter Logic
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(s => s.Ad.Contains(filter.SearchString)
                                       || s.Soyad.Contains(filter.SearchString)
                                       || s.Email.Contains(filter.SearchString));
            }

            // Filter by selected staff IDs (from autocomplete multiselect)
            if (filter.StaffIds != null && filter.StaffIds.Any())
            {
                query = query.Where(s => filter.StaffIds.Contains(s.Id));
            }

            if (filter.GorevliDurumIds != null && filter.GorevliDurumIds.Any())
            {
                query = query.Where(s => s.GorevliDurumId.HasValue && filter.GorevliDurumIds.Contains(s.GorevliDurumId.Value));
            }

            if (filter.SozlesmeTipId.HasValue)
            {
                query = query.Where(s => s.SozlesmeTipId == filter.SozlesmeTipId);
            }

            if (filter.KurumId.HasValue)
            {
                // Filter by ACTIVE assignment in this Kurum
                query = query.Where(s => s.Gorevlendirmeler.Any(g => g.KurumId == filter.KurumId 
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            }
            
            if (!string.IsNullOrEmpty(filter.Sehir))
            {
                 // Filter by ACTIVE assignment in city
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Kurum != null && g.Kurum.Sehir == filter.Sehir
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            }

            if (!string.IsNullOrEmpty(filter.Cinsiyet))
            {
                query = query.Where(s => s.Cinsiyet == filter.Cinsiyet);
            }

            if (filter.IsActive.HasValue)
            {
                // isActive logic: Has any assignment that is currently active or future? 
                // Using exact definition of isActive property:
                // Gorevlendirmeler.Any(g => g.BaslangicTarihi <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now))
                if (filter.IsActive.Value)
                {
                    query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
                }
                else
                {
                     query = query.Where(s => !s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
                }
            }
            
            // Note: Date Range filtering? "Tarih Aralığı". 
            // Interpret as: Has assignment starting in range? Or currently active in range?
            // Let's implement: Has assignment starting >= Start and ending <= End (or null).
            if (filter.TarihBaslangic.HasValue)
            {
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih >= filter.TarihBaslangic.Value));
            }
            if (filter.TarihBitis.HasValue)
            {
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.BitisTarihi <= filter.TarihBitis.Value));
            }

            // New Filters Logic
            if (filter.UnvanIds != null && filter.UnvanIds.Any())
                query = query.Where(s => s.UnvanId.HasValue && filter.UnvanIds.Contains(s.UnvanId.Value));
            
            if (filter.EgitimDurumuIds != null && filter.EgitimDurumuIds.Any())
                query = query.Where(s => s.EgitimDurumuId.HasValue && filter.EgitimDurumuIds.Contains(s.EgitimDurumuId.Value));

            if (filter.HafizlikDurumuIds != null && filter.HafizlikDurumuIds.Any())
                query = query.Where(s => s.HafizlikDurumuId.HasValue && filter.HafizlikDurumuIds.Contains(s.HafizlikDurumuId.Value));

            if (filter.KadroTuruIds != null && filter.KadroTuruIds.Any())
                query = query.Where(s => s.KadroTuruId.HasValue && filter.KadroTuruIds.Contains(s.KadroTuruId.Value));

            if (filter.AskerlikDurumuIds != null && filter.AskerlikDurumuIds.Any())
                query = query.Where(s => s.AskerlikDurumuId.HasValue && filter.AskerlikDurumuIds.Contains(s.AskerlikDurumuId.Value));

            if (filter.KanGrubuIds != null && filter.KanGrubuIds.Any())
                query = query.Where(s => s.KanGrubuId.HasValue && filter.KanGrubuIds.Contains(s.KanGrubuId.Value));

            if (!string.IsNullOrEmpty(filter.BabaAdi))
                query = query.Where(s => s.BabaAdi != null && s.BabaAdi.Contains(filter.BabaAdi));

            if (!string.IsNullOrEmpty(filter.AnneAdi))
                query = query.Where(s => s.AnneAdi != null && s.AnneAdi.Contains(filter.AnneAdi));

            if (!string.IsNullOrEmpty(filter.DogumYeri))
                query = query.Where(s => s.DogumYeri != null && s.DogumYeri.Contains(filter.DogumYeri));

            if (filter.DogumTarihiBaslangic.HasValue)
                query = query.Where(s => s.DogumTarihi >= filter.DogumTarihiBaslangic.Value);

            if (filter.DogumTarihiBitis.HasValue)
                query = query.Where(s => s.DogumTarihi <= filter.DogumTarihiBitis.Value);

             if (!string.IsNullOrEmpty(filter.CepTelefonu))
                query = query.Where(s => s.CepTelefonu != null && s.CepTelefonu.Contains(filter.CepTelefonu));


            // Sorting
            ViewData["CurrentSort"] = filter.SortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(filter.SortOrder) ? "name_desc" : "";
            ViewData["StatusSortParm"] = filter.SortOrder == "Status" ? "status_desc" : "Status";
            // Date sort omitted for simplicity in this pass, can be re-added
            
            switch (filter.SortOrder)
            {
                case "name_desc":
                    query = query.OrderByDescending(s => s.Ad);
                    break;
                case "Status":
                    // Sort by dynamic status name? Or Order?
                    query = query.OrderBy(s => s.GorevliDurumBilgisi != null ? s.GorevliDurumBilgisi.Sira : 999);
                    break;
                case "status_desc":
                    query = query.OrderByDescending(s => s.GorevliDurumBilgisi != null ? s.GorevliDurumBilgisi.Sira : 999);
                    break;
                default:
                    query = query.OrderBy(s => s.Ad);
                    break;
            }

            // Pagination
            int pageSize = 10;
            filter.PageNumber ??= 1;
            
            // Pass the filter back to the view
            ViewData["Filter"] = filter;
            
            return View(await PaginatedList<Gorevli>.CreateAsync(query.AsNoTracking(), filter.PageNumber.Value, pageSize));
        }

        // GET: Gorevli/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gorevli = await _context.Gorevli
                .Include(g => g.Gorevlendirmeler)
                .ThenInclude(gr => gr.Kurum)
                .Include(g => g.GorevGecmisleri)
                .ThenInclude(gg => gg.Kurum)
                .Include(g => g.GorevliNotlari)
                .Include(g => g.GorevliDurumBilgisi)
                .Include(g => g.SozlesmeTip)
                .Include(g => g.Unvan)
                .Include(g => g.EgitimDurumu)
                .Include(g => g.HafizlikDurumu)
                .Include(g => g.KadroTuru)
                .Include(g => g.AskerlikDurumu)
                .Include(g => g.KanGrubu)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gorevli == null)
            {
                return NotFound();
            }

            return View(gorevli);
        }

        // GET: Gorevli/Create
        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return View();
        }

        // POST: Gorevli/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gorevli gorevli, string? IlkNot)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gorevli);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(IlkNot))
                {
                    var not = new GorevliNot
                    {
                        GorevliId = gorevli.Id,
                        NotIcerik = IlkNot,
                        Tarih = DateTime.Now,
                        YazanKisiId = User.Identity?.Name
                    };
                    _context.Add(not);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns(gorevli);
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
            await PrepareDropdowns(gorevli);
            return View(gorevli);
        }

        // POST: Gorevli/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Gorevli gorevli, string? YeniNot)
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

                    if (!string.IsNullOrWhiteSpace(YeniNot))
                    {
                          var not = new GorevliNot
                        {
                            GorevliId = gorevli.Id,
                            NotIcerik = YeniNot,
                            Tarih = DateTime.Now,
                            YazanKisiId = User.Identity?.Name
                        };
                        _context.Add(not);
                        await _context.SaveChangesAsync();
                    }
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
            await PrepareDropdowns(gorevli);
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

        // Export to Excel with filters
        public async Task<IActionResult> ExportToExcel(GorevliFilterViewModel filter)
        {
            var query = _context.Gorevli
                .Include(g => g.Gorevlendirmeler)
                .ThenInclude(gr => gr.Kurum)
                .Include(g => g.GorevliDurumBilgisi)
                .Include(g => g.SozlesmeTip)
                .Include(g => g.Unvan)
                .Include(g => g.EgitimDurumu)
                .Include(g => g.HafizlikDurumu)
                .Include(g => g.KadroTuru)
                .Include(g => g.AskerlikDurumu)
                .Include(g => g.KanGrubu)
                .AsQueryable();

            // Apply same filtering logic as Index
            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(s => s.Ad.Contains(filter.SearchString)
                                       || s.Soyad.Contains(filter.SearchString)
                                       || s.Email.Contains(filter.SearchString));
            }

            // Filter by selected staff IDs (from autocomplete multiselect)
            if (filter.StaffIds != null && filter.StaffIds.Any())
            {
                query = query.Where(s => filter.StaffIds.Contains(s.Id));
            }

            if (filter.GorevliDurumIds != null && filter.GorevliDurumIds.Any())
            {
                query = query.Where(s => s.GorevliDurumId.HasValue && filter.GorevliDurumIds.Contains(s.GorevliDurumId.Value));
            }

            if (filter.SozlesmeTipId.HasValue)
            {
                query = query.Where(s => s.SozlesmeTipId == filter.SozlesmeTipId);
            }

            if (filter.KurumId.HasValue)
            {
                query = query.Where(s => s.Gorevlendirmeler.Any(g => g.KurumId == filter.KurumId 
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            }
            
            if (!string.IsNullOrEmpty(filter.Sehir))
            {
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Kurum != null && g.Kurum.Sehir == filter.Sehir
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            }

            if (!string.IsNullOrEmpty(filter.Cinsiyet))
            {
                query = query.Where(s => s.Cinsiyet == filter.Cinsiyet);
            }

            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                {
                    query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
                }
                else
                {
                     query = query.Where(s => !s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
                }
            }

            if (filter.UnvanIds != null && filter.UnvanIds.Any())
                query = query.Where(s => s.UnvanId.HasValue && filter.UnvanIds.Contains(s.UnvanId.Value));
            
            if (filter.EgitimDurumuIds != null && filter.EgitimDurumuIds.Any())
                query = query.Where(s => s.EgitimDurumuId.HasValue && filter.EgitimDurumuIds.Contains(s.EgitimDurumuId.Value));

            if (filter.HafizlikDurumuIds != null && filter.HafizlikDurumuIds.Any())
                query = query.Where(s => s.HafizlikDurumuId.HasValue && filter.HafizlikDurumuIds.Contains(s.HafizlikDurumuId.Value));

            if (filter.KadroTuruIds != null && filter.KadroTuruIds.Any())
                query = query.Where(s => s.KadroTuruId.HasValue && filter.KadroTuruIds.Contains(s.KadroTuruId.Value));

            if (filter.AskerlikDurumuIds != null && filter.AskerlikDurumuIds.Any())
                query = query.Where(s => s.AskerlikDurumuId.HasValue && filter.AskerlikDurumuIds.Contains(s.AskerlikDurumuId.Value));

            if (filter.KanGrubuIds != null && filter.KanGrubuIds.Any())
                query = query.Where(s => s.KanGrubuId.HasValue && filter.KanGrubuIds.Contains(s.KanGrubuId.Value));

            var gorevliler = await query.OrderBy(g => g.Ad).ToListAsync();

            // Create Excel file
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Görevliler");
            
            // Headers
            worksheet.Cell(1, 1).Value = "Ad";
            worksheet.Cell(1, 2).Value = "Soyad";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Cinsiyet";
            worksheet.Cell(1, 5).Value = "Ünvan";
            worksheet.Cell(1, 6).Value = "Görevli Durumu";
            worksheet.Cell(1, 7).Value = "Sözleşme Tipi";
            worksheet.Cell(1, 8).Value = "TC Kimlik No";
            worksheet.Cell(1, 9).Value = "Cep Telefonu";
            worksheet.Cell(1, 10).Value = "Eğitim Durumu";
            worksheet.Cell(1, 11).Value = "Hafızlık";
            worksheet.Cell(1, 12).Value = "Son Görev Yeri";
            worksheet.Cell(1, 13).Value = "İlk Görev Tarihi";
            worksheet.Cell(1, 14).Value = "Son Bitiş Tarihi";
            worksheet.Cell(1, 15).Value = "Aktif mi?";
            
            // Style header
            var headerRange = worksheet.Range(1, 1, 1, 15);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
            // Data
            int row = 2;
            foreach (var gorevli in gorevliler)
            {
                var lastAssignment = gorevli.Gorevlendirmeler.OrderByDescending(g => g.Tarih).FirstOrDefault();
                var earliestStart = gorevli.Gorevlendirmeler.OrderBy(g => g.Tarih).FirstOrDefault()?.Tarih;
                var latestEnd = gorevli.Gorevlendirmeler.OrderByDescending(g => g.BitisTarihi).FirstOrDefault()?.BitisTarihi;
                
                worksheet.Cell(row, 1).Value = gorevli.Ad;
                worksheet.Cell(row, 2).Value = gorevli.Soyad;
                worksheet.Cell(row, 3).Value = gorevli.Email ?? "";
                worksheet.Cell(row, 4).Value = gorevli.Cinsiyet == "E" ? "Erkek" : gorevli.Cinsiyet == "K" ? "Kadın" : "";
                worksheet.Cell(row, 5).Value = gorevli.Unvan?.Ad ?? "";
                worksheet.Cell(row, 6).Value = gorevli.GorevliDurumBilgisi?.Ad ?? "";
                worksheet.Cell(row, 7).Value = gorevli.SozlesmeTip?.Ad ?? "";
                worksheet.Cell(row, 8).Value = gorevli.TCKimlikNo ?? "";
                worksheet.Cell(row, 9).Value = gorevli.CepTelefonu ?? "";
                worksheet.Cell(row, 10).Value = gorevli.EgitimDurumu?.Ad ?? "";
                worksheet.Cell(row, 11).Value = gorevli.HafizlikDurumu?.Ad ?? "";
                worksheet.Cell(row, 12).Value = lastAssignment?.Kurum?.Isim ?? "-";
                worksheet.Cell(row, 13).Value = earliestStart?.ToShortDateString() ?? "-";
                worksheet.Cell(row, 14).Value = latestEnd?.ToShortDateString() ?? "-";
                worksheet.Cell(row, 15).Value = gorevli.isActive ? "Evet" : "Hayır";
                
                row++;
            }
            
            // AutoFit columns
            worksheet.Columns().AdjustToContents();
            
            // Return file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();
            var fileName = $"Gorevliler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private bool GorevliExists(int id)
        {
            return _context.Gorevli.Any(e => e.Id == id);
        }

        private async Task PrepareDropdowns(Gorevli? gorevli = null)
        {
            ViewBag.Durumlar = await _lookupService.GetGorevliDurumlariAsync();
            ViewBag.SozlesmeTipleri = await _lookupService.GetSozlesmeTipleriAsync();
            ViewBag.Unvanlar = await _lookupService.GetUnvanlarAsync();
            ViewBag.EgitimDurumlari = await _lookupService.GetEgitimDurumlariAsync();
            ViewBag.HafizlikDurumlari = await _lookupService.GetHafizlikDurumlariAsync();
            ViewBag.KadroTurleri = await _lookupService.GetKadroTurleriAsync();
            ViewBag.AskerlikDurumlari = await _lookupService.GetAskerlikDurumlariAsync();
            ViewBag.KanGruplari = await _lookupService.GetKanGruplariAsync();
            
            // Only need selected values if gorevli is not null, but ViewBags are lists, selection happens in View
        }

        private async Task PrepareFilterDropdowns(Models.ViewModels.GorevliFilterViewModel filter)
        {
            ViewBag.Durumlar = await _lookupService.GetGorevliDurumlariAsync();
            ViewBag.SozlesmeTipleri = await _lookupService.GetSozlesmeTipleriAsync();
            ViewBag.Kurumlar = await _context.Kurum.OrderBy(k => k.Isim).ToListAsync();
            // Get Cities for dropdown
            ViewBag.Sehirler = await _context.Kurum.Select(k => k.Sehir).Distinct().OrderBy(s => s).ToListAsync();
            
            ViewBag.Unvanlar = await _lookupService.GetUnvanlarAsync();
            ViewBag.EgitimDurumlari = await _lookupService.GetEgitimDurumlariAsync();
            ViewBag.HafizlikDurumlari = await _lookupService.GetHafizlikDurumlariAsync();
            ViewBag.KadroTurleri = await _lookupService.GetKadroTurleriAsync();
            ViewBag.AskerlikDurumlari = await _lookupService.GetAskerlikDurumlariAsync();
            ViewBag.KanGruplari = await _lookupService.GetKanGruplariAsync();
        }

        // Note Management
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int gorevliId, string notIcerik)
        {
            try
            {
                var not = new GorevliNot
                {
                    GorevliId = gorevliId,
                    NotIcerik = notIcerik,
                    Tarih = DateTime.Now
                };

                _context.GorevliNotlari.Add(not);
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
                var not = await _context.GorevliNotlari.FindAsync(id);
                if (not == null) return Json(new { success = false });

                _context.GorevliNotlari.Remove(not);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Excel Import for SuperAdmin
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Lütfen bir Excel dosyası seçin.";
                return View("Import");
            }

            var importResults = new List<string>();
            var errors = new List<string>();
            int successCount = 0;
            int errorCount = 0;

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(1); // Skip header

                        // Flexible column mapping - find columns by name
                        var headerRow = worksheet.Row(1);
                        var columnMap = new Dictionary<string, int>();
                        
                        for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
                        {
                            var headerValue = headerRow.Cell(col).GetString().Trim().ToLower();
                            columnMap[headerValue] = col;
                        }

                        // Helper function to get cell value
                        string GetCellValue(IXLRow row, params string[] possibleNames)
                        {
                            foreach (var name in possibleNames)
                            {
                                if (columnMap.TryGetValue(name.ToLower(), out int colIndex))
                                {
                                    return row.Cell(colIndex).GetString().Trim();
                                }
                            }
                            return null;
                        }

                        DateTime? GetDateValue(IXLRow row, params string[] possibleNames)
                        {
                            foreach (var name in possibleNames)
                            {
                                if (columnMap.TryGetValue(name.ToLower(), out int colIndex))
                                {
                                    var cell = row.Cell(colIndex);
                                    if (cell.TryGetValue(out DateTime date))
                                        return date;
                                }
                            }
                            return null;
                        }

                        foreach (var row in rows)
                        {
                            try
                            {
                                var ad = GetCellValue(row, "ad", "adı", "isim", "name", "first name");
                                var soyad = GetCellValue(row, "soyad", "soyadı", "surname", "last name");

                                if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad))
                                {
                                    errors.Add($"Satır {row.RowNumber()}: Ad ve Soyad zorunludur");
                                    errorCount++;
                                    continue;
                                }

                                var gorevli = new Gorevli
                                {
                                    Ad = ad,
                                    Soyad = soyad,
                                    Email = GetCellValue(row, "email", "e-posta", "eposta", "e-mail"),
                                    TCKimlikNo = GetCellValue(row, "tc", "tc kimlik no", "tckimlikno", "tc no"),
                                    Cinsiyet = GetCellValue(row, "cinsiyet", "gender", "sex")?.ToUpper().Substring(0, 1),
                                    CepTelefonu = GetCellValue(row, "cep telefonu", "telefon", "cep", "phone", "mobile"),
                                    EvTelefonu = GetCellValue(row, "ev telefonu", "ev tel", "home phone"),
                                    Adres = GetCellValue(row, "adres", "address"),
                                    BabaAdi = GetCellValue(row, "baba adı", "babaadi", "father name", "baba"),
                                    AnneAdi = GetCellValue(row, "anne adı", "anneadi", "mother name", "anne"),
                                    DogumYeri = GetCellValue(row, "doğum yeri", "dogumyeri", "birth place"),
                                    DogumTarihi = GetDateValue(row, "doğum tarihi", "dogumtarihi", "birth date"),
                                    IlkGoreveBaslamaTarihi = GetDateValue(row, "ilk görev tarihi", "başlama tarihi", "hire date"),
                                    DiyanetGirisTarihi = GetDateValue(row, "diyanet giriş", "diyanet tarihi"),
                                    EmeklilikTarihi = GetDateValue(row, "emeklilik tarihi", "emeklilik", "retirement date"),
                                    MezuniyetOkul = GetCellValue(row, "mezuniyet okul", "okul", "school"),
                                    MezuniyetBolum = GetCellValue(row, "mezuniyet bölüm", "bölüm", "department")
                                };

                                _context.Gorevli.Add(gorevli);
                                await _context.SaveChangesAsync();
                                
                                importResults.Add($"✓ {ad} {soyad} başarıyla eklendi");
                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Satır {row.RowNumber()}: {ex.Message}");
                                errorCount++;
                            }
                        }
                    }
                }

                ViewBag.SuccessCount = successCount;
                ViewBag.ErrorCount = errorCount;
                ViewBag.ImportResults = importResults;
                ViewBag.Errors = errors;
                ViewBag.Message = $"Import tamamlandı: {successCount} başarılı, {errorCount} hata";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Hata: {ex.Message}";
            }

            return View("Import");
        }
    }
}
