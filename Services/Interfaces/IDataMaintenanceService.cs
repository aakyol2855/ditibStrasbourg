using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IDataMaintenanceService
    {
        Task<IEnumerable<DuplicateEntryViewModel>> GetDuplicateEntriesAsync(string module);
        Task<int> PurgeDuplicateEntriesAsync(string module);
        Task ImportExcelStreamAsync(Stream stream, string module, string progressKey);
    }
}
