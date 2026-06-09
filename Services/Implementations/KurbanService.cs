using DitibStasbourg.Core.Utilities;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Services.Implementations
{
    public class KurbanService : BaseService<Kurbanlik>, IKurbanService
    {
        private readonly ISystemAuditLogService _auditLogService;

        public KurbanService(
            ApplicationDbContext context,
            ILogger<KurbanService> logger,
            ISystemAuditLogService auditLogService) : base(context, logger)
        {
            _auditLogService = auditLogService;
        }

        // ── Kurbanlik CRUD ─────────────────────────────────────────────────────

        public async Task<IEnumerable<Kurbanlik>> GetActiveKurbanlarAsync()
        {
            return await dbSet
                .AsNoTracking()
                .Include(k => k.Hissedarlar)
                .OrderBy(k => k.TagNumber)
                .ToListAsync();
        }

        public async Task<Kurbanlik?> GetKurbanlikByIdAsync(int id)
        {
            return await dbSet
                .AsNoTracking()
                .Include(k => k.Hissedarlar)
                .FirstOrDefaultAsync(k => k.Id == id);
        }

        public async Task UpdateKurbanlikAsync(Kurbanlik kurbanlik)
        {
            _context.Update(kurbanlik);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteKurbanlikAsync(int id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                // Mark as logically deleted by setting remaining shares to 0 and status to Inactive
                entity.Status = "Inactive";
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
        }

        // ── Hissedar (Shareholder) CRUD ────────────────────────────────────────

        /// <summary>
        /// Adds a shareholder with 60-second temporal deduplication guard.
        /// Decrements available shares atomically in a transaction.
        /// </summary>
        public async Task<(bool Success, string? ErrorReason)> AddHissedarAsync(Hissedar hissedar, IMemoryCache cache)
        {
            // ── Deduplication check (60-second window) ───────────────────────
            var fingerprint = DeduplicationGuard.BuildFingerprint(hissedar.Name, hissedar.Phone);
            if (DeduplicationGuard.IsDuplicate(cache, "Hissedar", fingerprint))
                return (false, "Bu kayıt son 60 saniye içinde zaten eklendi. Lütfen bekleyip tekrar deneyin.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                if (hissedar.KurbanlikId.HasValue)
                {
                    var kurban = await dbSet.FindAsync(hissedar.KurbanlikId.Value);
                    if (kurban == null)
                        return (false, "Belirtilen kurbanlık bulunamadı.");

                    if (kurban.RemainingShares <= 0)
                        return (false, "Bu kurbanlıkta boş hisse kalmamıştır.");

                    kurban.RemainingShares--;
                    if (kurban.RemainingShares == 0)
                        kurban.Status = "Full";

                    _context.Update(kurban);
                }

                hissedar.JoinedAt = DateTime.UtcNow;
                await _context.Hissedarlar.AddAsync(hissedar);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                await _auditLogService.LogAsync(
                    "Information",
                    "System",
                    $"Yeni hissedar eklendi: '{hissedar.Name}' (Telefon: {hissedar.Phone})",
                    "127.0.0.1",
                    "KurbanService");

                return (true, null);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"Kayıt sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Hissedar>> GetHissedarlarAsync(int kurbanlikId)
        {
            return await _context.Hissedarlar
                .AsNoTracking()
                .Where(h => h.KurbanlikId == kurbanlikId)
                .OrderBy(h => h.JoinedAt)
                .ToListAsync();
        }

        public async Task<bool> UpdateHissedarAsync(Hissedar hissedar)
        {
            var existing = await _context.Hissedarlar.FindAsync(hissedar.Id);
            if (existing == null) return false;

            existing.Name            = hissedar.Name;
            existing.Phone           = hissedar.Phone;
            existing.PaymentStatus   = hissedar.PaymentStatus;
            existing.IsVekaletTaken  = hissedar.IsVekaletTaken;

            _context.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHissedarAsync(int id)
        {
            var hissedar = await _context.Hissedarlar.FindAsync(id);
            if (hissedar == null) return false;

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Restore the share slot to the animal
                if (hissedar.KurbanlikId.HasValue)
                {
                    var kurban = await dbSet.FindAsync(hissedar.KurbanlikId.Value);
                    if (kurban != null)
                    {
                        kurban.RemainingShares++;
                        if (kurban.Status == "Full")
                            kurban.Status = "Available";
                        _context.Update(kurban);
                    }
                }

                _context.Hissedarlar.Remove(hissedar);
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                return false;
            }
        }

        // ── Auto-assignment ────────────────────────────────────────────────────

        public async Task<bool> AutoAssignShareholderAsync(int shareholderId)
        {
            var shareholder = await _context.Hissedarlar.FindAsync(shareholderId);
            if (shareholder == null || shareholder.KurbanlikId != null) return false;

            var availableKurban = await dbSet
                .Where(k => k.RemainingShares > 0 && k.Status == "Available")
                .OrderByDescending(k => k.RemainingShares)
                .FirstOrDefaultAsync();

            if (availableKurban == null) return false;

            shareholder.KurbanlikId = availableKurban.Id;
            availableKurban.RemainingShares--;
            if (availableKurban.RemainingShares == 0)
                availableKurban.Status = "Full";

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                "Information",
                "System_Daemon",
                $"Sistem Hissedar '{shareholder.Name}' (ID: {shareholder.Id}) kaydını otomatik olarak '{availableKurban.TagNumber}' küpeli kurbanlığa atadı.",
                "127.0.0.1",
                "KurbanService");

            return true;
        }
    }
}
