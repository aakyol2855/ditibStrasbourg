using System.Threading.Tasks;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IDibbysPdfEngine
    {
        Task<byte[]> GenerateLeavePdfAsync(int izinId);
    }
}
