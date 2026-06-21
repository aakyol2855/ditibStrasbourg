using DitibStasbourg.Core.Utilities;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DitibStasbourg.Services.Implementations
{
    public class DernekIslemleriService : IDernekIslemleriService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IGeocodingService _geocodingService;
        private readonly IServiceProvider _serviceProvider;

        public DernekIslemleriService(ApplicationDbContext context, IMemoryCache cache, IGeocodingService geocodingService, IServiceProvider serviceProvider)
        {
            _context = context;
            _cache   = cache;
            _geocodingService = geocodingService;
            _serviceProvider = serviceProvider;
        }

        public IQueryable<Kurum> GetFilteredQueryable(string? search = null, string? sehir = null, string? bolge = null)
        {
            var query = _context.Kurum
                .AsNoTracking()
                .Where(k => k.Tip == KurumTip.Dernek && k.AktifMi == true)
                .Include(k => k.UstKurum)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(k => k.Isim.Contains(search) 
                    || (k.Adres != null && k.Adres.Contains(search))
                    || (k.DernekBaskaniAd != null && k.DernekBaskaniAd.Contains(search))
                    || (k.Sehir != null && k.Sehir.Contains(search))
                    || (k.Bolge != null && k.Bolge.Contains(search)));
            }

            if (!string.IsNullOrEmpty(sehir)) query = query.Where(k => k.Sehir == sehir);
            if (!string.IsNullOrEmpty(bolge)) query = query.Where(k => k.Bolge == bolge);

            return query.OrderBy(k => k.Isim);
        }

        public async Task<List<Kurum>> GetActiveDerneklerAsync()
        {
            return await _context.Kurum
                .Where(k => k.Tip == KurumTip.Dernek && k.AktifMi == true)
                .Include(k => k.UstKurum)
                .OrderBy(k => k.Isim)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<string>> GetSehirlerAsync()
        {
            var result = await _context.Kurum
                .AsNoTracking()
                .Where(k => !string.IsNullOrEmpty(k.Sehir))
                .Select(k => k.Sehir)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
                
            return result.Where(s => s != null).Cast<string>().ToList();
        }

        public async Task<List<Ref_KurumTuru>> GetUstKurumlarAsync()
        {
            return await _context.Ref_KurumTurus
                .Where(x => !x.IsDeleted)
                .OrderBy(k => k.Ad)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Kurum?> GetDernekDetayAsync(int id)
        {
            return await _context.Kurum
                .Include(k => k.UstKurum)
                .Include(k => k.DernekUyeleri)
                .Include(k => k.YonetimKuruluUyeleri)
                    .ThenInclude(y => y.YonetimRol)
                .Include(k => k.Gorevlendirmeler)
                    .ThenInclude(g => g.Gorevli)
                .Include(k => k.Gorevlendirmeler)
                    .ThenInclude(g => g.GorevlendirmeNotlari)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        private void StartBackgroundGeocoding(int dernekId, string? adres, string? sehir)
        {
            if (string.IsNullOrWhiteSpace(adres) && string.IsNullOrWhiteSpace(sehir)) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var geocoder = scope.ServiceProvider.GetRequiredService<IGeocodingService>();

                        var coords = await geocoder.GeocodeAddressAsync(adres, sehir);
                        if (coords.Latitude.HasValue && coords.Longitude.HasValue)
                        {
                            var dernek = await dbContext.Kurum.FindAsync(dernekId);
                            if (dernek != null)
                            {
                                dernek.Latitude = coords.Latitude;
                                dernek.Longitude = coords.Longitude;
                                await dbContext.SaveChangesAsync();
                            }
                        }
                    }
                }
                catch
                {
                    // Fail silently
                }
            });
        }

        public async Task<Kurum> CreateDernekAsync(Kurum dernek)
        {
            // ── 60-second deduplication guard ──────────────────────
            var fingerprint = DeduplicationGuard.BuildFingerprint(dernek.Isim, dernek.DernekBaskaniIletisim);
            if (DeduplicationGuard.IsDuplicate(_cache, "Dernek", fingerprint))
                throw new InvalidOperationException("Bu dernek kaydı son 60 saniye içinde zaten gönderildi. Lütfen bekleyin.");

            dernek.Tip    = KurumTip.Dernek;
            dernek.AktifMi = true;

            if (dernek.YonetimKuruluUyeleri != null)
            {
                foreach (var member in dernek.YonetimKuruluUyeleri)
                {
                    member.IsDeleted = false;
                }
            }

            _context.Add(dernek);
            await _context.SaveChangesAsync();

            if (!dernek.Latitude.HasValue || !dernek.Longitude.HasValue)
            {
                StartBackgroundGeocoding(dernek.Id, dernek.Adres, dernek.Sehir);
            }

            return dernek;
        }

        public async Task UpdateBaskanAsync(int id, string ad, string iletisim, string? baskanMail)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek != null)
            {
                dernek.DernekBaskaniAd = ad;
                dernek.DernekBaskaniIletisim = iletisim;
                dernek.BaskanMail = baskanMail;
                _context.Update(dernek);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateDinGorevlisiAsync(int id, string ad, string iletisim)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek != null)
            {
                dernek.DinGorevlisiAd = ad;
                dernek.DinGorevlisiIletisim = iletisim;
                _context.Update(dernek);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddUyeAsync(DernekUye uye)
        {
            _context.DernekUyeleri.Add(uye);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUyeAsync(int id)
        {
            var uye = await _context.DernekUyeleri.FindAsync(id);
            if (uye != null)
            {
                _context.DernekUyeleri.Remove(uye);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdateUyeAsync(int id, string adSoyad, string iletisim, int aileUyeSayisi)
        {
            var uye = await _context.DernekUyeleri.FindAsync(id);
            if (uye == null) return false;

            uye.AdSoyad = adSoyad;
            uye.Iletisim = iletisim;
            uye.AileUyeSayisi = aileUyeSayisi;

            _context.Update(uye);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateDernekAsync(int id, string isim, string? sehir, string? adres, 
            string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId,
            string? iletisimNumarasi, string? maili, double? latitude, double? longitude, int? cemaatCount, string? frenchRegistrationName, List<KurumYonetimKuruluUyesi>? yonetimKurulu)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return false;

            dernek.Isim = isim;
            dernek.Sehir = sehir;
            dernek.Adres = adres;
            dernek.KurulusKanunu = kurulusKanunu;
            dernek.BaskonsoloslukBolgesi = baskonsoloslukBolgesi;
            dernek.Bolge = bolge;
            dernek.CrmUyelikFormDurumu = crmUyelikFormDurumu;
            dernek.UstKurumId = ustKurumId;
            dernek.IletisimNumarasi = iletisimNumarasi;
            dernek.Maili = maili;
            dernek.CemaatCount = cemaatCount;
            dernek.FrenchRegistrationName = frenchRegistrationName;

            if (latitude.HasValue && longitude.HasValue)
            {
                dernek.Latitude = latitude;
                dernek.Longitude = longitude;
            }
            else
            {
                StartBackgroundGeocoding(dernek.Id, dernek.Adres, dernek.Sehir);
            }

            // Sync Board Members
            var existingMembers = await _context.KurumYonetimKuruluUyeleri
                .Where(m => m.KurumId == id && !m.IsDeleted)
                .ToListAsync();

            if (yonetimKurulu == null)
            {
                yonetimKurulu = new List<KurumYonetimKuruluUyesi>();
            }

            foreach (var existing in existingMembers)
            {
                if (!yonetimKurulu.Any(n => n.Id == existing.Id))
                {
                    existing.IsDeleted = true;
                    _context.Entry(existing).State = EntityState.Modified;
                }
            }

            foreach (var member in yonetimKurulu)
            {
                if (member.Id > 0)
                {
                    var existing = existingMembers.FirstOrDefault(e => e.Id == member.Id);
                    if (existing != null)
                    {
                        existing.FullName = member.FullName;
                        existing.ContactPhone = member.ContactPhone;
                        existing.YonetimRolId = member.YonetimRolId;
                        _context.Entry(existing).State = EntityState.Modified;
                    }
                }
                else
                {
                    member.KurumId = id;
                    member.IsDeleted = false;
                    _context.KurumYonetimKuruluUyeleri.Add(member);
                }
            }

            _context.Update(dernek);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteDernekAsync(int id)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return false;

            dernek.AktifMi = false;
            dernek.IsDeleted = true;
            _context.Update(dernek);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedList<Kurum>> GetPaginatedDerneklerAsync(string? search, string? sehir, string? bolge, int pageIndex, int pageSize)
        {
            var query = GetFilteredQueryable(search, sehir, bolge);
            return await PaginatedList<Kurum>.CreateAsync(query, pageIndex, pageSize);
        }

        public async Task<List<Ref_YonetimRol>> GetYonetimRolleriAsync()
        {
            return await _context.Ref_YonetimRols
                .Where(x => !x.IsDeleted)
                .OrderBy(r => r.Ad)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
