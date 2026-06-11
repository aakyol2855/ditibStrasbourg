using ClosedXML.Excel;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class AssociationImportService : IAssociationImportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IGeocodingService _geocodingService;

        public AssociationImportService(ApplicationDbContext context, IFileStorageService fileStorage, IGeocodingService geocodingService)
        {
            _context = context;
            _fileStorage = fileStorage;
            _geocodingService = geocodingService;
        }

        public async Task<ImportResultViewModel> ImportAssociationsAsync(IFormFile file)
        {
            var result = new ImportResultViewModel();
            
            if (file == null || file.Length == 0)
            {
                result.Errors.Add("Dosya boş veya geçersiz.");
                return result;
            }

            var fileName = await _fileStorage.SaveFileAsync(file, "Imports");
            var filePath = _fileStorage.GetFilePath(fileName, "Imports");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1);
                
                int headerRowIndex = -1;
                IXLRow? headerRow = null;

                for (int r = 1; r <= Math.Min(worksheet.LastRowUsed()?.RowNumber() ?? 0, 10); r++)
                {
                    var row = worksheet.Row(r);
                    if (row.LastCellUsed() == null) continue;
                    
                    bool hasHeader = false;
                    for (int c = 1; c <= row.LastCellUsed().Address.ColumnNumber; c++)
                    {
                        var cellValue = row.Cell(c).GetString().Trim();
                        if (cellValue.Contains("dernek", StringComparison.OrdinalIgnoreCase) || 
                            cellValue.Contains("resmi", StringComparison.OrdinalIgnoreCase) ||
                            cellValue.Contains("kurum", StringComparison.OrdinalIgnoreCase))
                        {
                            hasHeader = true;
                            break;
                        }
                    }
                    if (hasHeader)
                    {
                        headerRowIndex = r;
                        headerRow = row;
                        break;
                    }
                }

                if (headerRowIndex == -1 || headerRow == null)
                {
                    throw new ArgumentException("Yüklenen belgede geçerli bir 'Dernek Adı' tablosu bulunamadı. Lütfen Excel yerleşimini kontrol edin.");
                }

                var columnMap = new Dictionary<string, int>();
                
                for (int col = 1; col <= headerRow.LastCellUsed().Address.ColumnNumber; col++)
                {
                    var headerValue = headerRow.Cell(col).GetString().Trim().ToLower();
                    if (!string.IsNullOrEmpty(headerValue))
                    {
                        columnMap[headerValue] = col;
                    }
                }

                bool IsAssociationNameHeader(string header)
                {
                    if (string.IsNullOrWhiteSpace(header)) return false;
                    var val = header.Trim().ToLowerInvariant().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                    
                    if (val.Contains("sira no") || val.Contains("s.n.") || val == "no" || val == "s.no")
                        return false;
                        
                    if (val == "isim" || val == "name" || val == "adi" || val == "ad")
                        return true;

                    if (val.Contains("dernek") && (val.Contains("adi") || val.Contains("ismi") || val.Contains("resmi")))
                        return true;
                        
                    if (val.Contains("kurum") && (val.Contains("ismi") || val.Contains("adi")))
                        return true;
                        
                    if (val.Contains("resmi ad") || val.Contains("resmi adi") || val.Contains("association name"))
                        return true;

                    return false;
                }

                int? FindColumnIndex(params string[] keywords)
                {
                    if (keywords.Contains("dernek adı", StringComparer.OrdinalIgnoreCase))
                    {
                        foreach (var entry in columnMap)
                        {
                            if (IsAssociationNameHeader(entry.Key))
                            {
                                return entry.Value;
                            }
                        }
                    }

                    foreach (var kw in keywords)
                    {
                        var kwLower = kw.Trim().ToLowerInvariant().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                        foreach (var entry in columnMap)
                        {
                            var keyLower = entry.Key.Trim().ToLowerInvariant().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                            if (keyLower == kwLower)
                            {
                                return entry.Value;
                            }
                        }
                    }
                    
                    foreach (var kw in keywords)
                    {
                        var kwLower = kw.Trim().ToLowerInvariant().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                        foreach (var entry in columnMap)
                        {
                            var keyLower = entry.Key.Trim().ToLowerInvariant().Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u").Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");
                            if (keyLower.Contains(kwLower))
                            {
                                return entry.Value;
                            }
                        }
                    }
                    return null;
                }

                string GetValueByKeywords(IXLRow row, params string[] keywords)
                {
                    var index = FindColumnIndex(keywords);
                    if (index.HasValue)
                    {
                        return row.Cell(index.Value).GetValue<string>()?.Trim() ?? string.Empty;
                    }
                    return string.Empty;
                }

                // Check for required primary column (Dernek Adı)
                var isimColIndex = FindColumnIndex("dernek adı", "derneğin resmi adı", "kurum ismi", "dernek adi", "dernegin resmi adi", "kurum ismi");
                if (!isimColIndex.HasValue)
                {
                    throw new ArgumentException("Yüklenen belgede geçerli bir 'Dernek Adı' tablosu bulunamadı. Lütfen Excel yerleşimini kontrol edin.");
                }

                var rows = worksheet.RowsUsed().Where(r => r.RowNumber() > headerRowIndex);
                var ustKurumlar = await _context.Ref_KurumTurus.ToListAsync();

                foreach (var row in rows)
                {
                    result.TotalRows++;
                    try
                    {
                        var isim = GetValueByKeywords(row, "dernek adı", "derneğin resmi adı", "kurum ismi", "dernek adi", "dernegin resmi adi", "kurum ismi");
                        
                        // Strict structural exclusion condition
                        if (string.IsNullOrEmpty(isim) || 
                            isim.Contains("Sıra No", StringComparison.OrdinalIgnoreCase) || 
                            isim.Contains("S.N.", StringComparison.OrdinalIgnoreCase) || 
                            isim.Equals("No", StringComparison.OrdinalIgnoreCase) || 
                            isim.Equals("S.No", StringComparison.OrdinalIgnoreCase) ||
                            int.TryParse(isim, out _) || 
                            double.TryParse(isim, out _))
                        {
                            result.Errors.Add($"Satır {row.RowNumber()}: Geçersiz dernek adı '{isim}' atlandı.");
                            result.FailureCount++;
                            continue;
                        }

                        // Check duplicate — skip (do not abort the whole batch)
                        if (await _context.Kurum.AnyAsync(k => k.Isim == isim))
                        {
                            result.Errors.Add($"Satır {row.RowNumber()}: '{isim}' isimli dernek zaten mevcut, atlandı.");
                            result.FailureCount++;
                            continue;
                        }

                        var ustKurumAd = GetValueByKeywords(row, "üst kurum", "ust kurum", "bağlı olduğu üst kurum", "bagli oldugu ust kurum");
                        var ustKurum = ustKurumlar.FirstOrDefault(u => u.Ad.Equals(ustKurumAd, StringComparison.OrdinalIgnoreCase));

                        var sehir = GetValueByKeywords(row, "şehir", "sehir", "city", "location", "il");
                        var adres = GetValueByKeywords(row, "adres", "address");

                        double? lat = null;
                        double? lon = null;
                        var coords = await _geocodingService.GeocodeAddressAsync(adres, sehir);
                        if (coords.Latitude.HasValue && coords.Longitude.HasValue)
                        {
                            lat = coords.Latitude;
                            lon = coords.Longitude;
                        }

                        var dernek = new Kurum
                        {
                            Isim = isim,
                            Sehir = sehir,
                            Adres = adres,
                            DernekBaskaniAd = GetValueByKeywords(row, "başkan ad soyad", "başkan ad", "baskan ad soyad", "baskan ad", "dernek başkanı", "dernek baskani", "başkan", "baskan", "president"),
                            DernekBaskaniIletisim = GetValueByKeywords(row, "başkan iletişim", "başkan iletisim", "baskan iletisim", "başkan telefon", "baskan telefon", "iletişim numarası", "iletisim numarasi", "baskan tel", "başkan tel", "dernek başkanı iletişim", "dernek başkanı telefon", "dernek baskani tel", "telefon", "iletişim", "iletisim", "phone", "contact"),
                            DinGorevlisiAd = GetValueByKeywords(row, "din görevlisi ad", "din görevlisi", "din gorevlisi", "din görevlisi ad soyad", "din gorevlisi ad soyad", "görevli hoca", "hoca ad soyad", "hoca"),
                            DinGorevlisiIletisim = GetValueByKeywords(row, "din görevlisi iletişim", "din görevlisi telefon", "din gorevlisi iletisim", "din gorevlisi telefon", "din görevlisi tel", "din gorevlisi tel", "hoca telefon", "hoca tel"),
                            Bolge = GetValueByKeywords(row, "bölge", "bolge", "region"),
                            BaskonsoloslukBolgesi = GetValueByKeywords(row, "başkonsolosluk bölgesi", "baskonsolosluk bolgesi", "başkonsolosluk", "baskonsolosluk", "mail", "eposta", "email", "e-posta"),
                            UstKurumId = ustKurum?.Id,
                            Tip = KurumTip.Dernek,
                            AktifMi = true,
                            IsDeleted = false,
                            Latitude = lat,
                            Longitude = lon
                        };

                        _context.Kurum.Add(dernek);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Satır {row.RowNumber()}: Beklenmedik hata - {ex.Message}");
                        result.FailureCount++;
                    }
                }

                // ── Partial-success strategy: commit all valid rows regardless of skipped duplicates ──
                if (result.SuccessCount > 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                    result.Errors.Add("İçe aktarılacak geçerli kayıt bulunamadı. İşlem iptal edildi.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                if (ex is ArgumentException)
                {
                    throw;
                }
                result.Errors.Add($"Dosya okuma hatası: {ex.Message}");
            }
            finally
            {
                // Clean up the temp file
                _fileStorage.DeleteFile(fileName, "Imports");
            }

            return result;
        }
    }
}
