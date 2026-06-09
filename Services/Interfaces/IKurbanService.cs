using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IKurbanService : IBaseService<Kurbanlik>
    {
        // ── Kurbanlik ──────────────────────────────────────────────────────────
        Task<bool> AutoAssignShareholderAsync(int shareholderId);
        Task<IEnumerable<Kurbanlik>> GetActiveKurbanlarAsync();
        Task<Kurbanlik?> GetKurbanlikByIdAsync(int id);
        Task UpdateKurbanlikAsync(Kurbanlik kurbanlik);
        Task SoftDeleteKurbanlikAsync(int id);

        // ── Hissedar (Shareholder) ─────────────────────────────────────────────
        /// <summary>
        /// Adds a shareholder with 60-second deduplication guard.
        /// Returns (false, reason) if the entry is considered a duplicate.
        /// </summary>
        Task<(bool Success, string? ErrorReason)> AddHissedarAsync(Hissedar hissedar, IMemoryCache cache);
        Task<IEnumerable<Hissedar>> GetHissedarlarAsync(int kurbanlikId);
        Task<bool> UpdateHissedarAsync(Hissedar hissedar);
        Task<bool> DeleteHissedarAsync(int id);
    }
}
