using System;

namespace DitibStasbourg.Models.ViewModels
{
    public class DuplicateEntryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string TargetModule { get; set; } = string.Empty;
        public double? TimeGapSeconds { get; set; }
        public string Details { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
