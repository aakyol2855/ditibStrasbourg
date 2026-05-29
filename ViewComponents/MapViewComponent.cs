using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DitibStasbourg.Data;
using DitibStasbourg.Models.ViewModels;

namespace DitibStasbourg.ViewComponents
{
    public class MapViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MapViewComponent(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var jsonPath = Path.Combine(_env.WebRootPath, "data", "map_metadata.json");
            if (!File.Exists(jsonPath)) return Content("Map data missing");

            var jsonContent = await File.ReadAllTextAsync(jsonPath);
            var mapData = JsonSerializer.Deserialize<MapMetadata>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (mapData == null) return Content("Invalid map data");

            var model = new MapViewModel
            {
                RegionName = mapData.RegionName,
                ViewBox = mapData.BaseViewBox
            };

            // Fetch Association and Staff counts per City/District
            var associations = await _context.Kurum.Include(k => k.Gorevlendirmeler).ToListAsync();
            
            foreach (var district in mapData.Districts)
            {
                var districtAssocs = associations.Where(a => 
                    a.Sehir != null && mapData.CityMapping.ContainsKey(a.Sehir) && mapData.CityMapping[a.Sehir] == district.Id).ToList();

                model.Districts.Add(new DistrictViewModel
                {
                    Id = district.Id,
                    Name = district.Name,
                    SvgPath = district.SvgPath,
                    Color = district.Color,
                    AssociationCount = districtAssocs.Count,
                    TotalStaff = districtAssocs.Sum(da => da.Gorevlendirmeler?.Count ?? 0)
                });

                // Task 5: Address-based auto-marker logic
                // Simple coordinate resolver: city center + random offset for visual separation
                var rnd = new Random();
                foreach (var assoc in districtAssocs)
                {
                    model.Markers.Add(new AssociationMarkerViewModel
                    {
                        Id = assoc.Id,
                        Name = assoc.Isim ?? "Unnamed",
                        City = assoc.Sehir ?? "Unknown",
                        X = district.CenterPoint.X + rnd.Next(-20, 20),
                        Y = district.CenterPoint.Y + rnd.Next(-20, 20)
                    });
                }
            }

            return View(model);
        }

        private class MapMetadata
        {
            public string RegionName { get; set; } = string.Empty;
            public string BaseViewBox { get; set; } = string.Empty;
            public List<DistrictMetadata> Districts { get; set; } = new();
            public Dictionary<string, string> CityMapping { get; set; } = new();
        }

        private class DistrictMetadata
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string SvgPath { get; set; } = string.Empty;
            public Point CenterPoint { get; set; } = new();
            public string Color { get; set; } = string.Empty;
        }

        private class Point { public int X { get; set; } public int Y { get; set; } }
    }
}
