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
    }
}
