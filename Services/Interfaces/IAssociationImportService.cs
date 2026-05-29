using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IAssociationImportService
    {
        Task<ImportResultViewModel> ImportAssociationsAsync(IFormFile file);
    }
}
