using System.Collections.Generic;

namespace DitibStasbourg.Models.ViewModels
{
    public class DataMaintenanceViewModel
    {
        public int TotalPotentialBottlenecks { get; set; }
        public int HissedarDuplicatesCount { get; set; }
        public int GorevliDuplicatesCount { get; set; }
        public int DernekDuplicatesCount { get; set; }
        public List<DuplicateEntryViewModel> FlaggedDuplicates { get; set; } = new();
    }
}
