using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Base;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IGorevlendirmeService : IBaseService<Gorevlendirme>
    {
        IQueryable<Gorevlendirme> GetFilteredQueryable(GorevlendirmeFilterViewModel filter);
        Task<PaginatedList<Gorevlendirme>> GetFilteredGorevlendirmelerAsync(GorevlendirmeFilterViewModel filter, int pageSize);
        Task<Gorevlendirme?> GetGorevlendirmeDetailsAsync(int id);
        Task<byte[]> ExportToExcelAsync(int? year, KurumTip? tip, int? gorevliId, int? kurumId, DateTime? startDate, DateTime? endDate, List<string> columns);
        Task AddNoteAsync(int gorevlendirmeId, string notIcerik, string? userName);
        Task DeleteNoteAsync(int noteId);

        /// <summary>
        /// Returns the conflicting assignment name if the given gorevli already has an active (or date-overlapping)
        /// placement during the proposed window, otherwise null.
        /// </summary>
        Task<string?> CheckOverlapAsync(int gorevliId, DateTime tarih, DateTime? bitisTarihi, int? excludeId = null);
        Task<Dictionary<int, string>> GetActiveAssignmentsLookupAsync();

        /// <summary>
        /// Exports only the selected placement records to Excel, using the specified columns.
        /// If columns is null/empty, a standard set is used.
        /// </summary>
        Task<byte[]> ExportSelectedPlacementsAsync(int[] ids, string[]? columns);

        /// <summary>
        /// Marks the specified placement records as deleted (IsDeleted = true) without
        /// physically removing them from the database.
        /// </summary>
        Task<bool> BulkSoftDeletePlacementsAsync(int[] ids);
    }
}
