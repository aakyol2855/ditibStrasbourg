using System;
using System.ComponentModel.DataAnnotations;

namespace DitibStasbourg.Models
{
    public class SystemAuditLog
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = "Information";
        public string Username { get; set; } = "System_Deamon";
        public string Action { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public string? LogType { get; set; }
        public string? Message { get; set; }
    }
}
