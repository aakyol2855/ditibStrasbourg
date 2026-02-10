namespace DitibStasbourg.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalGorevli { get; set; }
        public int ActiveGorevli { get; set; } // Green Status
        public int TotalKurum { get; set; }
        public int TotalGorevlendirme { get; set; }
        public int GorevlendirmeThisMonth { get; set; }
        public int GorevlendirmeThisYear { get; set; }
        public int UpcomingAssignments { get; set; }
        public object TotalActiveGorevli { get; set; }
    }
}
