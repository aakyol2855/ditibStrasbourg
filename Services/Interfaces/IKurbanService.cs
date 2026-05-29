using DitibStasbourg.Models;
using DitibStasbourg.Services.Base;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IKurbanService : IBaseService<Kurbanlik>
    {
        Task<bool> AutoAssignShareholderAsync(int shareholderId);
        Task<IEnumerable<Kurbanlik>> GetActiveKurbanlarAsync();
    }
}
