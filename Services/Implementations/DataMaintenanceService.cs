using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace DitibStasbourg.Services.Implementations
{
    public class DataMaintenanceService : IDataMaintenanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ImportProgressTracker _progressTracker;

        public DataMaintenanceService(ApplicationDbContext context, ImportProgressTracker progressTracker)
        {
            _context = context;
            _progressTracker = progressTracker;
        }

        public async Task<IEnumerable<DuplicateEntryViewModel>> GetDuplicateEntriesAsync(string module)
        {
            var duplicates = new List<DuplicateEntryViewModel>();

            if (string.Equals(module, "Hissedar", StringComparison.OrdinalIgnoreCase))
            {
                var allShareholders = await _context.Hissedarlar
                    .AsNoTracking()
                    .OrderBy(h => h.JoinedAt)
                    .ToListAsync();

                var groups = allShareholders
                    .Where(h => h.KurbanlikId != null)
                    .GroupBy(h => new { h.Name, h.Phone, h.KurbanlikId });

                foreach (var group in groups)
                {
                    var list = group.ToList();
                    for (int i = 0; i < list.Count; i++)
                    {
                        for (int j = i + 1; j < list.Count; j++)
                        {
                            var timeDiff = Math.Abs((list[j].JoinedAt - list[i].JoinedAt).TotalSeconds);
                            if (timeDiff < 60)
                            {
                                duplicates.Add(new DuplicateEntryViewModel
                                {
                                    Id = list[j].Id,
                                    Name = list[j].Name,
                                    Phone = list[j].Phone,
                                    TargetModule = "Hissedar",
                                    TimeGapSeconds = timeDiff,
                                    Details = $"Aynı isim ({list[j].Name}), telefon ({list[j].Phone}) ve kurbanlığa (ID: {list[j].KurbanlikId}) atanan mükerrer kayıt. Zaman Farkı: {timeDiff:F1} sn.",
                                    CreatedAt = list[j].JoinedAt
                                });
                            }
                        }
                    }
                }
            }
            else if (string.Equals(module, "Gorevli", StringComparison.OrdinalIgnoreCase))
            {
                var allGorevli = await _context.Gorevli.AsNoTracking().ToListAsync();
                var groups = allGorevli
                    .GroupBy(g => new { g.Ad, g.Soyad, g.CepTelefonu })
                    .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key.CepTelefonu));

                foreach (var group in groups)
                {
                    var list = group.OrderBy(g => g.Id).ToList();
                    for (int i = 1; i < list.Count; i++)
                    {
                        duplicates.Add(new DuplicateEntryViewModel
                        {
                            Id = list[i].Id,
                            Name = $"{list[i].Ad} {list[i].Soyad}",
                            Phone = list[i].CepTelefonu ?? string.Empty,
                            TargetModule = "Gorevli",
                            TimeGapSeconds = null,
                            Details = $"Aynı isim ve telefon numarasına sahip mükerrer görevli kaydı.",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
            else if (string.Equals(module, "Dernek", StringComparison.OrdinalIgnoreCase))
            {
                var allDernek = await _context.Kurum
                    .AsNoTracking()
                    .Where(k => k.Tip == KurumTip.Dernek)
                    .ToListAsync();
                var groups = allDernek
                    .GroupBy(k => new { k.Isim, k.Sehir })
                    .Where(g => g.Count() > 1);

                foreach (var group in groups)
                {
                    var list = group.OrderBy(k => k.Id).ToList();
                    for (int i = 1; i < list.Count; i++)
                    {
                        duplicates.Add(new DuplicateEntryViewModel
                        {
                            Id = list[i].Id,
                            Name = list[i].Isim,
                            Phone = list[i].DernekBaskaniIletisim ?? string.Empty,
                            TargetModule = "Dernek",
                            TimeGapSeconds = null,
                            Details = $"'{list[i].Sehir}' şehrinde aynı isimle kayıtlı mükerrer dernek.",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            return duplicates;
        }

        public async Task<int> PurgeDuplicateEntriesAsync(string module)
        {
            var duplicates = await GetDuplicateEntriesAsync(module);
            var duplicateIds = duplicates.Select(d => d.Id).ToList();

            if (duplicateIds.Count == 0) return 0;

            if (string.Equals(module, "Hissedar", StringComparison.OrdinalIgnoreCase))
            {
                var rowsToDelete = await _context.Hissedarlar
                    .Where(h => duplicateIds.Contains(h.Id))
                    .ToListAsync();

                // Increment remaining shares for the animal if we remove shareholders
                foreach (var row in rowsToDelete)
                {
                    if (row.KurbanlikId != null)
                    {
                        var kurban = await _context.Kurbanliklar.FindAsync(row.KurbanlikId);
                        if (kurban != null)
                        {
                            kurban.RemainingShares++;
                            if (kurban.Status == "Full")
                            {
                                kurban.Status = "Available";
                            }
                        }
                    }
                }

                _context.Hissedarlar.RemoveRange(rowsToDelete);
            }
            else if (string.Equals(module, "Gorevli", StringComparison.OrdinalIgnoreCase))
            {
                var rowsToDelete = await _context.Gorevli
                    .Where(g => duplicateIds.Contains(g.Id))
                    .ToListAsync();
                _context.Gorevli.RemoveRange(rowsToDelete);
            }
            else if (string.Equals(module, "Dernek", StringComparison.OrdinalIgnoreCase))
            {
                var rowsToDelete = await _context.Kurum
                    .Where(k => duplicateIds.Contains(k.Id))
                    .ToListAsync();
                _context.Kurum.RemoveRange(rowsToDelete);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task ImportExcelStreamAsync(Stream stream, string module, string progressKey)
        {
            try
            {
                _progressTracker.SetProgress(progressKey, 1);

                // Count total rows to calculate progress percentage (Deferred streaming read)
                stream.Position = 0;
                int totalRows = 0;
                foreach (var row in stream.Query(useHeaderRow: true))
                {
                    totalRows++;
                }

                if (totalRows == 0)
                {
                    _progressTracker.SetProgress(progressKey, 100);
                    return;
                }

                stream.Position = 0;
                int processedRows = 0;
                var batch = new List<IDictionary<string, object>>();

                foreach (IDictionary<string, object> row in stream.Query(useHeaderRow: true))
                {
                    batch.Add(row);
                    if (batch.Count >= 50)
                    {
                        await SaveBatchAsync(batch, module);
                        processedRows += batch.Count;
                        int progress = (int)((double)processedRows / totalRows * 100);
                        // Cap progress at 99 until finished
                        _progressTracker.SetProgress(progressKey, Math.Min(progress, 99));
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    await SaveBatchAsync(batch, module);
                    processedRows += batch.Count;
                }

                _progressTracker.SetProgress(progressKey, 100);
            }
            catch (Exception ex)
            {
                _progressTracker.SetProgress(progressKey, -1); // -1 signifies an error
                throw;
            }
        }

        private async Task SaveBatchAsync(List<IDictionary<string, object>> batch, string module)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (string.Equals(module, "Dernek", StringComparison.OrdinalIgnoreCase))
                    {
                        var kurumlar = new List<Kurum>();
                        foreach (var row in batch)
                        {
                            var isim = GetVal(row, "Derneğin resmi adı", "Dernek Adı", "Isim", "Name", "AssociationName");
                            if (string.IsNullOrEmpty(isim)) continue;

                            var kurum = new Kurum
                            {
                                Isim = isim,
                                Sehir = GetVal(row, "Şehir", "Sehir", "Location", "City"),
                                Adres = GetVal(row, "Adres", "Address"),
                                DernekBaskaniAd = GetVal(row, "Başkan ad soyad", "Baskan", "PresidentName"),
                                DernekBaskaniIletisim = GetVal(row, "İletişim numarası", "Telefon", "PhoneNumber"),
                                BaskonsoloslukBolgesi = GetVal(row, "Maili / Başkan mail", "Email", "EmailAddress"),
                                Tip = KurumTip.Dernek,
                                AktifMi = true
                            };
                            kurumlar.Add(kurum);
                        }
                        if (kurumlar.Count > 0)
                        {
                            await _context.Kurum.AddRangeAsync(kurumlar);
                        }
                    }
                    else if (string.Equals(module, "Gorevli", StringComparison.OrdinalIgnoreCase))
                    {
                        var gorevliler = new List<Gorevli>();
                        foreach (var row in batch)
                        {
                            var ad = GetVal(row, "Ad", "FirstName");
                            var soyad = GetVal(row, "Soyad", "LastName");
                            if (string.IsNullOrEmpty(ad) || string.IsNullOrEmpty(soyad)) continue;

                            var gorevli = new Gorevli
                            {
                                Ad = ad,
                                Soyad = soyad,
                                Email = GetVal(row, "E-posta", "Email", "Mail"),
                                CepTelefonu = GetVal(row, "Cep Telefonu", "Telefon", "Phone", "Mobile"),
                                TCKimlikNo = GetVal(row, "TC Kimlik No", "TC", "NationalId")
                            };
                            gorevliler.Add(gorevli);
                        }
                        if (gorevliler.Count > 0)
                        {
                            await _context.Gorevli.AddRangeAsync(gorevliler);
                        }
                    }
                    else if (string.Equals(module, "Kurban", StringComparison.OrdinalIgnoreCase))
                    {
                        var kurbanliklar = new List<Kurbanlik>();
                        foreach (var row in batch)
                        {
                            var tag = GetVal(row, "Küpe Numarası", "Tag Number", "TagNumber", "KupeNo");
                            if (string.IsNullOrEmpty(tag)) continue;

                            decimal.TryParse(GetVal(row, "Kilo", "Ağırlık", "Weight"), out decimal weight);
                            decimal.TryParse(GetVal(row, "Fiyat", "Tutar", "Price"), out decimal price);
                            int.TryParse(GetVal(row, "Hisse Sayısı", "Toplam Hisse", "TotalShares") ?? "7", out int shares);

                            var kurbanlik = new Kurbanlik
                            {
                                TagNumber = tag,
                                Species = GetVal(row, "Tür", "Kategori", "Species") ?? "Büyükbaş",
                                Weight = weight,
                                Price = price,
                                TotalShares = shares,
                                RemainingShares = shares,
                                Status = "Available"
                            };
                            kurbanliklar.Add(kurbanlik);
                        }
                        if (kurbanliklar.Count > 0)
                        {
                            await _context.Kurbanliklar.AddRangeAsync(kurbanliklar);
                        }
                    }

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

        private string? GetVal(IDictionary<string, object> row, params string[] possibleKeys)
        {
            foreach (var key in possibleKeys)
            {
                var matchedKey = row.Keys.FirstOrDefault(k => string.Equals(k.Trim(), key, StringComparison.OrdinalIgnoreCase));
                if (matchedKey != null && row[matchedKey] != null)
                {
                    return row[matchedKey]?.ToString()?.Trim();
                }
            }
            return null;
        }
    }
}
