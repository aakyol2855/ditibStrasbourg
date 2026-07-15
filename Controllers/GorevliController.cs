using Microsoft.AspNetCore.Mvc;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using DitibStasbourg.Services;
using Microsoft.AspNetCore.Identity;
using DitibStasbourg.Data;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Controllers
{
    public class GorevliController : Controller
    {
        private readonly IGorevliService _gorevliService;
        private readonly ILookupService _lookupService;
        private readonly DitibStasbourg.Services.Base.IBaseService<Kurum> _kurumService;
        private readonly IDynamicExportService _exportService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISystemAuditLogService _auditService;
        private readonly IIzinHesaplamaService _izinEngine;

        public GorevliController(IGorevliService gorevliService, ILookupService lookupService, 
            DitibStasbourg.Services.Base.IBaseService<Kurum> kurumService,
            IDynamicExportService exportService,
            UserManager<IdentityUser> userManager,
            ISystemAuditLogService auditService,
            IIzinHesaplamaService izinEngine)
        {
            _gorevliService = gorevliService;
            _lookupService = lookupService;
            _kurumService = kurumService;
            _exportService = exportService;
            _userManager = userManager;
            _auditService = auditService;
            _izinEngine = izinEngine;
        }

        [HttpGet]
        public async Task<IActionResult> SearchStaff(string term)
        {
            var staff = await _gorevliService.SearchStaffAsync(term);
            return Json(staff);
        }

        [HttpGet]
        public async Task<IActionResult> GetContactInfo(int id)
        {
            var info = await _gorevliService.GetContactInfoAsync(id);
            if (info == null)
                return Json(new { phone = (string?)null, email = (string?)null });

            return Json(new { phone = info.Value.Phone, email = info.Value.Email });
        }

        public async Task<IActionResult> Index(GorevliFilterViewModel filter, string? sortOrder, int page = 1)
        {
            await PrepareFilterDropdowns();
            
            if (!string.IsNullOrEmpty(sortOrder))
            {
                filter.SortOrder = sortOrder;
            }
            
            int pageSize = filter.PageSize ?? 20;
            filter.PageNumber = filter.PageNumber ?? page;
            
            ViewData["CurrentSort"] = filter.SortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(filter.SortOrder) || filter.SortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["StatusSortParm"] = filter.SortOrder == "Status" ? "status_desc" : "Status";
            ViewData["DateSortParm"] = filter.SortOrder == "Date" ? "date_desc" : "Date";
            ViewData["ActiveSortParm"] = filter.SortOrder == "Active" ? "active_desc" : "Active";
            ViewData["Filter"] = filter;
            
            var paginatedList = await _gorevliService.GetFilteredGorevlilerAsync(filter, pageSize);
            return View(paginatedList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetGorevliDetailsAsync(id.Value);
            if (gorevli == null) return NotFound();

            var startDate = gorevli.FransaGirisTarihi 
                ?? gorevli.Gorevlendirmeler?.Where(gl => !gl.IsDeleted).OrderBy(gl => gl.Tarih).Select(gl => (DateTime?)gl.Tarih).FirstOrDefault();
            var totalAccrued = _izinEngine.CalculateTotalAccruedDays(startDate, null);
            var totalUsed = await _gorevliService.GetTotalUsedLeavesAsync(gorevli.Id);

            ViewBag.TotalAccrued = totalAccrued;
            ViewBag.TotalUsed = totalUsed;
            ViewBag.NetBalance = totalAccrued - totalUsed;

            return View(gorevli);
        }

        public async Task<IActionResult> Create()
        {
            await PrepareDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Gorevli gorevli, string? IlkNot, BelgeTipi? BelgeTipi, string? BelgeSeriNo, DateTime? BelgeExpirationDate, string? BelgeDescription, IFormFile? BelgeFile, [FromServices] IDocumentStorageService storageService, [FromServices] ApplicationDbContext context)
        {
            if (ModelState.IsValid)
            {
                // Deduplication Logic
                var existingMatches = await _gorevliService.CheckDuplicateMatchesAsync(gorevli.Ad, gorevli.Soyad, gorevli.TCKimlikNo, gorevli.Email);

                if (existingMatches.Any())
                {
                    bool isAbsoluteDuplicate = existingMatches.Any(g => 
                        (!string.IsNullOrEmpty(gorevli.TCKimlikNo) && g.TCKimlikNo == gorevli.TCKimlikNo) ||
                        (!string.IsNullOrEmpty(gorevli.Email) && g.Email == gorevli.Email));

                    if (isAbsoluteDuplicate)
                    {
                        ModelState.AddModelError(string.Empty, "Sistemde birebir eşleşen bir görevli kaydı (TC Kimlik / E-posta aynı) bulunmaktadır. Mükerrer giriş engellendi.");
                        await PrepareDropdowns();
                        return View(gorevli);
                    }
                    else
                    {
                        await _auditService.LogAsync(
                            "MükerrerTespiti_Uyarı",
                            User.Identity?.Name ?? "system",
                            $"AKILLI UYARI: {User.Identity?.Name} tarafından girilen veri kümesinde benzerlik saptandı. Detay: Görevli ({gorevli.Ad} {gorevli.Soyad}) kaydı mevcut veri yapılarıyla %80 üzerinde benzerlik gösteriyor.",
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                            "GorevliController");
                        // Do not block
                    }
                }

                await _gorevliService.AddAsync(gorevli);

                if (!string.IsNullOrWhiteSpace(IlkNot))
                {
                    var userId = _userManager.GetUserId(User) ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "00000000-0000-0000-0000-000000000000";
                    await _gorevliService.AddNoteAsync(gorevli.Id, IlkNot, userId);
                }

                if (BelgeFile != null && BelgeFile.Length > 0 && BelgeTipi.HasValue)
                {
                    try
                    {
                        var relativePath = await storageService.UploadAsync(BelgeFile, $"gorevli/{gorevli.Id}");
                        var doc = new GorevliBelge
                        {
                            GorevliId = gorevli.Id,
                            BelgeTipi = BelgeTipi.Value,
                            SeriNo = BelgeSeriNo?.Trim(),
                            GecerlilikTarihi = BelgeExpirationDate,
                            DosyaYolu = relativePath,
                            Aciklama = BelgeDescription?.Trim(),
                            YuklenmeTarihi = DateTime.UtcNow,
                            YukleyenKullanici = User.Identity?.Name ?? "Sistem Yöneticisi",
                            IsDeleted = false
                        };
                        context.GorevliBelgeleri.Add(doc);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Fallback gracefully
                        await _auditService.LogAsync(
                            "BelgeYuklemeHatasi_Create",
                            User.Identity?.Name ?? "system",
                            $"Gorevli oluşturulurken belge yüklenemedi: {ex.Message}",
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                            "GorevliController");
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns();
            return View(gorevli);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetByIdAsync(id.Value);
            if (gorevli == null) return NotFound();
            
            await PrepareDropdowns();
            return View(gorevli);
        }

        [HttpPost]
        public async Task<IActionResult> CheckDuplicate([FromBody] Gorevli gorevli)
        {
            var existingMatches = await _gorevliService.CheckDuplicateMatchesAsync(gorevli.Ad, gorevli.Soyad, gorevli.TCKimlikNo, gorevli.Email);

            if (existingMatches.Any())
            {
                bool isAbsoluteDuplicate = existingMatches.Any(g => 
                    (!string.IsNullOrEmpty(gorevli.TCKimlikNo) && g.TCKimlikNo == gorevli.TCKimlikNo) ||
                    (!string.IsNullOrEmpty(gorevli.Email) && g.Email == gorevli.Email));

                if (isAbsoluteDuplicate)
                {
                    return Json(new { isDuplicate = true, type = "absolute", message = "Sistemde birebir eşleşen bir görevli kaydı (TC Kimlik / E-posta aynı) bulunmaktadır. İşlem engellenecektir." });
                }
                
                return Json(new { isDuplicate = true, type = "warning", message = "⚠️ DİKKAT: Girmekte olduğunuz bilgilere benzer kayıtlar sistemde mevcut. Eğer eminseniz işleme devam edebilirsiniz, sistem yöneticiye doğrulama bildirimi gönderecektir." });
            }

            return Json(new { isDuplicate = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Gorevli gorevli, string? YeniNot, BelgeTipi? BelgeTipi, string? BelgeSeriNo, DateTime? BelgeExpirationDate, string? BelgeDescription, IFormFile? BelgeFile, [FromServices] IDocumentStorageService storageService, [FromServices] ApplicationDbContext context)
        {
            if (id != gorevli.Id) return NotFound();

            if (ModelState.IsValid)
            {
                await _gorevliService.UpdateAsync(gorevli);

                if (!string.IsNullOrWhiteSpace(YeniNot))
                {
                    var userId = _userManager.GetUserId(User) ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "00000000-0000-0000-0000-000000000000";
                    await _gorevliService.AddNoteAsync(gorevli.Id, YeniNot, userId);
                }

                if (BelgeFile != null && BelgeFile.Length > 0 && BelgeTipi.HasValue)
                {
                    try
                    {
                        var relativePath = await storageService.UploadAsync(BelgeFile, $"gorevli/{gorevli.Id}");
                        var doc = new GorevliBelge
                        {
                            GorevliId = gorevli.Id,
                            BelgeTipi = BelgeTipi.Value,
                            SeriNo = BelgeSeriNo?.Trim(),
                            GecerlilikTarihi = BelgeExpirationDate,
                            DosyaYolu = relativePath,
                            Aciklama = BelgeDescription?.Trim(),
                            YuklenmeTarihi = DateTime.UtcNow,
                            YukleyenKullanici = User.Identity?.Name ?? "Sistem Yöneticisi",
                            IsDeleted = false
                        };
                        context.GorevliBelgeleri.Add(doc);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        // Fallback gracefully
                        await _auditService.LogAsync(
                            "BelgeYuklemeHatasi_Edit",
                            User.Identity?.Name ?? "system",
                            $"Gorevli düzenlenirken belge yüklenemedi: {ex.Message}",
                            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                            "GorevliController");
                    }
                }
                
                return RedirectToAction(nameof(Index));
            }
            await PrepareDropdowns();
            return View(gorevli);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var gorevli = await _gorevliService.GetByIdAsync(id.Value);
            if (gorevli == null) return NotFound();

            return View(gorevli);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Doğrudan servis katmanını tetikliyoruz, o arkada her şeyi temizliyor
            await _gorevliService.DeleteAsync(id); 
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Route("Gorevli/BulkSoftDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkSoftDelete([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Lütfen silinecek görevlileri seçiniz." });
            }

            try
            {
                foreach (var id in ids)
                {
                    // Invoke the underlying core abstraction layer safely
                    await _gorevliService.DeleteAsync(id);
                }
                return Json(new { success = true, message = "Seçilen kayıtlar başarıyla işlemden geçirildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel veri bütünlüğü hatası: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignNewLocation(int gorevliId, int newKurumId)
        {
            if (gorevliId <= 0 || newKurumId <= 0)
                return BadRequest("Invalid IDs.");

            // Begin transaction
            using var transaction = await _gorevliService.BeginTransactionAsync();
            try
            {
                // Deactivate current active assignment
                await _gorevliService.DeactivateCurrentAssignmentAsync(gorevliId);

                // Create new active assignment
                await _gorevliService.CreateAssignmentAsync(gorevliId, newKurumId);

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Yeni atama başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = ex.Message });
            }
        }


        public async Task<IActionResult> ExportToExcel(GorevliFilterViewModel filter)
        {
            var content = await _gorevliService.ExportToExcelAsync(filter);
            var fileName = $"Gorevliler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> CustomExport(GorevliFilterViewModel filter, List<string> columns, List<int>? SelectedIds)
        {
            if (columns == null || !columns.Any())
            {
                columns = new List<string> { "Ad", "Soyad", "Email", "TCKimlikNo" };
            }

            var query = _gorevliService.GetFilteredQueryable(filter);
            if (SelectedIds != null && SelectedIds.Any())
            {
                query = query.Where(g => SelectedIds.Contains(g.Id));
            }
            var content = await _exportService.ExportFilteredAsync(query, columns, "Görevliler");
            var fileName = $"Custom_Export_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(int gorevliId, string notIcerik)
        {
            try
            {
                var userId = _userManager.GetUserId(User) ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "00000000-0000-0000-0000-000000000000";
                await _gorevliService.AddNoteAsync(gorevliId, notIcerik, userId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Gorevli/EditNote")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNote(int noteId, string content)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    return Json(new { success = false, message = "Not içeriği boş olamaz." });
                }
                
                var note = await _gorevliService.GetNoteByIdAsync(noteId);
                if (note == null)
                {
                    return Json(new { success = false, message = "Not bulunamadı." });
                }

                note.NotIcerik = content;
                note.YazanKisiId = _userManager.GetUserId(User) ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "00000000-0000-0000-0000-000000000000";

                await _gorevliService.UpdateNoteAsync(note);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Gorevli/BulkExportSelected")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkExportSelected([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Lütfen görevli seçiniz." });
            }

            try
            {
                var content = await _gorevliService.ExportSelectedToExcelAsync(ids);
                var base64 = Convert.ToBase64String(content);
                return Json(new { 
                    success = true, 
                    fileContents = base64, 
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName = $"Secilen_Gorevliler_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx" 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistemsel veri bütünlüğü hatası: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNote(int id)
        {
            try
            {
                await _gorevliService.DeleteNoteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Message = "Lütfen bir Excel dosyası seçin.";
                return View("Import");
            }

            try
            {
                var result = await _gorevliService.ImportFromExcelAsync(file);

                ViewBag.SuccessCount = result.SuccessCount;
                ViewBag.ErrorCount = result.ErrorCount;
                ViewBag.ImportResults = result.Results;
                ViewBag.Errors = result.Errors;
                ViewBag.Message = $"Import tamamlandı: {result.SuccessCount} başarılı, {result.ErrorCount} hata";
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Hata: {ex.Message}";
            }

            return View("Import");
        }

        private async Task PrepareDropdowns()
        {
            ViewBag.Durumlar = await _lookupService.GetGorevliDurumlariAsync();
            ViewBag.SozlesmeTipleri = await _lookupService.GetSozlesmeTipleriAsync();
            ViewBag.Unvanlar = await _lookupService.GetUnvanlarAsync();
            ViewBag.EgitimDurumlari = await _lookupService.GetEgitimDurumlariAsync();
            ViewBag.HafizlikDurumlari = await _lookupService.GetHafizlikDurumlariAsync();
            ViewBag.KadroTurleri = await _lookupService.GetKadroTurleriAsync();
            ViewBag.AskerlikDurumlari = await _lookupService.GetAskerlikDurumlariAsync();
            ViewBag.KanGruplari = await _lookupService.GetKanGruplariAsync();
        }

        private async Task PrepareFilterDropdowns()
        {
            await PrepareDropdowns();
            var kurumlar = await _kurumService.GetAllAsync(orderBy: q => q.OrderBy(k => k.Isim));
            ViewBag.Kurumlar = kurumlar.ToList();
            ViewBag.Sehirler = kurumlar.Where(k => !string.IsNullOrEmpty(k.Sehir)).Select(k => k.Sehir).Distinct().OrderBy(s => s).ToList();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGorevliDocument(int gorevliId, BelgeTipi type, string? seriNo, DateTime? expirationDate, string? description, IFormFile file, [FromServices] IDocumentStorageService storageService, [FromServices] ApplicationDbContext context)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });

            try
            {
                var relativePath = await storageService.UploadAsync(file, $"gorevli/{gorevliId}");
                var doc = new GorevliBelge
                {
                    GorevliId = gorevliId,
                    BelgeTipi = type,
                    SeriNo = seriNo?.Trim(),
                    GecerlilikTarihi = expirationDate,
                    DosyaYolu = relativePath,
                    Aciklama = description?.Trim(),
                    YuklenmeTarihi = DateTime.UtcNow,
                    YukleyenKullanici = User.Identity?.Name ?? "Sistem Yöneticisi",
                    IsDeleted = false
                };

                context.GorevliBelgeleri.Add(doc);
                await context.SaveChangesAsync();

                return Json(new { success = true, message = "Belge başarıyla yüklendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Dosya yükleme hatası: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGorevliDocument(int id, [FromServices] ApplicationDbContext context)
        {
            var doc = await context.GorevliBelgeleri.FindAsync(id);
            if (doc == null) return Json(new { success = false, message = "Belge bulunamadı." });

            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Json(new { success = true, message = "Belge silindi." });
        }
    }
}
