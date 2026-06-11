using System.Threading.Tasks;

namespace DitibStasbourg.Services.Interfaces
{
    public interface IGeocodingService
    {
        Task<(double? Latitude, double? Longitude)> GeocodeAddressAsync(string? address, string? city);
    }
}
