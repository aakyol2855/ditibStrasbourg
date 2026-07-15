using DitibStasbourg.Models;
using DitibStasbourg.Models.ViewModels;
using DitibStasbourg.Services.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

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
        Task<byte[]> ExportSelectedToExcelAsync(List<int> ids);
        Task<(int SuccessCount, int ErrorCount, List<string> Results, List<string> Errors)> ImportFromExcelAsync(IFormFile file);

        // NEW rotation methods
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task DeactivateCurrentAssignmentAsync(int gorevliId);
        Task CreateAssignmentAsync(int gorevliId, int kurumId);

        // Architectural refactoring updates
        Task<int> GetTotalUsedLeavesAsync(int gorevliId);
        Task<List<Gorevli>> CheckDuplicateMatchesAsync(string ad, string soyad, string tcKimlikNo, string eposta);
        Task<GorevliNot?> GetNoteByIdAsync(int noteId);
        Task UpdateNoteAsync(GorevliNot note);

        // Contact auto-fill for İzin Create
        Task<(string? Phone, string? Email)?> GetContactInfoAsync(int gorevliId);
    }
}
