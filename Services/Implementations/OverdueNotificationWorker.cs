using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DitibStasbourg.Services.Implementations
{
    /// <summary>
    /// Arka plan servisi: Her gün gece yarısı gecikmiş ödemeleri, süresi dolan belgeleri,
    /// vize/pasaport/oturum kartı bitiş tarihlerini tarar ve OverdueNotification tablosuna yazar.
    /// </summary>
    public class OverdueNotificationWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OverdueNotificationWorker> _logger;

        public OverdueNotificationWorker(IServiceScopeFactory scopeFactory, ILogger<OverdueNotificationWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OverdueNotificationWorker is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                var delay = nextMidnight - now;

                // On first run, execute immediately if past 2 AM
                if (now.Hour >= 2)
                {
                    await GenerateNotificationsAsync();
                    delay = nextMidnight.AddHours(2) - now;
                }

                _logger.LogInformation("OverdueNotificationWorker: Next scan scheduled at {NextRun} (in {Delay})", 
                    now.Add(delay), delay);

                try
                {
                    await Task.Delay(delay, stoppingToken);
                    await GenerateNotificationsAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OverdueNotificationWorker encountered an error.");
                }
            }
        }

        private async Task GenerateNotificationsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var today = DateTime.Today;
            var threeMonthsLater = today.AddMonths(3);
            int newNotifications = 0;

            // ─── 1. Maliye: Gecikmiş Taksit Ödemeleri ──────────────────────────
            var overduePeriods = await context.KurumButcePeriods
                .Include(p => p.KurumButce).ThenInclude(b => b.Kurum)
                .Where(p => !p.IsPaid && p.TargetDate < today)
                .ToListAsync();

            foreach (var period in overduePeriods)
            {
                int targetPeriodId = period.Id;
                bool alertExists = await context.OverdueNotifications
                    .AnyAsync(n => n.KurumButcePeriodId == targetPeriodId && !n.IsResolved);

                if (!alertExists)
                {
                    context.OverdueNotifications.Add(new OverdueNotification
                    {
                        NotificationType = "OdemeGecikme",
                        Severity = "Critical",
                        Title = $"{period.KurumButce.Kurum.Isim} - {period.PeriodNumber}. Dönem Ödemesi Gecikti",
                        Message = $"{period.KurumButce.Yil} yılı {period.PeriodNumber}. dönem ödeme vade tarihi ({period.TargetDate:dd.MM.yyyy}) geçmiştir. Gecikme: {(today - period.TargetDate).Days} gün. Tutar: {period.ScheduledAmount:C2}",
                        RelatedKurumId = period.KurumButce.KurumId,
                        RelatedBudgetPeriodId = period.Id,
                        KurumButcePeriodId = period.Id,
                        DueDate = period.TargetDate,
                        TargetEmail = period.KurumButce.Kurum.Maili
                    });
                    newNotifications++;
                }
            }

            // ─── 2. Görevli: Vize / Pasaport / Oturum İzni Uyarıları ──────────
            var expiringStaff = await context.Gorevli
                .Where(g => !g.IsDeleted &&
                    ((g.VisaExpirationDate.HasValue && g.VisaExpirationDate.Value <= threeMonthsLater) ||
                     (g.PassportExpirationDate.HasValue && g.PassportExpirationDate.Value <= threeMonthsLater) ||
                     (g.ResidencePermitExpirationDate.HasValue && g.ResidencePermitExpirationDate.Value <= threeMonthsLater)))
                .ToListAsync();

            foreach (var g in expiringStaff)
            {
                await CreateImmigrationNotification(context, g, "VizeSuresi", g.VisaExpirationDate, "Vize", today, threeMonthsLater);
                await CreateImmigrationNotification(context, g, "PasaportSuresi", g.PassportExpirationDate, "Pasaport", today, threeMonthsLater);
                await CreateImmigrationNotification(context, g, "OturumIzni", g.ResidencePermitExpirationDate, "Oturum Kartı (Titre de Séjour)", today, threeMonthsLater);
            }

            // ─── 3. Dernek Evrak: Süresi Dolan Resmi Belgeler ─────────────────
            var expiringDocs = await context.KurumDocuments
                .Include(d => d.Kurum)
                .Where(d => !d.IsDeleted && d.ExpirationDate.HasValue && d.ExpirationDate.Value <= threeMonthsLater)
                .ToListAsync();

            foreach (var doc in expiringDocs)
            {
                var exists = await context.OverdueNotifications.AnyAsync(n =>
                    n.NotificationType == "BelgeSuresi" &&
                    n.Title.Contains(doc.DocumentName) &&
                    n.RelatedKurumId == doc.KurumId &&
                    n.CreatedAt.Date == today);

                if (!exists)
                {
                    var daysLeft = (doc.ExpirationDate!.Value - today).Days;
                    context.OverdueNotifications.Add(new OverdueNotification
                    {
                        NotificationType = "BelgeSuresi",
                        Severity = daysLeft <= 0 ? "Critical" : daysLeft <= 30 ? "Warning" : "Info",
                        Title = $"{doc.Kurum.Isim} - '{doc.DocumentName}' Belgesi Sona Eriyor",
                        Message = daysLeft <= 0
                            ? $"'{doc.DocumentName}' ({doc.Category}) belgesinin süresi dolmuştur! Lütfen yenileme prosedürünü başlatın."
                            : $"'{doc.DocumentName}' ({doc.Category}) belgesinin süresine {daysLeft} gün kalmıştır.",
                        RelatedKurumId = doc.KurumId,
                        DueDate = doc.ExpirationDate.Value,
                        TargetEmail = doc.Kurum.Maili
                    });
                    newNotifications++;
                }
            }

            // ─── 4. Dernek Not: Hatırlatma / Bitiş Tarihi Yaklaşan Notlar ──────
            var expiringNotes = await context.DernekNotlari
                .Include(n => n.Dernek)
                .Where(n => !n.IsDeleted && n.BitisTarihi.HasValue && n.BitisTarihi.Value <= threeMonthsLater)
                .ToListAsync();

            foreach (var note in expiringNotes)
            {
                var exists = await context.OverdueNotifications.AnyAsync(n =>
                    n.NotificationType == "DernekNotHatirlatma" &&
                    n.RelatedDernekNotId == note.Id &&
                    !n.IsResolved);

                if (!exists)
                {
                    var daysLeft = (note.BitisTarihi!.Value - today).Days;
                    context.OverdueNotifications.Add(new OverdueNotification
                    {
                        NotificationType = "DernekNotHatirlatma",
                        Severity = daysLeft <= 0 ? "Critical" : daysLeft <= 30 ? "Warning" : "Info",
                        Title = $"{note.Dernek?.Isim} - Not Hatırlatması",
                        Message = daysLeft <= 0
                            ? $"Dernek notu için belirlenen bitiş tarihi ({note.BitisTarihi:dd.MM.yyyy}) dolmuştur! Not içeriği: {note.NotIcerigi}"
                            : $"Dernek notu bitiş tarihine {daysLeft} gün kalmıştır ({note.BitisTarihi:dd.MM.yyyy}). Not içeriği: {note.NotIcerigi}",
                        RelatedKurumId = note.DernekId,
                        RelatedDernekNotId = note.Id,
                        DueDate = note.BitisTarihi.Value,
                        TargetEmail = note.Dernek?.Maili
                    });
                    newNotifications++;
                }
            }

            // ─── 5. Görevli Belge: Süresi Dolan Resmi/Dil Belgeleri ────────────
            var expiringGorevliDocs = await context.GorevliBelgeleri
                .Include(b => b.Gorevli)
                .Where(b => !b.IsDeleted && b.GecerlilikTarihi.HasValue && b.GecerlilikTarihi.Value <= threeMonthsLater)
                .ToListAsync();

            foreach (var doc in expiringGorevliDocs)
            {
                var exists = await context.OverdueNotifications.AnyAsync(n =>
                    n.NotificationType == "GorevliBelgeSuresi" &&
                    n.RelatedGorevliBelgeId == doc.Id &&
                    !n.IsResolved);

                if (!exists)
                {
                    var daysLeft = (doc.GecerlilikTarihi!.Value - today).Days;
                    var typeLabel = doc.BelgeTipi.ToString();
                    context.OverdueNotifications.Add(new OverdueNotification
                    {
                        NotificationType = "GorevliBelgeSuresi",
                        Severity = daysLeft <= 0 ? "Critical" : daysLeft <= 30 ? "Warning" : "Info",
                        Title = $"{doc.Gorevli?.AdSoyad} - '{typeLabel}' Süresi Sona Eriyor",
                        Message = daysLeft <= 0
                            ? $"Görevlinin '{typeLabel}' belgesinin geçerlilik süresi dolmuştur!"
                            : $"Görevlinin '{typeLabel}' belgesinin geçerlilik süresine {daysLeft} gün kalmıştır.",
                        RelatedGorevliId = doc.GorevliId,
                        RelatedGorevliBelgeId = doc.Id,
                        DueDate = doc.GecerlilikTarihi.Value,
                        TargetEmail = doc.Gorevli?.Email
                    });
                    newNotifications++;
                }
            }

            if (newNotifications > 0)
            {
                await context.SaveChangesAsync();
            }

            _logger.LogInformation("OverdueNotificationWorker: Scan complete. {Count} new notifications generated.", newNotifications);
        }

        private static async Task CreateImmigrationNotification(
            ApplicationDbContext context, Gorevli g, string type, DateTime? expDate, string label, DateTime today, DateTime threshold)
        {
            if (!expDate.HasValue || expDate.Value > threshold) return;

            var exists = await context.OverdueNotifications.AnyAsync(n =>
                n.NotificationType == type &&
                n.RelatedGorevliId == g.Id &&
                n.CreatedAt.Date == today);

            if (exists) return;

            var daysLeft = (expDate.Value - today).Days;
            context.OverdueNotifications.Add(new OverdueNotification
            {
                NotificationType = type,
                Severity = daysLeft <= 0 ? "Critical" : daysLeft <= 30 ? "Warning" : "Info",
                Title = $"{g.AdSoyad} - {label} Süresi {(daysLeft <= 0 ? "Doldu!" : $"{daysLeft} Gün Kaldı")}",
                Message = $"{g.AdSoyad}'ın {label} geçerlilik tarihi: {expDate.Value:dd.MM.yyyy}. {(daysLeft <= 0 ? "Süresi dolmuştur!" : $"Kalan süre: {daysLeft} gün.")}",
                RelatedGorevliId = g.Id,
                DueDate = expDate.Value,
                TargetEmail = g.Email
            });
        }
    }
}
