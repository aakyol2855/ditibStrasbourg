using DitibStasbourg.Core.Utilities;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Services.Implementations
{
    public class GorevliService : BaseService<Gorevli>, IGorevliService
    {
        private readonly IMemoryCache _cache;
        private readonly ISystemAuditLogService _auditService;

        public GorevliService(
            ApplicationDbContext context,
            ILogger<GorevliService> logger,
            IMemoryCache cache,
            ISystemAuditLogService auditService) : base(context, logger)
        {
            _cache = cache;
            _auditService = auditService;
        }

        public async Task<List<object>> SearchStaffAsync(string term)
        {
            // Optimization: Only trigger search if at least 3 characters
            if (string.IsNullOrEmpty(term) || term.Length < 3) return new List<object>();

            var staff = await dbSet
                .AsNoTracking()
                .Where(g => g.Ad.Contains(term) || g.Soyad.Contains(term) || (g.Email != null && g.Email.Contains(term)))
                .OrderBy(g => g.Ad)
                .ThenBy(g => g.Soyad)
                .Take(20)
                .Select(g => new {
                    id = g.Id,
                    text = $"{g.Ad} {g.Soyad}" + (string.IsNullOrEmpty(g.Email) ? "" : $" ({g.Email})")
                })
                .ToListAsync();

            return staff.Cast<object>().ToList();
        }

        private IQueryable<Gorevli> BuildFilterQuery(GorevliFilterViewModel filter)
        {
            // Use AsNoTracking() globally for filtering queries
            var query = dbSet
                .AsNoTracking()
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
                .Include(g => g.GorevliNotlari)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchString))
            {
                query = query.Where(s => s.Ad.Contains(filter.SearchString)
                                       || s.Soyad.Contains(filter.SearchString)
                                       || s.Email.Contains(filter.SearchString));
            }

            if (filter.StaffIds != null && filter.StaffIds.Any())
                query = query.Where(s => filter.StaffIds.Contains(s.Id));

            if (filter.SelectedIds != null && filter.SelectedIds.Any())
                query = query.Where(s => filter.SelectedIds.Contains(s.Id));

            if (filter.GorevliDurumIds != null && filter.GorevliDurumIds.Any())
                query = query.Where(s => s.GorevliDurumId.HasValue && filter.GorevliDurumIds.Contains(s.GorevliDurumId.Value));

            if (filter.SozlesmeTipId.HasValue)
                query = query.Where(s => s.SozlesmeTipId == filter.SozlesmeTipId);

            if (filter.KurumId.HasValue)
                query = query.Where(s => s.Gorevlendirmeler.Any(g => g.KurumId == filter.KurumId 
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            
            if (!string.IsNullOrEmpty(filter.Sehir))
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Kurum != null && g.Kurum.Sehir == filter.Sehir
                                                                  && g.Tarih <= DateTime.Now 
                                                                  && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));

            if (!string.IsNullOrEmpty(filter.Cinsiyet))
                query = query.Where(s => s.Cinsiyet == filter.Cinsiyet);

            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                    query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
                else
                     query = query.Where(s => !s.Gorevlendirmeler.Any(g => g.Tarih <= DateTime.Now && (g.BitisTarihi == null || g.BitisTarihi >= DateTime.Now)));
            }
            
            if (filter.TarihBaslangic.HasValue)
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih >= filter.TarihBaslangic.Value));
            if (filter.TarihBitis.HasValue)
                 query = query.Where(s => s.Gorevlendirmeler.Any(g => g.BitisTarihi <= filter.TarihBitis.Value));

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

            // Fransa Giriş Tarihi range filter
            if (filter.FransaGirisBaslangic.HasValue)
                query = query.Where(s => s.FransaGirisTarihi.HasValue && s.FransaGirisTarihi.Value >= filter.FransaGirisBaslangic.Value);

            if (filter.FransaGirisBitis.HasValue)
                query = query.Where(s => s.FransaGirisTarihi.HasValue && s.FransaGirisTarihi.Value <= filter.FransaGirisBitis.Value);

            // Sözleşme Başlangıcı range filter (maps to earliest active assignment start)
            if (filter.SozlesmeBaslangicMin.HasValue)
                query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih >= filter.SozlesmeBaslangicMin.Value));

            if (filter.SozlesmeBaslangicMax.HasValue)
                query = query.Where(s => s.Gorevlendirmeler.Any(g => g.Tarih <= filter.SozlesmeBaslangicMax.Value));

            // Leave status filter
            if (filter.HasLeave.HasValue)
            {
                if (filter.HasLeave.Value)
                    query = query.Where(s => s.Izinler != null && s.Izinler.Any(i => !i.IsDeleted && i.OnayDurumu == DitibStasbourg.Models.Enums.OnayDurumu.Onaylandi));
                else
                    query = query.Where(s => s.Izinler == null || !s.Izinler.Any(i => !i.IsDeleted && i.OnayDurumu == DitibStasbourg.Models.Enums.OnayDurumu.Onaylandi));
            }

            switch (filter.SortOrder)
            {
                case "name_desc":
                    return query.OrderByDescending(s => s.Ad).ThenByDescending(s => s.Soyad);
                case "name_asc":
                    return query.OrderBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "Status":
                    return query.OrderBy(s => s.GorevliDurumBilgisi != null ? s.GorevliDurumBilgisi.Sira : 999).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "status_desc":
                    return query.OrderByDescending(s => s.GorevliDurumBilgisi != null ? s.GorevliDurumBilgisi.Sira : 999).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "Active":
                    return query.OrderByDescending(s => s.Gorevlendirmeler.Any(g => !g.IsDeleted && DateTime.Now.Date >= g.Tarih.Date && (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date))).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "active_desc":
                    return query.OrderBy(s => s.Gorevlendirmeler.Any(g => !g.IsDeleted && DateTime.Now.Date >= g.Tarih.Date && (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date))).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "Date":
                    return query.OrderBy(s => s.Gorevlendirmeler.Where(g => !g.IsDeleted).OrderBy(g => g.Tarih).FirstOrDefault() != null ? s.Gorevlendirmeler.Where(g => !g.IsDeleted).OrderBy(g => g.Tarih).FirstOrDefault().Tarih : DateTime.MaxValue).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
                case "date_desc":
                    return query.OrderByDescending(s => s.Gorevlendirmeler.Where(g => !g.IsDeleted).OrderBy(g => g.Tarih).FirstOrDefault() != null ? s.Gorevlendirmeler.Where(g => !g.IsDeleted).OrderBy(g => g.Tarih).FirstOrDefault().Tarih : DateTime.MinValue).ThenByDescending(s => s.Ad).ThenByDescending(s => s.Soyad);
                default:
                    // Default: active personnel first, then alphabetical
                    return query.OrderByDescending(s => s.Gorevlendirmeler.Any(g => !g.IsDeleted && DateTime.Now.Date >= g.Tarih.Date && (!g.BitisTarihi.HasValue || DateTime.Now.Date <= g.BitisTarihi.Value.Date))).ThenBy(s => s.Ad).ThenBy(s => s.Soyad);
            }
        }

        public IQueryable<Gorevli> GetFilteredQueryable(GorevliFilterViewModel filter)
        {
            return BuildFilterQuery(filter);
        }

        public async Task<PaginatedList<Gorevli>> GetFilteredGorevlilerAsync(GorevliFilterViewModel filter, int pageSize)
        {
            var query = BuildFilterQuery(filter);
            return await PaginatedList<Gorevli>.CreateAsync(query.AsNoTracking(), filter.PageNumber ?? 1, pageSize);
        }

        public async Task<Gorevli?> GetGorevliDetailsAsync(int id)
        {
            return await dbSet
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
                .Include(g => g.Belgeler)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddNoteAsync(int gorevliId, string notIcerik, string? userName)
        {
            var not = new GorevliNot
            {
                GorevliId = gorevliId,
                NotIcerik = notIcerik,
                Tarih = DateTime.Now,
                YazanKisiId = userName
            };

            _context.GorevliNotlari.Add(not);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int noteId)
        {
            var not = await _context.GorevliNotlari.FindAsync(noteId);
            if (not != null)
            {
                not.IsDeleted = true;
                _context.Entry(not).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<byte[]> ExportToExcelAsync(GorevliFilterViewModel filter)
        {
            var query = BuildFilterQuery(filter);
            var gorevliler = await query.ToListAsync();

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
            
            var headerRange = worksheet.Range(1, 1, 1, 15);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
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
            
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportSelectedToExcelAsync(List<int> ids)
        {
            if (ids == null) ids = new List<int>();
            
            var query = dbSet
                .Include(g => g.GorevliDurumBilgisi)
                .Include(g => g.SozlesmeTip)
                .Include(g => g.Unvan)
                .Include(g => g.EgitimDurumu)
                .Include(g => g.HafizlikDurumu)
                .Include(g => g.Gorevlendirmeler)
                    .ThenInclude(gr => gr.Kurum)
                .Where(g => ids.Contains(g.Id));

            var gorevliler = await query.ToListAsync();

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
            
            var headerRange = worksheet.Range(1, 1, 1, 15);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            
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
            
            worksheet.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<(int SuccessCount, int ErrorCount, List<string> Results, List<string> Errors)> ImportFromExcelAsync(IFormFile file)
        {
            var importResults = new List<string>();
            var errors = new List<string>();
            int successCount = 0;
            int errorCount = 0;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    int headerRowIndex = -1;
                    IXLRow? headerRow = null;

                    string NormalizeHeader(string val)
                    {
                        if (string.IsNullOrEmpty(val)) return string.Empty;
                        return val.Trim()
                                  .Replace("\u00A0", " ") // Replace non-breaking spaces
                                  .Replace("\r", " ")
                                  .Replace("\n", " ")     // Remove line breaks inside cells
                                  .ToLowerInvariant()
                                  .Replace("ı", "i")
                                  .Replace("ğ", "g")
                                  .Replace("ü", "u")
                                  .Replace("ş", "s")
                                  .Replace("ö", "o")
                                  .Replace("ç", "c");
                    }

                    for (int r = 1; r <= Math.Min(worksheet.LastRowUsed()?.RowNumber() ?? 0, 15); r++)
                    {
                        var row = worksheet.Row(r);
                        if (row.LastCellUsed() == null) continue;
                        
                        int score = 0;
                        for (int c = 1; c <= row.LastCellUsed().Address.ColumnNumber; c++)
                        {
                            var cellValue = NormalizeHeader(row.Cell(c).GetString());
                            if (string.IsNullOrEmpty(cellValue)) continue;

                            if (cellValue == "ad" || cellValue == "adi" || cellValue == "isim" || cellValue == "name" || cellValue == "first name")
                                score += 3;
                            if (cellValue == "soyad" || cellValue == "soyadi" || cellValue == "surname" || cellValue == "last name")
                                score += 3;
                            if (cellValue.Contains("telefon") || cellValue.Contains("phone") || cellValue == "tel" || cellValue == "cep")
                                score += 2;
                            if (cellValue.Contains("tc") || cellValue.Contains("tckn") || cellValue.Contains("kimlik"))
                                score += 2;
                            if (cellValue.Contains("adres") || cellValue.Contains("address"))
                                score += 2;
                            if (cellValue.Contains("sira") || cellValue == "no" || cellValue == "sn" || cellValue == "s.n." || cellValue == "s.no")
                                score += 1;
                        }

                        if (score >= 4)
                        {
                            headerRowIndex = r;
                            headerRow = row;
                            break;
                        }
                    }

                    if (headerRowIndex == -1 || headerRow == null)
                    {
                        headerRowIndex = 1;
                        headerRow = worksheet.Row(1);
                    }

                    var rows = worksheet.RowsUsed().Where(r => r.RowNumber() > headerRowIndex);
                    var columnMap = new Dictionary<string, int>();
                    
                    for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
                    {
                        var rawHeader = headerRow.Cell(col).GetString();
                        var normalizedHeader = NormalizeHeader(rawHeader);
                        if (!string.IsNullOrEmpty(normalizedHeader))
                        {
                            columnMap[normalizedHeader] = col;
                        }
                    }

                    // Check if required headers exist: "ad" or "soyad"
                    bool hasAd = columnMap.Keys.Any(k => k == "ad" || k == "adi" || k == "isim" || k == "name" || k == "first name" || k.Contains("ad") || k.Contains("adi") || k.Contains("isim") || k.Contains("name"));
                    bool hasSoyad = columnMap.Keys.Any(k => k == "soyad" || k == "soyadi" || k == "surname" || k == "last name" || k.Contains("soyad") || k.Contains("soyadi") || k.Contains("surname"));

                    if (!hasAd || !hasSoyad)
                    {
                        throw new ArgumentException("Yüklenen Excel dosyasında zorunlu 'Ad' veya 'Soyad' sütunları bulunamadı. Lütfen sütun başlıklarını kontrol ediniz.");
                    }

                    string? GetCellValue(IXLRow row, params string[] possibleNames)
                    {
                        foreach (var name in possibleNames)
                        {
                            var nameNorm = NormalizeHeader(name);
                            if (columnMap.TryGetValue(nameNorm, out int colIndex))
                            {
                                return row.Cell(colIndex).GetString().Trim();
                            }
                        }
                        foreach (var name in possibleNames)
                        {
                            var nameNorm = NormalizeHeader(name);
                            var matchedKey = columnMap.Keys.FirstOrDefault(k => k.Contains(nameNorm));
                            if (matchedKey != null)
                            {
                                return row.Cell(columnMap[matchedKey]).GetString().Trim();
                            }
                        }
                        return null;
                    }

                    DateTime? GetDateValue(IXLRow row, params string[] possibleNames)
                    {
                        foreach (var name in possibleNames)
                        {
                            var nameNorm = NormalizeHeader(name);
                            if (columnMap.TryGetValue(nameNorm, out int colIndex))
                            {
                                var cell = row.Cell(colIndex);
                                if (cell.TryGetValue(out DateTime date)) return date;
                            }
                        }
                        foreach (var name in possibleNames)
                        {
                            var nameNorm = NormalizeHeader(name);
                            var matchedKey = columnMap.Keys.FirstOrDefault(k => k.Contains(nameNorm));
                            if (matchedKey != null)
                            {
                                var cell = row.Cell(columnMap[matchedKey]);
                                if (cell.TryGetValue(out DateTime date)) return date;
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
                            var cep  = GetCellValue(row, "cep telefonu", "telefon", "cep", "phone", "mobile");

                            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad))
                            {
                                errors.Add($"Satır {row.RowNumber()}: Ad ve Soyad zorunludur");
                                errorCount++;
                                continue;
                            }

                            // ── 60-second deduplication guard ───────────────
                            var fingerprint = DeduplicationGuard.BuildFingerprint($"{ad} {soyad}", cep);
                            if (DeduplicationGuard.IsDuplicate(_cache, "Gorevli", fingerprint))
                            {
                                errors.Add($"Satır {row.RowNumber()}: Mükerrer kayıt tespit edildi ({ad} {soyad}), atlandı.");
                                errorCount++;
                                continue;
                            }

                            var tc = GetCellValue(row, "tc", "tc kimlik no", "tckimlikno", "tc no")?.Trim();
                            
                            if (string.IsNullOrWhiteSpace(tc))
                            {
                                errors.Add($"Satır {row.RowNumber()}: TC Kimlik No eksik veya boş, kayıt engellendi.");
                                errorCount++;
                                continue;
                            }

                            var email = GetCellValue(row, "email", "e-posta", "eposta", "e-mail");

                            // Explicit database lookup constraint
                            bool existsInDb = false;
                            string duplicateDetail = "";

                            existsInDb = await _context.Gorevli.AnyAsync(g => g.TCKimlikNo == tc && !g.IsDeleted);
                            if (existsInDb)
                            {
                                duplicateDetail = $"TC Kimlik Numarası ({tc}) zaten veritabanında mevcut.";
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(cep))
                                {
                                    existsInDb = await _context.Gorevli.AnyAsync(g => g.Email == email || g.CepTelefonu == cep);
                                    if (existsInDb)
                                    {
                                        duplicateDetail = $"E-posta ({email}) veya Cep Telefonu ({cep}) zaten veritabanında mevcut.";
                                    }
                                }
                                else if (!string.IsNullOrEmpty(email))
                                {
                                    existsInDb = await _context.Gorevli.AnyAsync(g => g.Email == email);
                                    if (existsInDb)
                                    {
                                        duplicateDetail = $"E-posta ({email}) zaten veritabanında mevcut.";
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cep))
                                {
                                    existsInDb = await _context.Gorevli.AnyAsync(g => g.CepTelefonu == cep);
                                    if (existsInDb)
                                    {
                                        duplicateDetail = $"Cep Telefonu ({cep}) zaten veritabanında mevcut.";
                                    }
                                }
                            }

                            if (existsInDb)
                            {
                                errors.Add($"Satır {row.RowNumber()}: Mükerrer kayıt engellendi ({ad} {soyad}). Detay: {duplicateDetail}");
                                errorCount++;
                                continue;
                            }

                            var gorevli = new Gorevli
                            {
                                Ad = ad,
                                Soyad = soyad,
                                Email = email,
                                TCKimlikNo = tc,
                                Cinsiyet = GetCellValue(row, "cinsiyet", "gender", "sex")?.ToUpper().Substring(0, 1),
                                CepTelefonu = cep,
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
                                MezuniyetBolum = GetCellValue(row, "mezuniyet bölüm", "bölüm", "department"),
                                IsDeleted = false
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

            return (successCount, errorCount, importResults, errors);
        }

        public override async Task DeleteAsync(Gorevli entityToDelete)
        {
            var gorevliId = entityToDelete.Id;
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Soft-delete Gorevlendirmeler and related notes
                    var activeAssignments = await _context.Gorevlendirme
                        .Where(g => g.GorevliId == gorevliId)
                        .ToListAsync();

                    foreach (var g in activeAssignments)
                    {
                        g.IsDeleted = true;
                        g.DeletedAt = DateTime.UtcNow;
                        _context.Entry(g).State = EntityState.Modified;

                        var gNotlar = await _context.GorevlendirmeNotlari
                            .Where(gn => gn.GorevlendirmeId == g.Id)
                            .ToListAsync();
                        foreach (var gn in gNotlar)
                        {
                            gn.IsDeleted = true;
                            _context.Entry(gn).State = EntityState.Modified;
                        }
                    }

                    // 2. Soft-delete GorevliNotlari
                    var notes = await _context.GorevliNotlari
                        .Where(n => n.GorevliId == gorevliId)
                        .ToListAsync();
                    foreach (var n in notes)
                    {
                        n.IsDeleted = true;
                        _context.Entry(n).State = EntityState.Modified;
                    }

                    // 3. Soft-delete KurumKasaOdenekler (Budgets)
                    var budgets = await _context.KurumKasaOdenekler
                        .Where(b => b.TargetGorevliId == gorevliId)
                        .ToListAsync();
                    foreach (var b in budgets)
                    {
                        b.IsDeleted = true;
                        b.DeletedAt = DateTime.UtcNow;
                        _context.Entry(b).State = EntityState.Modified;
                    }

                    // 4. Clean up related OverdueNotifications
                    var notifications = await _context.OverdueNotifications
                        .Where(n => n.RelatedGorevliId == gorevliId)
                        .ToListAsync();
                    _context.OverdueNotifications.RemoveRange(notifications);

                    // 5. Soft-delete Gorevli
                    entityToDelete.IsDeleted = true;
                    entityToDelete.DeletedAt = DateTime.UtcNow;
                    _context.Entry(entityToDelete).State = EntityState.Modified;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        // NEW: Begin a transaction for rotation wizard
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        // NEW: Deactivate any current active assignment for a personnel
        public async Task DeactivateCurrentAssignmentAsync(int gorevliId)
        {
            var current = await _context.Gorevlendirme
                .Where(g => g.GorevliId == gorevliId && g.IsActive)
                .FirstOrDefaultAsync();
            if (current != null)
            {
                current.IsActive = false;
                current.BitisTarihi = DateTime.UtcNow;
                _context.Entry(current).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        // NEW: Create a fresh assignment row and activate it
        public async Task CreateAssignmentAsync(int gorevliId, int kurumId)
        {
            var newAssign = new Gorevlendirme
            {
                GorevliId = gorevliId,
                KurumId = kurumId,
                Tarih = DateTime.UtcNow,
                IsActive = true,
                BaslangicTarihi = DateTime.UtcNow
            };
            _context.Gorevlendirme.Add(newAssign);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalUsedLeavesAsync(int gorevliId)
        {
            return await _context.GorevliIzinler
                .AsNoTracking()
                .Where(i => i.GorevliId == gorevliId && i.IzinTuru == Models.Enums.IzinTuru.YillikIzin && i.OnayDurumu == Models.Enums.OnayDurumu.Onaylandi && !i.IsDeleted)
                .SumAsync(i => i.ToplamGun);
        }

        public async Task<List<Gorevli>> CheckDuplicateMatchesAsync(string ad, string soyad, string tcKimlikNo, string eposta)
        {
            return await _context.Gorevli
                .AsNoTracking()
                .Where(g => !g.IsDeleted && (g.Ad == ad && g.Soyad == soyad || g.TCKimlikNo == tcKimlikNo || g.Email == eposta))
                .ToListAsync();
        }

        public async Task<GorevliNot?> GetNoteByIdAsync(int noteId)
        {
            return await _context.GorevliNotlari.FindAsync(noteId);
        }

        public async Task UpdateNoteAsync(GorevliNot note)
        {
            _context.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task<(string? Phone, string? Email)?> GetContactInfoAsync(int gorevliId)
        {
            var result = await _context.Gorevli
                .AsNoTracking()
                .Where(g => g.Id == gorevliId && !g.IsDeleted)
                .Select(g => new { g.CepTelefonu, g.Email })
                .FirstOrDefaultAsync();

            if (result == null) return null;
            return (result.CepTelefonu, result.Email);
        }
    }
}
