using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Implementations
{
    public class GeocodingService : IGeocodingService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public GeocodingService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(double? Latitude, double? Longitude)> GeocodeAddressAsync(string? address, string? city)
        {
            if (string.IsNullOrWhiteSpace(address) && string.IsNullOrWhiteSpace(city))
            {
                return (null, null);
            }

            // 1. Try geocoding the full address
            if (!string.IsNullOrWhiteSpace(address))
            {
                var query = address.Trim();
                if (!query.Contains("france", StringComparison.OrdinalIgnoreCase))
                {
                    query += ", France";
                }

                var coords = await FetchCoordinatesAsync(query);
                if (coords.Latitude.HasValue && coords.Longitude.HasValue)
                {
                    return coords;
                }
            }

            // 2. Fallback: Try geocoding zip code + city or just city
            string fallbackQuery = string.Empty;
            if (!string.IsNullOrWhiteSpace(address))
            {
                var match = Regex.Match(address, @"\b\d{5}\b");
                if (match.Success)
                {
                    var zip = match.Value;
                    if (!string.IsNullOrWhiteSpace(city))
                    {
                        fallbackQuery = $"{zip} {city.Trim()}, France";
                    }
                    else
                    {
                        fallbackQuery = $"{zip}, France";
                    }
                }
            }

            if (string.IsNullOrEmpty(fallbackQuery) && !string.IsNullOrWhiteSpace(city))
            {
                fallbackQuery = $"{city.Trim()}, France";
            }

            if (!string.IsNullOrEmpty(fallbackQuery))
            {
                var fallbackCoords = await FetchCoordinatesAsync(fallbackQuery);
                if (fallbackCoords.Latitude.HasValue && fallbackCoords.Longitude.HasValue)
                {
                    return fallbackCoords;
                }
            }

            return (null, null);
        }

        private async Task<(double? Latitude, double? Longitude)> FetchCoordinatesAsync(string query)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("GeocodingClient");
                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json&limit=1";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "DitibStasbourgApp/1.0");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(jsonString);
                    var array = doc.RootElement;
                    if (array.ValueKind == JsonValueKind.Array && array.GetArrayLength() > 0)
                    {
                        var first = array[0];
                        if (first.TryGetProperty("lat", out var latProp) && 
                            first.TryGetProperty("lon", out var lonProp))
                        {
                            if (double.TryParse(latProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                                double.TryParse(lonProp.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lon))
                            {
                                return (lat, lon);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Defensive catch to prevent crashes during external API failures
            }

            return (null, null);
        }
    }
}
