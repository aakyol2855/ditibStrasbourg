using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services
{
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _context;

        public LookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Gorevli Durum
        public async Task<List<Ref_GorevliDurum>> GetGorevliDurumlariAsync(bool activeOnly = true)
        {
            var query = _context.Ref_GorevliDurums.AsQueryable();
            if (activeOnly)
            {
                query = query.Where(x => x.AktifMi && !x.IsDeleted);
            }
            else
            {
                query = query.Where(x => !x.IsDeleted);
            }
            return await query.OrderBy(x => x.Sira).ToListAsync();
        }

        public async Task<Ref_GorevliDurum?> GetGorevliDurumByIdAsync(int id)
        {
            return await _context.Ref_GorevliDurums.FindAsync(id);
        }

        public async Task AddGorevliDurumAsync(Ref_GorevliDurum durum)
        {
            _context.Ref_GorevliDurums.Add(durum);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateGorevliDurumAsync(Ref_GorevliDurum durum)
        {
            _context.Ref_GorevliDurums.Update(durum);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteGorevliDurumAsync(int id)
        {
            var entity = await _context.Ref_GorevliDurums.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Sozlesme Tip
        public async Task<List<Ref_SozlesmeTip>> GetSozlesmeTipleriAsync(bool activeOnly = true)
        {
             var query = _context.Ref_SozlesmeTips.AsQueryable();
             if (activeOnly)
             {
                 query = query.Where(x => !x.IsDeleted); // SozlesmeTip has no AktifMi field currently, just IsDeleted
             }
             return await query.OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<Ref_SozlesmeTip?> GetSozlesmeTipByIdAsync(int id)
        {
             return await _context.Ref_SozlesmeTips.FindAsync(id);
        }

        public async Task AddSozlesmeTipAsync(Ref_SozlesmeTip tip)
        {
            _context.Ref_SozlesmeTips.Add(tip);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSozlesmeTipAsync(Ref_SozlesmeTip tip)
        {
            _context.Ref_SozlesmeTips.Update(tip);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSozlesmeTipAsync(int id)
        {
            var entity = await _context.Ref_SozlesmeTips.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Kurum Turu
        public async Task<List<Ref_KurumTuru>> GetKurumTurleriAsync(bool activeOnly = true)
        {
            var query = _context.Ref_KurumTurus.AsQueryable();
            if (activeOnly)
            {
                query = query.Where(x => !x.IsDeleted);
            }
            return await query.OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<Ref_KurumTuru?> GetKurumTuruByIdAsync(int id)
        {
            return await _context.Ref_KurumTurus.FindAsync(id);
        }

        public async Task AddKurumTuruAsync(Ref_KurumTuru tur)
        {
            _context.Ref_KurumTurus.Add(tur);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateKurumTuruAsync(Ref_KurumTuru tur)
        {
            _context.Ref_KurumTurus.Update(tur);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteKurumTuruAsync(int id)
        {
            var entity = await _context.Ref_KurumTurus.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        // New Reference Tables Implementations
        public async Task<List<Ref_Unvan>> GetUnvanlarAsync(bool activeOnly = true)
        {
            return await _context.Ref_Unvans.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<List<Ref_EgitimDurumu>> GetEgitimDurumlariAsync(bool activeOnly = true)
        {
            return await _context.Ref_EgitimDurumlari.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<List<Ref_HafizlikDurumu>> GetHafizlikDurumlariAsync(bool activeOnly = true)
        {
            return await _context.Ref_HafizlikDurumlari.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<List<Ref_KanGrubu>> GetKanGruplariAsync(bool activeOnly = true)
        {
            return await _context.Ref_KanGruplari.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<List<Ref_AskerlikDurumu>> GetAskerlikDurumlariAsync(bool activeOnly = true)
        {
            return await _context.Ref_AskerlikDurumlari.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }

        public async Task<List<Ref_KadroTuru>> GetKadroTurleriAsync(bool activeOnly = true)
        {
            return await _context.Ref_KadroTurleri.Where(x => !activeOnly || !x.IsDeleted).OrderBy(x => x.Ad).ToListAsync();
        }
    }
}
