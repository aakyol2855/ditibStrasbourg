using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Base;
using Microsoft.AspNetCore.Http;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IGorevliService : IBaseService<Gorevli>
    {
        IQueryable<Gorevli> GetFilteredQueryable(GorevliFilterViewModel filter);
        Task<PaginatedList<Gorevli>> GetFilteredGorevlilerAsync(GorevliFilterViewModel filter, int pageSize);
        Task<List<object>> SearchStaffAsync(string term);
        Task<Gorevli?> GetGorevliDetailsAsync(int id);
        Task AddNoteAsync(int gorevliId, string notIcerik, string? userName);
        Task DeleteNoteAsync(int noteId);
        Task<byte[]> ExportToExcelAsync(GorevliFilterViewModel filter);
        Task<(int SuccessCount, int ErrorCount, List<string> Results, List<string> Errors)> ImportFromExcelAsync(IFormFile file);
    }
}
