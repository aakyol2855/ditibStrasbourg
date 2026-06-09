using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Services.Implementations
{
    public class SystemAuditLogService : ISystemAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor _httpContextAccessor;

        public SystemAuditLogService(ApplicationDbContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string level, string username, string action, string ipAddress, string component)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (string.IsNullOrEmpty(username))
            {
                username = httpContext?.User?.Identity?.Name ?? "System_Deamon";
            }
            
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            }

            var log = new SystemAuditLog
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = level,
                Username = username,
                Action = DitibStasbourg.Core.Utilities.FormatUtils.SafeAuditPayload(action),
                IpAddress = ipAddress,
                Component = component
            };

            _context.SystemAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SystemAuditLog>> GetLogsAsync(string? logLevel = null, string? search = null, int limit = 200)
        {
            var query = _context.SystemAuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(logLevel) && logLevel != "All")
            {
                query = query.Where(l => l.LogLevel == logLevel);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Action.Contains(search) || l.Username.Contains(search) || l.Component.Contains(search) || l.IpAddress.Contains(search));
            }

            return await query
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task ClearLogsAsync()
        {
            _context.SystemAuditLogs.RemoveRange(_context.SystemAuditLogs);
            await _context.SaveChangesAsync();
        }
    }
}
