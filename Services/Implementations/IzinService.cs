using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Enums;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DitibStasbourg.Services.Implementations
{
    public class IzinService : IIzinService
    {
        private readonly ApplicationDbContext _context;
        private readonly IIzinHesaplamaService _izinEngine;

        public IzinService(ApplicationDbContext context, IIzinHesaplamaService izinEngine)
        {
            _context = context;
            _izinEngine = izinEngine;
        }

        public async Task<List<IzinListDto>> GetIzinsAsync(int? gorevliId)
        {
            var query = _context.GorevliIzinler
                .AsNoTracking()
                .Where(i => !i.IsDeleted);

            if (gorevliId.HasValue)
            {
                query = query.Where(i => i.GorevliId == gorevliId.Value);
            }

            var records = await query
                .OrderByDescending(i => i.BaslangicTarihi)
                .Select(i => new
                {
                    i.Id,
                    i.GorevliId,
                    GorevliAdSoyad = i.Gorevli != null ? i.Gorevli.Ad + " " + i.Gorevli.Soyad : "",
                    i.IzinTuru,
                    i.BaslangicTarihi,
                    i.BitisTarihi,
                    i.ToplamGun,
                    i.IsManualEntryByAdmin,
                    i.EvrakNo,
                    i.OnayDurumu,
                    i.OnaylayanKisi
                })
                .ToListAsync();

            // Dynamic grouping/aggregation query at DB level to compute total used leaves
            var totalUsedMap = await _context.GorevliIzinler
                .AsNoTracking()
                .Where(i => !i.IsDeleted && i.OnayDurumu == OnayDurumu.Onaylandi && i.IzinTuru == IzinTuru.YillikIzin)
                .GroupBy(i => i.GorevliId)
                .Select(g => new { GorevliId = g.Key, TotalUsed = g.Sum(i => i.ToplamGun) })
                .ToDictionaryAsync(x => x.GorevliId, x => x.TotalUsed);

            // Fetch dates for accrued calculation
            var staffInfo = await _context.Gorevli
                .AsNoTracking()
                .Include(g => g.Gorevlendirmeler)
                .Where(g => !g.IsDeleted)
                .Select(g => new
                {
                    g.Id,
                    g.FransaGirisTarihi,
                    FirstAssignmentDate = g.Gorevlendirmeler
                        .Where(gl => !gl.IsDeleted)
                        .OrderBy(gl => gl.Tarih)
                        .Select(gl => (DateTime?)gl.Tarih)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var accruedMap = new Dictionary<int, decimal>();
            foreach (var s in staffInfo)
            {
                var startDate = s.FransaGirisTarihi ?? s.FirstAssignmentDate;
                accruedMap[s.Id] = _izinEngine.CalculateTotalAccruedDays(startDate, null);
            }

            return records.Select(r =>
            {
                var accrued = accruedMap.TryGetValue(r.GorevliId, out var acc) ? acc : 0m;
                var used = totalUsedMap.TryGetValue(r.GorevliId, out var usd) ? usd : 0;
                return new IzinListDto
                {
                    Id = r.Id,
                    GorevliId = r.GorevliId,
                    GorevliAdSoyad = r.GorevliAdSoyad,
                    IzinTuru = r.IzinTuru,
                    BaslangicTarihi = r.BaslangicTarihi,
                    BitisTarihi = r.BitisTarihi,
                    ToplamGun = r.ToplamGun,
                    AccruedDays = accrued,
                    RemainingDays = accrued - used,
                    IsManualEntryByAdmin = r.IsManualEntryByAdmin,
                    EvrakNo = r.EvrakNo,
                    OnayDurumu = r.OnayDurumu,
                    OnaylayanKisi = r.OnaylayanKisi
                };
            }).ToList();
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            var years = await _context.GorevliIzinler
                .AsNoTracking()
                .Where(i => !i.IsDeleted)
                .Select(i => i.BaslangicTarihi.Year)
                .Distinct()
                .ToListAsync();

            if (!years.Contains(DateTime.Today.Year))
            {
                years.Add(DateTime.Today.Year);
            }

            return years.OrderByDescending(y => y).ToList();
        }

        public async Task<GorevliIzin?> GetByIdAsync(int id)
        {
            return await _context.GorevliIzinler
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<GorevliIzin?> GetDetailsAsync(int id)
        {
            return await _context.GorevliIzinler
                .AsNoTracking()
                .Include(i => i.Gorevli)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
        }

        public async Task AddAsync(GorevliIzin request)
        {
            await _context.GorevliIzinler.AddAsync(request);
        }

        public async Task UpdateStatusAsync(int id, OnayDurumu durum, string? username)
        {
            var izin = await _context.GorevliIzinler.FindAsync(id);
            if (izin != null)
            {
                izin.OnayDurumu = durum;
                izin.OnaylayanKisi = username;
                izin.OnayTarihi = DateTime.UtcNow;
                _context.Entry(izin).State = EntityState.Modified;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<dynamic>> GetGorevlilerSelectListAsync()
        {
            var gorevliler = await _context.Gorevli
                .AsNoTracking()
                .Where(g => !g.IsDeleted)
                .OrderBy(g => g.Ad)
                .Select(g => new { g.Id, Isim = g.Ad + " " + g.Soyad })
                .ToListAsync();

            return gorevliler.Select(g => (dynamic)g).ToList();
        }

        public async Task<List<Gorevli>> GetMerkezStaffAsync(int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var targetDate = (targetYear == DateTime.Today.Year) ? DateTime.Today : new DateTime(targetYear, 12, 31);

            var staffList = await _context.Gorevli
                .AsNoTracking()
                .Include(g => g.Izinler)
                .Include(g => g.Gorevlendirmeler)
                    .ThenInclude(gl => gl.Kurum)
                .Where(g => !g.IsDeleted)
                .ToListAsync();

            return staffList.Where(g => {
                if (g.IsMerkezPersoneli) return true;
                var active = g.Gorevlendirmeler
                    .Where(gl => !gl.IsDeleted && gl.Tarih <= targetDate && (gl.BitisTarihi == null || gl.BitisTarihi >= targetDate))
                    .OrderByDescending(gl => gl.Tarih)
                    .FirstOrDefault();
                return active != null && (active.KurumId == 1085 || active.Kurum.Isim == "DİTİB Strasbourg" || active.Kurum.Isim == "DITIB Strasbourg");
            }).ToList();
        }

        public async Task<List<Gorevli>> GetOtherStaffAsync(int? year)
        {
            var targetYear = year ?? DateTime.Today.Year;
            var targetDate = (targetYear == DateTime.Today.Year) ? DateTime.Today : new DateTime(targetYear, 12, 31);

            var staffList = await _context.Gorevli
                .AsNoTracking()
                .Include(g => g.Izinler)
                .Include(g => g.Gorevlendirmeler)
                    .ThenInclude(gl => gl.Kurum)
                .Where(g => !g.IsDeleted)
                .ToListAsync();

            var merkezStaff = staffList.Where(g => {
                if (g.IsMerkezPersoneli) return true;
                var active = g.Gorevlendirmeler
                    .Where(gl => !gl.IsDeleted && gl.Tarih <= targetDate && (gl.BitisTarihi == null || gl.BitisTarihi >= targetDate))
                    .OrderByDescending(gl => gl.Tarih)
                    .FirstOrDefault();
                return active != null && (active.KurumId == 1085 || active.Kurum.Isim == "DİTİB Strasbourg" || active.Kurum.Isim == "DITIB Strasbourg");
            }).ToList();

            return staffList.Except(merkezStaff).ToList();
        }
    }
}
