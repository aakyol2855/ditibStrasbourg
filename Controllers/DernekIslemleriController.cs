using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Data;
using System.IO;
using MiniExcelLibs;
using System.Security.Claims;

namespace DitibStasbourg.Controllers
{
    public class DernekIslemleriController : Controller
    {
        private readonly IDernekIslemleriService _dernekService;
        private readonly IAssociationImportService _importService;
        private readonly ApplicationDbContext _context;

        public DernekIslemleriController(
            IDernekIslemleriService dernekService, 
            IAssociationImportService importService,
            ApplicationDbContext context)
        {
            _dernekService = dernekService;
            _importService = importService;
            _context = context;
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
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Kurum dernek)
        {
            ModelState.Remove("UstKurum");
            ModelState.Remove("Gorevlendirmeler");
            ModelState.Remove("DernekUyeleri");
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("YonetimKuruluUyeleri"))
                {
                    ModelState.Remove(key);
                }
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Deduplication Logic
                    var existingDernek = await _context.Kurum
                        .Where(k => k.Isim == dernek.Isim && k.Tip == KurumTip.Dernek)
                        .FirstOrDefaultAsync();

                    if (existingDernek != null)
                    {
                        bool isAbsoluteDuplicate = 
                            (!string.IsNullOrEmpty(dernek.Adres) && dernek.Adres == existingDernek.Adres) ||
                            (!string.IsNullOrEmpty(dernek.SiretNo) && dernek.SiretNo == existingDernek.SiretNo) ||
                            (!string.IsNullOrEmpty(dernek.RnaNo) && dernek.RnaNo == existingDernek.RnaNo) ||
                            (!string.IsNullOrEmpty(dernek.IbanNo) && dernek.IbanNo == existingDernek.IbanNo);

                        if (isAbsoluteDuplicate)
                        {
                            ModelState.AddModelError(string.Empty, "Sistemde birebir eşleşen bir dernek kaydı bulunmaktadır. Mükerrer giriş engellendi.");
                            goto ReturnView;
                        }
                        else
                        {
                            var logAlert = new SystemAuditLog {
                                UserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? "system",
                                LogType = "MükerrerTespiti_Uyarı",
                                Message = $"AKILLI UYARI: {User.Identity?.Name} tarafından girilen veri kümesinde benzerlik saptandı. Detay: Dernek ({dernek.Isim}) kaydı mevcut veri yapılarıyla %80 üzerinde benzerlik gösteriyor.",
                                Timestamp = DateTime.UtcNow,
                                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
                            };
                            _context.SystemAuditLogs.Add(logAlert);
                            // Do not block
                        }
                    }

                    await _dernekService.CreateDernekAsync(dernek);
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            
            ReturnView:
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
            return View(dernek);
        }

        [HttpPost]
        public async Task<IActionResult> CheckDuplicate([FromBody] Kurum dernek)
        {
            var existingDernek = await _context.Kurum
                .Where(k => k.Isim == dernek.Isim && k.Tip == KurumTip.Dernek)
                .FirstOrDefaultAsync();

            if (existingDernek != null)
            {
                bool isAbsoluteDuplicate = 
                    (!string.IsNullOrEmpty(dernek.Adres) && dernek.Adres == existingDernek.Adres) ||
                    (!string.IsNullOrEmpty(dernek.SiretNo) && dernek.SiretNo == existingDernek.SiretNo) ||
                    (!string.IsNullOrEmpty(dernek.RnaNo) && dernek.RnaNo == existingDernek.RnaNo) ||
                    (!string.IsNullOrEmpty(dernek.IbanNo) && dernek.IbanNo == existingDernek.IbanNo);

                if (isAbsoluteDuplicate)
                {
                    return Json(new { isDuplicate = true, type = "absolute", message = "Sistemde birebir eşleşen bir dernek kaydı (Adres/SIRET/RNA/IBAN aynı) bulunmaktadır. İşlem engellenecektir." });
                }
                
                return Json(new { isDuplicate = true, type = "warning", message = "⚠️ DİKKAT: Girmekte olduğunuz bilgilere benzer kayıtlar sistemde mevcut. Eğer eminseniz işleme devam edebilirsiniz, sistem yöneticiye doğrulama bildirimi gönderecektir." });
            }

            return Json(new { isDuplicate = false });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            // Directly resolve finansal donemler — the service navigation property does not eagerly load this collection.
            ViewBag.FinansalDonemler = await _context.KurumFinansalDonemler
                .Where(fd => fd.KurumId == id)
                .OrderByDescending(fd => fd.Year)
                .ToListAsync();

            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.KurumTurleri = await _dernekService.GetUstKurumlarAsync();
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
            return View(dernek);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetay(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            ViewBag.KurumTurleri = await _dernekService.GetUstKurumlarAsync();
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
            return PartialView("_DernekDetayPartial", dernek);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dernek = await _dernekService.GetDernekDetayAsync(id);
            if (dernek == null) return NotFound();
            
            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
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
            foreach (var key in ModelState.Keys.ToList())
            {
                if (key.StartsWith("YonetimKuruluUyeleri"))
                {
                    ModelState.Remove(key);
                }
            }

            if (ModelState.IsValid)
            {
                await _dernekService.UpdateDernekAsync(
                    dernek.Id, dernek.Isim, dernek.Sehir, dernek.Adres, 
                    dernek.KurulusKanunu, dernek.BaskonsoloslukBolgesi, dernek.Bolge, 
                    dernek.CrmUyelikFormDurumu, dernek.UstKurumId, dernek.IletisimNumarasi, 
                    dernek.Maili, dernek.IbanNo, dernek.SiretNo, dernek.RnaNo, dernek.Latitude, dernek.Longitude,
                    dernek.CemaatCount, dernek.FrenchRegistrationName, dernek.YonetimKuruluUyeleri?.ToList());

                await _dernekService.UpdateBaskanAsync(dernek.Id, dernek.DernekBaskaniAd ?? "", dernek.DernekBaskaniIletisim ?? "", dernek.BaskanMail);
                await _dernekService.UpdateDinGorevlisiAsync(dernek.Id, dernek.DinGorevlisiAd ?? "", dernek.DinGorevlisiIletisim ?? "");

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Sehirler = await _dernekService.GetSehirlerAsync();
            ViewBag.UstKurumlar = await _dernekService.GetUstKurumlarAsync();
            ViewBag.YonetimRolleri = await _dernekService.GetYonetimRolleriAsync();
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

        [HttpGet]
        public async Task<IActionResult> ExportExcel(bool missingStaffOnly = false)
        {
            var today = DateTime.Today;
            var threeMonthsLater = today.AddMonths(3);

            var query = _context.Kurum
                .Include(k => k.Gorevlendirmeler)
                .ThenInclude(g => g.Gorevli)
                .Where(k => k.AktifMi && !k.IsDeleted);

            if (missingStaffOnly)
            {
                query = query.Where(k => 
                    !k.Gorevlendirmeler.Any(a => today >= a.Tarih && (!a.BitisTarihi.HasValue || today <= a.BitisTarihi)) ||
                    k.Gorevlendirmeler.Any(a => today >= a.Tarih && a.BitisTarihi.HasValue && today <= a.BitisTarihi && a.BitisTarihi.Value <= threeMonthsLater)
                );
            }

            var institutions = await query.ToListAsync();

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Kurum Listesi");

            var headerRow = ws.Row(1);
            headerRow.Cell(1).Value = "Kurum Adı";
            headerRow.Cell(2).Value = "Tipi";
            headerRow.Cell(3).Value = "Bölge";
            headerRow.Cell(4).Value = "Şehir";
            headerRow.Cell(5).Value = "Telefon";
            headerRow.Cell(6).Value = "E-posta";
            headerRow.Cell(7).Value = "Aktif Görevli";
            headerRow.Cell(8).Value = "Görev Durumu";
            headerRow.Cell(9).Value = "IBAN No";
            headerRow.Cell(10).Value = "SIRET No";
            headerRow.Cell(11).Value = "RNA No";

            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#198754");
            headerRow.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;

            int currentRow = 2;
            foreach (var k in institutions)
            {
                var activeAssignment = k.Gorevlendirmeler
                    .FirstOrDefault(a => today >= a.Tarih && (!a.BitisTarihi.HasValue || today <= a.BitisTarihi));

                string activeGorevliName = activeAssignment?.Gorevli?.AdSoyad ?? "-";
                string statusText = "Görevli Mevcut";
                var statusColor = ClosedXML.Excel.XLColor.FromHtml("#D1E7DD");

                if (activeAssignment == null)
                {
                    statusText = "Görevli Yok";
                    statusColor = ClosedXML.Excel.XLColor.FromHtml("#F8D7DA");
                }
                else if (activeAssignment.BitisTarihi.HasValue && activeAssignment.BitisTarihi.Value <= threeMonthsLater)
                {
                    int daysLeft = (activeAssignment.BitisTarihi.Value - today).Days;
                    statusText = $"Görev Süresi Azalıyor ({daysLeft} Gün Kaldı)";
                    statusColor = ClosedXML.Excel.XLColor.FromHtml("#FFF3CD");
                }

                ws.Cell(currentRow, 1).Value = k.Isim;
                ws.Cell(currentRow, 2).Value = k.Tip.ToString();
                ws.Cell(currentRow, 3).Value = k.Bolge ?? "";
                ws.Cell(currentRow, 4).Value = k.Sehir ?? "";
                ws.Cell(currentRow, 5).Value = k.IletisimNumarasi ?? "";
                ws.Cell(currentRow, 6).Value = k.Maili ?? "";
                ws.Cell(currentRow, 7).Value = activeGorevliName;
                ws.Cell(currentRow, 8).Value = statusText;
                ws.Cell(currentRow, 9).Value  = k.IbanNo  ?? "";
                ws.Cell(currentRow, 10).Value = k.SiretNo ?? "";
                ws.Cell(currentRow, 11).Value = k.RnaNo   ?? "";

                ws.Cell(currentRow, 7).Style.Fill.BackgroundColor = statusColor;
                ws.Cell(currentRow, 8).Style.Fill.BackgroundColor = statusColor;

                currentRow++;
            }

            ws.Columns().AdjustToContents();

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            var content = memoryStream.ToArray();

            string suffix = missingStaffOnly ? "_Kadro_Acigi" : "_Listesi";
            string fileName = $"DITIB_Kurum{suffix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
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
        public async Task<IActionResult> AddBoardMember(int kurumId, string fullName, string? contactPhone, int yonetimRolId)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["Error"] = "Ad Soyad alanı zorunludur.";
                return RedirectToAction(nameof(Details), new { id = kurumId });
            }

            var member = new KurumYonetimKuruluUyesi
            {
                KurumId      = kurumId,
                FullName     = fullName.Trim(),
                ContactPhone = contactPhone?.Trim(),
                YonetimRolId = yonetimRolId,
                IsDeleted    = false
            };

            _context.KurumYonetimKuruluUyeleri.Add(member);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{fullName} yönetim kuruluna eklendi.";
            return RedirectToAction(nameof(Details), new { id = kurumId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBoardMember(int id, int kurumId)
        {
            var member = await _context.KurumYonetimKuruluUyeleri.FindAsync(id);
            if (member != null)
            {
                member.IsDeleted = true;
                _context.Update(member);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Details), new { id = kurumId });
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
            string? iletisimNumarasi, string? maili, string? ibanNo, string? siretNo, string? rnaNo, double? latitude, double? longitude, int? cemaatCount, string? frenchRegistrationName)
        {
            var success = await _dernekService.UpdateDernekAsync(id, isim, sehir, adres, kurulusKanunu, baskonsoloslukBolgesi, 
                bolge, crmUyelikFormDurumu, ustKurumId, iletisimNumarasi, maili, ibanNo, siretNo, rnaNo, latitude, longitude, cemaatCount, frenchRegistrationName, null);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFinancial(KurumFinansalDonem fd, [FromServices] ApplicationDbContext context)
        {
            ModelState.Remove("Kurum");
            if (ModelState.IsValid)
            {
                context.KurumFinansalDonemler.Add(fd);
                await context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Geçersiz form verisi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFinancial(int id, [FromServices] ApplicationDbContext context)
        {
            var fd = await context.KurumFinansalDonemler.FindAsync(id);
            if (fd != null)
            {
                context.KurumFinansalDonemler.Remove(fd);
                await context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        // ── Document Management System (DMS) ─────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadDocument(int kurumId, string documentName, string category,
            string? description, DateTime? expirationDate, IFormFile file, [FromServices] ApplicationDbContext context)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });

            var kurum = await context.Kurum.FindAsync(kurumId);
            if (kurum == null) return Json(new { success = false, message = "Kurum bulunamadı." });

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents", kurumId.ToString());
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var safeFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsFolder, safeFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var doc = new KurumDocument
                {
                    KurumId = kurumId,
                    DocumentName = documentName,
                    Category = category,
                    Description = description,
                    FilePath = $"/uploads/documents/{kurumId}/{safeFileName}",
                    FileSizeKb = file.Length / 1024,
                    ExpirationDate = expirationDate,
                    UploadedBy = User.Identity?.Name ?? "System"
                };

                context.KurumDocuments.Add(doc);
                await context.SaveChangesAsync();
                return Json(new { success = true, message = "Belge başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya yükleme hatası: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDocuments(int kurumId, [FromServices] ApplicationDbContext context)
        {
            var documents = await context.KurumDocuments
                .Where(d => d.KurumId == kurumId)
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.DocumentName,
                    d.Category,
                    d.Description,
                    d.FilePath,
                    d.FileSizeKb,
                    ExpirationDate = d.ExpirationDate.HasValue ? d.ExpirationDate.Value.ToString("dd.MM.yyyy") : null,
                    d.UploadedBy,
                    UploadedAt = d.UploadedAt.ToString("dd.MM.yyyy HH:mm"),
                    IsExpiringSoon = d.ExpirationDate.HasValue && d.ExpirationDate.Value <= DateTime.Today.AddMonths(3)
                })
                .ToListAsync();

            return Json(documents);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDocument(int documentId, [FromServices] ApplicationDbContext context)
        {
            var doc = await context.KurumDocuments.FindAsync(documentId);
            if (doc == null) return Json(new { success = false, message = "Belge bulunamadı." });

            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Belge arşivden silindi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDernekNot(int dernekId, string content, DateTime? bitisTarihi)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Not içeriği boş olamaz." });
            }

            var note = new DernekNot
            {
                DernekId = dernekId,
                NotIcerigi = content,
                BitisTarihi = bitisTarihi,
                EkleyenKullanici = User.Identity?.Name ?? "Sistem Yöneticisi",
                KayitTarihi = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.DernekNotlari.Add(note);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Not başarıyla eklendi." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDernekNot(int id)
        {
            var note = await _context.DernekNotlari.FindAsync(id);
            if (note == null) return Json(new { success = false, message = "Not bulunamadı." });

            note.IsDeleted = true;
            note.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Not silindi." });
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDernekGorsel(int dernekId, string? description, DernekGorselTipi type, IFormFile file, [FromServices] IDocumentStorageService storageService)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir görsel seçin." });

            var existingCount = await _context.DernekGorselleri.CountAsync(g => g.DernekId == dernekId && !g.IsDeleted);
            if (existingCount >= 5)
            {
                return Json(new { success = false, message = "En fazla 5 görsel yükleyebilirsiniz. Yeni yüklemek için lütfen önce eski bir görseli silin." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return Json(new { success = false, message = "Sadece JPG, JPEG veya PNG yükleyebilirsiniz." });

            try
            {
                var relativePath = await storageService.UploadAsync(file, $"dernekler/{dernekId}");
                var gorsel = new DernekGorsel
                {
                    DernekId = dernekId,
                    GorselYolu = relativePath,
                    Aciklama = description,
                    GorselTipi = type,
                    YuklenmeTarihi = DateTime.UtcNow,
                    YukleyenKullanici = User.Identity?.Name ?? "Sistem Yöneticisi",
                    IsDeleted = false
                };

                _context.DernekGorselleri.Add(gorsel);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Görsel başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Görsel yükleme hatası: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDernekGorsel(int id)
        {
            var gorsel = await _context.DernekGorselleri.FindAsync(id);
            if (gorsel == null) return Json(new { success = false, message = "Görsel bulunamadı." });

            gorsel.IsDeleted = true;
            gorsel.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Görsel silindi." });
        }
    }
}
