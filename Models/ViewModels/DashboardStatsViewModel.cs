using DitibStasbourg.Models.Dashboard;

namespace DitibStasbourg.Models.ViewModels
{
    public class DashboardStatsViewModel
    {
        public int TotalAssociations { get; set; }  // Dernek count only
        public int TotalCami { get; set; }           // Cami count only
        public int TotalPersonnel { get; set; }
        public int TotalAssignments { get; set; }
        
        public List<RegionStat> RegionStats { get; set; } = new List<RegionStat>();
        public List<StatusStat> AssignmentStats { get; set; } = new List<StatusStat>();
        public KurbanSummary KurbanSummary { get; set; } = new KurbanSummary();
        
        public DashboardPreference Preferences { get; set; } = new DashboardPreference();
    }

    public class RegionStat
    {
        public string RegionName { get; set; } = string.Empty;
        public int AssociationCount { get; set; }
        public int PersonnelCount { get; set; }
    }

    public class StatusStat
    {
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class KurbanSummary
    {
        public int TotalAnimals { get; set; }
        public int TotalShares { get; set; }
        public int TakenShares { get; set; }
        public double FillRate => TotalShares > 0 ? (double)TakenShares / TotalShares * 100 : 0;
    }
}
