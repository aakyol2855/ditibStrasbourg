using System.Collections.Generic;

namespace DitibStasbourg.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalGorevli { get; set; }
        public int ActiveGorevli { get; set; }
        public int TotalKurum { get; set; }
        public int TotalGorevlendirme { get; set; }
        public int GorevlendirmeThisMonth { get; set; }
        public int GorevlendirmeThisYear { get; set; }
        public int UpcomingAssignments { get; set; }

        // Financial Campaign Insights
        public List<FinancialCampaignSummaryDto> CampaignSummaries { get; set; } = new();

        // Regional Staff Density
        public List<RegionalStaffDensityDto> RegionalDensities { get; set; } = new();

        // Sacrificial Ledger Metrics
        public int TotalKurbanShares { get; set; }
        public int SoldKurbanShares { get; set; }
        public int RemainingKurbanShares { get; set; }
        public decimal TotalKurbanCollected { get; set; }
        public decimal TotalKurbanOverdue { get; set; }
    }

    public class FinancialCampaignSummaryDto
    {
        public int Year { get; set; }
        public string CampaignType { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class RegionalStaffDensityDto
    {
        public string Region { get; set; }
        public int ActiveCount { get; set; }
        public int UnassignedCount { get; set; }
    }
}
