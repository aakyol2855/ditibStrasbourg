using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class KurbanService : BaseService<Kurbanlik>, IKurbanService
    {
        public KurbanService(ApplicationDbContext context, ILogger<KurbanService> logger) : base(context, logger)
        {
        }

        public async Task<bool> AutoAssignShareholderAsync(int shareholderId)
        {
            var shareholder = await _context.Hissedarlar.FindAsync(shareholderId);
            if (shareholder == null || shareholder.KurbanlikId != null) return false;

            // Find first available animal with remaining shares
            var availableKurban = await dbSet
                .Where(k => k.RemainingShares > 0 && k.Status == "Available")
                .OrderByDescending(k => k.RemainingShares) // Fill animals that are already started
                .FirstOrDefaultAsync();

            if (availableKurban == null) return false;

            shareholder.KurbanlikId = availableKurban.Id;
            availableKurban.RemainingShares--;

            if (availableKurban.RemainingShares == 0)
            {
                availableKurban.Status = "Full";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Kurbanlik>> GetActiveKurbanlarAsync()
        {
            return await dbSet
                .Include(k => k.Hissedarlar)
                .OrderBy(k => k.TagNumber)
                .ToListAsync();
        }
    }
}
