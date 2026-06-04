using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DitibStasbourg.Data;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.ViewComponents
{
    public class MapViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private static List<LocalAddressItem>? _localAddresses;
        private static Dictionary<string, GeocodedAddress> _cache = new();
        private static readonly object _lock = new();
        private static readonly HttpClient _httpClient = new();

        public MapViewComponent(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new MapViewModel
            {
                RegionName = "DITIB Doğu Fransa Bölgesi",
                ViewBox = "0 0 800 600"
            };

            // Fetch Association and Staff counts
            var associations = await _context.Kurum
                .Include(k => k.Gorevlendirmeler)
                .ToListAsync();

            // Load local geocoding cache
            LoadCache();

            foreach (var assoc in associations)
            {
                var geo = await GeocodeAddressAsync(assoc.Isim, assoc.Sehir, assoc.Adres);
                
                model.Markers.Add(new AssociationMarkerViewModel
                {
                    Id = assoc.Id,
                    Name = assoc.Isim ?? "İsimsiz Dernek/Cami",
                    City = assoc.Sehir ?? "Bilinmiyor",
                    Address = assoc.Adres ?? string.Empty,
                    Latitude = geo?.Latitude ?? 48.589202, // Strasbourg center as fallback
                    Longitude = geo?.Longitude ?? 7.71117,
                    DepartmentCode = geo?.DepartmentCode ?? "67",
                    DepartmentName = geo?.DepartmentName ?? "Bas-Rhin",
                    StaffCount = assoc.Gorevlendirmeler?.Count(g => g.BitisTarihi == null || g.BitisTarihi >= DateTime.Today) ?? 0
                });
            }

            return View(model);
        }

        private void LoadCache()
        {
            lock (_lock)
            {
                if (_localAddresses == null)
                {
                    try
                    {
                        var path = Path.Combine(_env.WebRootPath, "data", "adresses.json");
                        if (File.Exists(path))
                        {
                            var json = File.ReadAllText(path);
                            _localAddresses = JsonSerializer.Deserialize<List<LocalAddressItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        }
                    }
                    catch
                    {
                        _localAddresses = new List<LocalAddressItem>();
                    }
                }

                if (_cache.Count == 0)
                {
                    try
                    {
                        var cachePath = Path.Combine(_env.WebRootPath, "data", "geocoding_cache.json");
                        if (File.Exists(cachePath))
                        {
                            var json = File.ReadAllText(cachePath);
                            var cachedItems = JsonSerializer.Deserialize<Dictionary<string, GeocodedAddress>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                            if (cachedItems != null)
                            {
                                _cache = cachedItems;
                            }
                        }
                    }
                    catch
                    {
                        // Cache file corrupt or inaccessible
                    }
                }
            }
        }

        private void SaveCache()
        {
            lock (_lock)
            {
                try
                {
                    var cachePath = Path.Combine(_env.WebRootPath, "data", "geocoding_cache.json");
                    var json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(cachePath, json);
                }
                catch
                {
                    // Fail silently
                }
            }
        }

        private async Task<GeocodedAddress?> GeocodeAddressAsync(string? name, string? city, string? address)
        {
            // 1. Try to clean name and match locally from DITIB addresses database
            var localMatch = MatchLocalAddress(name, city);
            if (localMatch != null) return localMatch;

            // 2. Check memory/file cache using the full address as key
            var cacheKey = $"{address}||{city}".Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(cacheKey)) return null;

            lock (_lock)
            {
                if (_cache.TryGetValue(cacheKey, out var cachedVal))
                {
                    return cachedVal;
                }
            }

            // 3. Dynamic geocoding via public API (data.gouv.fr API Adresse)
            var searchQuery = address;
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                if (!string.IsNullOrWhiteSpace(city))
                {
                    searchQuery = city + ", France";
                }
                else
                {
                    return null;
                }
            }

            try
            {
                var requestUrl = $"https://api-adresse.data.gouv.fr/search/?q={Uri.EscapeDataString(searchQuery)}";
                
                // Add required headers (User-Agent is recommended)
                using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
                {
                    request.Headers.Add("User-Agent", "DitibStasbourgApp/1.0");
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(content))
                        {
                            var root = doc.RootElement;
                            if (root.TryGetProperty("features", out var features) && features.GetArrayLength() > 0)
                            {
                                var firstFeature = features[0];
                                var geometry = firstFeature.GetProperty("geometry");
                                var coordinates = geometry.GetProperty("coordinates");
                                var properties = firstFeature.GetProperty("properties");

                                var lon = coordinates[0].GetDouble();
                                var lat = coordinates[1].GetDouble();
                                var postcode = properties.TryGetProperty("postcode", out var pc) ? pc.GetString() : "";
                                var context = properties.TryGetProperty("context", out var ctx) ? ctx.GetString() : "";

                                // Parse context (e.g., "67, Bas-Rhin, Grand Est")
                                var deptCode = "";
                                var deptName = "";
                                if (!string.IsNullOrEmpty(context))
                                {
                                    var parts = context.Split(',');
                                    if (parts.Length > 0) deptCode = parts[0].Trim();
                                    if (parts.Length > 1) deptName = parts[1].Trim();
                                }

                                var result = new GeocodedAddress
                                {
                                    Latitude = lat,
                                    Longitude = lon,
                                    Postcode = postcode ?? "",
                                    DepartmentCode = deptCode,
                                    DepartmentName = deptName
                                };

                                // Save to cache
                                lock (_lock)
                                {
                                    _cache[cacheKey] = result;
                                }
                                SaveCache();

                                return result;
                            }
                        }
                    }
                }
            }
            catch
            {
                // API error or network issue
            }

            return null;
        }

        private GeocodedAddress? MatchLocalAddress(string? name, string? city)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            if (_localAddresses == null || _localAddresses.Count == 0) return null;

            // Normalize search name
            var cleanName = name.Replace("DITIB", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Camii", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Türk Kültür Derneği", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Türk İslam Kültür Derneği", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Cemiyeti", "", StringComparison.OrdinalIgnoreCase)
                                .Replace("Association", "", StringComparison.OrdinalIgnoreCase)
                                .Trim().ToLowerInvariant();

            var cleanCity = (city ?? "").Trim().ToLowerInvariant();

            // Match by name overlap
            var match = _localAddresses.FirstOrDefault(a => 
                a.Name.Contains(cleanName, StringComparison.OrdinalIgnoreCase) || 
                cleanName.Contains(a.Name.Replace("DITIB", "", StringComparison.OrdinalIgnoreCase).Trim(), StringComparison.OrdinalIgnoreCase));

            // Fallback match by city
            if (match == null && !string.IsNullOrEmpty(cleanCity))
            {
                match = _localAddresses.FirstOrDefault(a => 
                    a.Name.Contains(cleanCity, StringComparison.OrdinalIgnoreCase) ||
                    a.ResolvedAddress.Contains(cleanCity, StringComparison.OrdinalIgnoreCase));
            }

            if (match != null)
            {
                return new GeocodedAddress
                {
                    Latitude = match.Lat,
                    Longitude = match.Lon,
                    Postcode = match.Postcode,
                    DepartmentCode = match.DepartmentCode,
                    DepartmentName = match.DepartmentName
                };
            }

            return null;
        }

        private class LocalAddressItem
        {
            public string Name { get; set; } = string.Empty;
            public string ResolvedAddress { get; set; } = string.Empty;
            public string Postcode { get; set; } = string.Empty;
            public string DepartmentCode { get; set; } = string.Empty;
            public string DepartmentName { get; set; } = string.Empty;
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        private class GeocodedAddress
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string Postcode { get; set; } = string.Empty;
            public string DepartmentCode { get; set; } = string.Empty;
            public string DepartmentName { get; set; } = string.Empty;
        }
    }
}
