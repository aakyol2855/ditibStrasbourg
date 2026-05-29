using System.Collections.Generic;

namespace DitibStasbourg.Models.ViewModels
{
    public class MapViewModel
    {
        public string RegionName { get; set; } = string.Empty;
        public string ViewBox { get; set; } = "0 0 800 600";
        public List<DistrictViewModel> Districts { get; set; } = new();
        public List<AssociationMarkerViewModel> Markers { get; set; } = new();
    }

    public class DistrictViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SvgPath { get; set; } = string.Empty;
        public string Color { get; set; } = "#3f51b5";
        public int AssociationCount { get; set; }
        public int TotalStaff { get; set; }
    }

    public class AssociationMarkerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public string City { get; set; } = string.Empty;
    }
}
