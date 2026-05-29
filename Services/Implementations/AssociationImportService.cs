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

        public AssociationImportService(ApplicationDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
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
                var rows = worksheet.RowsUsed().Skip(1); // Skip header

                var ustKurumlar = await _context.Ref_KurumTurus.ToListAsync();

                foreach (var row in rows)
                {
                    result.TotalRows++;
                    try
                    {
                        var isim = row.Cell(1).GetValue<string>().Trim();
                        if (string.IsNullOrEmpty(isim))
                        {
                            result.Errors.Add($"Satır {result.TotalRows + 1}: Dernek adı boş olamaz.");
                            result.FailureCount++;
                            continue;
                        }

                        // Check duplicate
                        if (await _context.Kurum.AnyAsync(k => k.Isim == isim))
                        {
                            result.Errors.Add($"Satır {result.TotalRows + 1}: '{isim}' isimli dernek zaten mevcut.");
                            result.FailureCount++;
                            continue;
                        }

                        var ustKurumAd = row.Cell(8).GetValue<string>().Trim();
                        var ustKurum = ustKurumlar.FirstOrDefault(u => u.Ad.Equals(ustKurumAd, StringComparison.OrdinalIgnoreCase));

                        var dernek = new Kurum
                        {
                            Isim = isim,
                            Sehir = row.Cell(2).GetValue<string>(),
                            Adres = row.Cell(3).GetValue<string>(),
                            DernekBaskaniAd = row.Cell(4).GetValue<string>(),
                            DernekBaskaniIletisim = row.Cell(5).GetValue<string>(),
                            DinGorevlisiAd = row.Cell(6).GetValue<string>(),
                            DinGorevlisiIletisim = row.Cell(7).GetValue<string>(),
                            Bolge = row.Cell(9).GetValue<string>(),
                            BaskonsoloslukBolgesi = row.Cell(10).GetValue<string>(),
                            UstKurumId = ustKurum?.Id,
                            Tip = KurumTip.Dernek,
                            AktifMi = true
                        };

                        _context.Kurum.Add(dernek);
                        result.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Satır {result.TotalRows + 1}: Beklenmedik hata - {ex.Message}");
                        result.FailureCount++;
                    }
                }

                if (result.FailureCount == 0)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                    result.Errors.Add("Hatalar nedeniyle işlem iptal edildi. Hiçbir veri kaydedilmedi.");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
