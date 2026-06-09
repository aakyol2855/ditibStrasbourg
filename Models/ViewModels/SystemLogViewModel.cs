using System.Collections.Generic;

namespace DitibStasbourg.Models.ViewModels
{
    public class SystemLogViewModel
    {
        public bool DatabaseHealthy { get; set; }
        public string DatabaseStatus { get; set; } = "Unknown";
        
        public double MemoryUsageMB { get; set; }
        public int ThreadCount { get; set; }
        
        public List<SystemAuditLog> RecentLogs { get; set; } = new();
    }
}
