using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class DernekIslemleriService : IDernekIslemleriService
    {
        private readonly ApplicationDbContext _context;

        public DernekIslemleriService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Kurum> GetFilteredQueryable(string? search = null, string? sehir = null, string? bolge = null)
        {
            var query = _context.Kurum
                .AsNoTracking()
                .Where(k => k.Tip == KurumTip.Dernek && k.AktifMi == true)
                .Include(k => k.UstKurum)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search) && search.Length >= 3)
            {
                query = query.Where(k => k.Isim.Contains(search) || (k.Adres != null && k.Adres.Contains(search)));
            }

            if (!string.IsNullOrEmpty(sehir)) query = query.Where(k => k.Sehir == sehir);
            if (!string.IsNullOrEmpty(bolge)) query = query.Where(k => k.Bolge == bolge);

            return query.OrderBy(k => k.Isim);
        }

        public async Task<List<Kurum>> GetActiveDerneklerAsync()
        {
            return await _context.Kurum
                .Where(k => (int)k.Tip == 1 && k.AktifMi == true) // 1 = Dernek
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
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task<Kurum> CreateDernekAsync(Kurum dernek)
        {
            dernek.Tip = KurumTip.Dernek;
            dernek.AktifMi = true;
            _context.Add(dernek);
            await _context.SaveChangesAsync();
            return dernek;
        }

        public async Task UpdateBaskanAsync(int id, string ad, string iletisim)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek != null)
            {
                dernek.DernekBaskaniAd = ad;
                dernek.DernekBaskaniIletisim = iletisim;
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
            string? kurulusKanunu, string? baskonsoloslukBolgesi, string? bolge, string? crmUyelikFormDurumu, int? ustKurumId)
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

            _context.Update(dernek);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteDernekAsync(int id)
        {
            var dernek = await _context.Kurum.FindAsync(id);
            if (dernek == null) return false;

            dernek.AktifMi = false;
            _context.Update(dernek);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
