using System;
using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class DatabaseAuditLogs
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string EntityName { get; set; } = "";
        
        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = ""; // "INSERT", "UPDATE", "DELETE"
        
        [MaxLength(100)]
        public string? Username { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public string? OldValues { get; set; } // JSON format
        public string? NewValues { get; set; } // JSON format
    }
}
