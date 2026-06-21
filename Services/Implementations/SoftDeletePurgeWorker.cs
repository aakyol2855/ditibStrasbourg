using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using DitibStasbourg.Services.Interfaces;

namespace DitibStasbourg.Services.Implementations
{
    public class SoftDeletePurgeWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SoftDeletePurgeWorker> _logger;

        public SoftDeletePurgeWorker(IServiceScopeFactory scopeFactory, ILogger<SoftDeletePurgeWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SoftDeletePurgeWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;
                _logger.LogInformation("SoftDeletePurgeWorker: Next check scheduled at {NextMidnight} (in {Delay})", nextMidnight, delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    _logger.LogInformation("SoftDeletePurgeWorker executing check...");
                    await PurgeExpiredEntitiesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the purge task.");
                }
            }

            _logger.LogInformation("SoftDeletePurgeWorker is stopping.");
        }

        private async Task PurgeExpiredEntitiesAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var auditLogService = scope.ServiceProvider.GetRequiredService<ISystemAuditLogService>();

                // Get retention days from setting
                var retentionSetting = await context.AppSettings.FirstOrDefaultAsync(s => s.Key == "SoftDeleteRetentionDays", stoppingToken);
                int retentionDays = 30;
                if (retentionSetting != null && int.TryParse(retentionSetting.Value, out var parsedDays))
                {
                    retentionDays = parsedDays;
                }

                var thresholdDate = DateTime.UtcNow.AddDays(-retentionDays);
                _logger.LogInformation("Purging soft-deleted records deleted on or before {ThresholdDate} (Retention: {RetentionDays} days)", thresholdDate, retentionDays);

                using (var transaction = await context.Database.BeginTransactionAsync(stoppingToken))
                {
                    try
                    {
                        // 1. Purge Assignments (Gorevlendirme)
                        var expiredAssignments = await context.Gorevlendirme
                            .IgnoreQueryFilters()
                            .Where(a => a.IsDeleted && a.DeletedAt != null && a.DeletedAt <= thresholdDate)
                            .ToListAsync(stoppingToken);

                        if (expiredAssignments.Any())
                        {
                            context.Gorevlendirme.RemoveRange(expiredAssignments);
                            _logger.LogInformation("Purged {Count} expired Assignments.", expiredAssignments.Count);
                        }

                        // 2. Purge Personnel (Gorevli)
                        var expiredPersonnel = await context.Gorevli
                            .IgnoreQueryFilters()
                            .Where(g => g.IsDeleted && g.DeletedAt != null && g.DeletedAt <= thresholdDate)
                            .ToListAsync(stoppingToken);

                        if (expiredPersonnel.Any())
                        {
                            context.Gorevli.RemoveRange(expiredPersonnel);
                            _logger.LogInformation("Purged {Count} expired Personnel records.", expiredPersonnel.Count);
                        }

                        // 3. Purge Associations (Kurum)
                        var expiredAssociations = await context.Kurum
                            .IgnoreQueryFilters()
                            .Where(k => k.IsDeleted && k.DeletedAt != null && k.DeletedAt <= thresholdDate)
                            .ToListAsync(stoppingToken);

                        if (expiredAssociations.Any())
                        {
                            context.Kurum.RemoveRange(expiredAssociations);
                            _logger.LogInformation("Purged {Count} expired Association records.", expiredAssociations.Count);
                        }

                        int totalPurged = expiredAssignments.Count + expiredPersonnel.Count + expiredAssociations.Count;

                        if (totalPurged > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            await transaction.CommitAsync(stoppingToken);

                            await auditLogService.LogAsync(
                                "Information",
                                "System_PurgeWorker",
                                $"Otomatik veri temizleme worker'ı tarafından saklama süresi dolan {totalPurged} adet kayıt (Dernek: {expiredAssociations.Count}, Görevli: {expiredPersonnel.Count}, Görevlendirme: {expiredAssignments.Count}) kalıcı olarak temizlendi.",
                                "127.0.0.1",
                                "SoftDeletePurgeWorker"
                            );
                        }
                        else
                        {
                            await transaction.RollbackAsync(stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(stoppingToken);
                        _logger.LogError(ex, "Transaction failed during database purge operation.");
                        throw;
                    }
                }
            }
        }
    }
}
