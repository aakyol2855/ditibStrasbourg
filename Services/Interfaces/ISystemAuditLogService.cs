using System.Collections.Generic;
using System.Threading.Tasks;
using DitibStasbourg.Models;

namespace DitibStasbourg.Services.Interfaces
{
    public interface ISystemAuditLogService
    {
        Task LogAsync(string level, string username, string action, string ipAddress, string component);
        Task<List<SystemAuditLog>> GetLogsAsync(string? logLevel = null, string? search = null, int limit = 200);
        Task ClearLogsAsync();
    }
}
