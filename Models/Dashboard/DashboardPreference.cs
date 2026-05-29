using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models.Dashboard
{
    public class DashboardPreference
    {
        [Key]
        public string UserId { get; set; }
        public bool ShowRegionMap { get; set; } = true;
        public bool ShowPersonnelChart { get; set; } = true;
        public bool ShowAssignmentChart { get; set; } = true;
        public bool ShowKurbanChart { get; set; } = true;
    }
}
