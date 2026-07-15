using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Models;
using DitibStasbourg.Models.Dashboard;
using DitibStasbourg.Models.Security;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DitibStasbourg.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<DatabaseAuditLogs> DatabaseAuditLogs { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }
    public DbSet<Gorevli> Gorevli { get; set; }
    public DbSet<Kurum> Kurum { get; set; }
    public DbSet<Gorevlendirme> Gorevlendirme { get; set; }
    public DbSet<KurumFinansalDonem> KurumFinansalDonemler { get; set; }
    public DbSet<Ref_GorevliDurum> Ref_GorevliDurums { get; set; }
    public DbSet<Ref_SozlesmeTip> Ref_SozlesmeTips { get; set; }
    public DbSet<Ref_KurumTuru> Ref_KurumTurus { get; set; }
    public DbSet<GorevGecmisi> GorevGecmisleri { get; set; }
    public DbSet<GorevliNot> GorevliNotlari { get; set; }
        public DbSet<KurumButce> KurumButceler { get; set; }
        public DbSet<KurumButcePeriod> KurumButcePeriods { get; set; }
        public DbSet<KurumHavuzTakibi> KurumHavuzTakibiSet { get; set; }
    public DbSet<GorevlendirmeNot> GorevlendirmeNotlari { get; set; }
    
    public DbSet<Ref_Unvan> Ref_Unvans { get; set; }
    public DbSet<Ref_EgitimDurumu> Ref_EgitimDurumlari { get; set; }
    public DbSet<Ref_HafizlikDurumu> Ref_HafizlikDurumlari { get; set; }
    public DbSet<Ref_KanGrubu> Ref_KanGruplari { get; set; }

    public DbSet<DernekUye> DernekUyeleri { get; set; }
    public DbSet<Ref_AskerlikDurumu> Ref_AskerlikDurumlari { get; set; }
    public DbSet<Ref_KadroTuru> Ref_KadroTurleri { get; set; }

    // Dynamic Lookups
    public DbSet<LookupType> LookupTypes { get; set; }
    public DbSet<LookupValue> LookupValues { get; set; }

    // Dynamic Security System
    public DbSet<RoleTemplate> RoleTemplates { get; set; }
    public DbSet<RoleTemplateClaim> RoleTemplateClaims { get; set; }
    public DbSet<UserRoleTemplate> UserRoleTemplates { get; set; }
    public DbSet<UserClaimOverride> UserClaimOverrides { get; set; }
    public DbSet<HelpTopic> HelpTopics { get; set; }
    public DbSet<Kurbanlik> Kurbanliklar { get; set; }
    public DbSet<Hissedar> Hissedarlar { get; set; }
    public DbSet<KurbanCampaignRecord> KurbanCampaignRecords { get; set; }
    public DbSet<DashboardPreference> DashboardPreferences { get; set; }
    public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
    public DbSet<Ref_YonetimRol> Ref_YonetimRols { get; set; }
    public DbSet<KurumYonetimKuruluUyesi> KurumYonetimKuruluUyeleri { get; set; }

    // DİBBYS Alsace Subsystem Tables
    public DbSet<GorevliIzin> GorevliIzinler { get; set; }
    public DbSet<KurumKasaOdenek> KurumKasaOdenekler { get; set; }
    public DbSet<GorevliFaaliyetRaporu> GorevliFaaliyetRaporlari { get; set; }

    // Enterprise Gap-Fill Subsystems
    public DbSet<KurumDocument> KurumDocuments { get; set; }
    public DbSet<BudgetRevision> BudgetRevisions { get; set; }
    public DbSet<OverdueNotification> OverdueNotifications { get; set; }

    // Schema Extension — Dernek & Görevli
    public DbSet<DernekNot> DernekNotlari { get; set; }
    public DbSet<DernekGorsel> DernekGorselleri { get; set; }
    public DbSet<GorevliBelge> GorevliBelgeleri { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global Soft-Delete Query Filters
        builder.Entity<Kurum>().HasQueryFilter(k => !k.IsDeleted);
        builder.Entity<Gorevli>().HasQueryFilter(g => !g.IsDeleted);
        builder.Entity<Gorevlendirme>().HasQueryFilter(gv => !gv.IsDeleted);
        builder.Entity<Ref_YonetimRol>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<KurumYonetimKuruluUyesi>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<GorevliNot>().HasQueryFilter(n => !n.IsDeleted);
        builder.Entity<GorevlendirmeNot>().HasQueryFilter(n => !n.IsDeleted);
        builder.Entity<GorevliIzin>().HasQueryFilter(i => !i.IsDeleted);
        builder.Entity<KurumKasaOdenek>().HasQueryFilter(o => !o.IsDeleted);
        builder.Entity<GorevliFaaliyetRaporu>().HasQueryFilter(f => !f.IsDeleted);
        builder.Entity<DernekNot>().HasQueryFilter(n => !n.IsDeleted);
        builder.Entity<DernekGorsel>().HasQueryFilter(g => !g.IsDeleted);
        builder.Entity<GorevliBelge>().HasQueryFilter(b => !b.IsDeleted);

        // Configure Lookups
        builder.Entity<LookupType>()
            .HasIndex(lt => lt.Code)
            .IsUnique();

        // Configure Gorevlendirme relationships
        builder.Entity<Gorevlendirme>()
            .HasOne(g => g.Gorevli)
            .WithMany(gov => gov.Gorevlendirmeler)
            .HasForeignKey(g => g.GorevliId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Gorevlendirme>()
            .HasOne(g => g.YerineGelecekGorevli)
            .WithMany()
            .HasForeignKey(g => g.YerineGelecekGorevliId)
            .OnDelete(DeleteBehavior.Restrict);



        // Configure Explicit Relationships
        builder.Entity<GorevGecmisi>()
            .HasOne(g => g.Gorevli)
            .WithMany(g => g.GorevGecmisleri)
            .HasForeignKey(g => g.GorevliId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GorevGecmisi>()
            .HasOne(g => g.YerineGelenGorevli)
            .WithMany()
            .HasForeignKey(g => g.YerineGelenGorevliId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Kurbanlik>(entity =>
        {
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18,2)");
        });

        builder.Entity<DashboardPreference>(entity =>
        {
            entity.HasKey(e => e.UserId);
        });

        // Performance Indexes
        builder.Entity<Gorevli>()
            .HasIndex(g => new { g.Ad, g.Soyad, g.Email })
            .HasDatabaseName("IX_Gorevli_Search");

        builder.Entity<Gorevli>()
            .HasIndex(g => g.TCKimlikNo)
            .IsUnique()
            .HasFilter("[TCKimlikNo] IS NOT NULL");

        builder.Entity<Kurum>()
            .HasIndex(k => new { k.Bolge, k.Sehir })
            .HasDatabaseName("IX_Kurum_Geo");

        builder.Entity<Gorevlendirme>()
            .HasIndex(g => new { g.Tarih, g.BitisTarihi, g.KurumId, g.GorevliId })
            .HasDatabaseName("IX_Gorevlendirme_Filters");

        builder.Entity<KurumYonetimKuruluUyesi>()
            .HasOne(m => m.Kurum)
            .WithMany(k => k.YonetimKuruluUyeleri)
            .HasForeignKey(m => m.KurumId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<KurumYonetimKuruluUyesi>()
            .HasOne(m => m.YonetimRol)
            .WithMany()
            .HasForeignKey(m => m.YonetimRolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Ref_YonetimRol>().HasData(
            new Ref_YonetimRol { Id = 1, Ad = "Başkan", IsDeleted = false },
            new Ref_YonetimRol { Id = 2, Ad = "Sekreter", IsDeleted = false },
            new Ref_YonetimRol { Id = 3, Ad = "Muhasip", IsDeleted = false },
            new Ref_YonetimRol { Id = 4, Ad = "Üye", IsDeleted = false }
        );

        // DİBBYS Entity Configurations
        builder.Entity<KurumKasaOdenek>(entity =>
        {
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Kurum)
                .WithMany()
                .HasForeignKey(e => e.KurumId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TargetGorevli)
                .WithMany()
                .HasForeignKey(e => e.TargetGorevliId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GorevliIzin>(entity =>
        {
            entity.HasOne(e => e.Gorevli)
                .WithMany(g => g.Izinler)
                .HasForeignKey(e => e.GorevliId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<GorevliFaaliyetRaporu>(entity =>
        {
            entity.HasOne(e => e.Gorevli)
                .WithMany(g => g.FaaliyetRaporlari)
                .HasForeignKey(e => e.GorevliId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Kurum)
                .WithMany()
                .HasForeignKey(e => e.KurumId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Gorevli>(entity =>
        {
            entity.HasIndex(e => e.SicilNo)
                .IsUnique()
                .HasFilter("[SicilNo] IS NOT NULL")
                .HasDatabaseName("IX_Gorevli_SicilNo");
        });

        // Enterprise Gap-Fill: Document Management System
        builder.Entity<KurumDocument>(entity =>
        {
            entity.HasQueryFilter(d => !d.IsDeleted);
            entity.HasOne(d => d.Kurum)
                .WithMany(k => k.Documents)
                .HasForeignKey(d => d.KurumId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(d => new { d.KurumId, d.Category })
                .HasDatabaseName("IX_KurumDocument_KurumCategory");
        });

        // Enterprise Gap-Fill: Budget Revision Workflow
        builder.Entity<BudgetRevision>(entity =>
        {
            entity.HasOne(r => r.KurumButce)
                .WithMany(b => b.Revisions)
                .HasForeignKey(r => r.KurumButceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Enterprise Gap-Fill: Overdue Notification Engine
        builder.Entity<OverdueNotification>(entity =>
        {
            entity.HasOne(n => n.RelatedKurum)
                .WithMany()
                .HasForeignKey(n => n.RelatedKurumId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(n => n.RelatedGorevli)
                .WithMany()
                .HasForeignKey(n => n.RelatedGorevliId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(n => n.RelatedBudgetPeriod)
                .WithMany()
                .HasForeignKey(n => n.RelatedBudgetPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(n => n.RelatedDernekNot)
                .WithMany()
                .HasForeignKey(n => n.RelatedDernekNotId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(n => n.RelatedGorevliBelge)
                .WithMany()
                .HasForeignKey(n => n.RelatedGorevliBelgeId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(n => new { n.IsRead, n.Severity })
                .HasDatabaseName("IX_Notification_ReadSeverity");
        });
        // Schema Extension: DernekNot
        builder.Entity<DernekNot>(entity =>
        {
            entity.HasOne(n => n.Dernek)
                .WithMany(k => k.DernekNotlari)
                .HasForeignKey(n => n.DernekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Schema Extension: DernekGorsel
        builder.Entity<DernekGorsel>(entity =>
        {
            entity.HasOne(g => g.Dernek)
                .WithMany(k => k.DernekGorselleri)
                .HasForeignKey(g => g.DernekId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Schema Extension: GorevliBelge
        builder.Entity<GorevliBelge>(entity =>
        {
            entity.HasOne(b => b.Gorevli)
                .WithMany(g => g.Belgeler)
                .HasForeignKey(b => b.GorevliId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Property(b => b.BelgeTipi)
                .HasConversion<string>();
            entity.HasIndex(b => new { b.GorevliId, b.BelgeTipi })
                .HasDatabaseName("IX_GorevliBelge_GorevliBelgeTipi");
        });

        // Gorevli decimal fields
        builder.Entity<Gorevli>()
            .Property(g => g.Agno)
            .HasColumnType("decimal(3,2)");
    }

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        OnAfterSaveChangesSync(auditEntries);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(cancellationToken);
        await OnAfterSaveChangesAsync(auditEntries);
        return result;
    }

    private List<AuditTempEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditTempEntry>();
        
        var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is DatabaseAuditLogs || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            // Automate soft delete DeletedAt timestamp setting
            if (entry.Entity is ISoftDeletable softDeletable)
            {
                if (entry.State == EntityState.Modified)
                {
                    var isDeletedProp = entry.Property("IsDeleted");
                    if (isDeletedProp != null && isDeletedProp.IsModified)
                    {
                        if (softDeletable.IsDeleted)
                        {
                            softDeletable.DeletedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            softDeletable.DeletedAt = null;
                        }
                    }
                }
                else if (entry.State == EntityState.Added && softDeletable.IsDeleted)
                {
                    softDeletable.DeletedAt = DateTime.UtcNow;
                }
            }

            var auditEntry = new AuditTempEntry
            {
                EntityName = entry.Entity.GetType().Name,
                Username = username,
                Timestamp = DateTime.UtcNow
            };

            if (entry.State == EntityState.Added)
            {
                auditEntry.Action = "INSERT";
                var newValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.IsPrimaryKey()) continue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                auditEntry.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditEntry.Action = "DELETE";
                var oldValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                }
                auditEntry.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
            }
            else if (entry.State == EntityState.Modified)
            {
                auditEntry.Action = "UPDATE";
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();

                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                if (oldValues.Count > 0)
                {
                    auditEntry.OldValues = System.Text.Json.JsonSerializer.Serialize(oldValues);
                    auditEntry.NewValues = System.Text.Json.JsonSerializer.Serialize(newValues);
                }
                else
                {
                    continue;
                }
            }

            auditEntries.Add(auditEntry);
        }

        return auditEntries;
    }

    private void OnAfterSaveChangesSync(List<AuditTempEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var entry in auditEntries)
        {
            DatabaseAuditLogs.Add(new DatabaseAuditLogs
            {
                EntityName = entry.EntityName,
                Action = entry.Action,
                Username = entry.Username,
                Timestamp = entry.Timestamp,
                OldValues = entry.OldValues,
                NewValues = entry.NewValues
            });
        }

        base.SaveChanges();
    }

    private async Task OnAfterSaveChangesAsync(List<AuditTempEntry> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        foreach (var entry in auditEntries)
        {
            DatabaseAuditLogs.Add(new DatabaseAuditLogs
            {
                EntityName = entry.EntityName,
                Action = entry.Action,
                Username = entry.Username,
                Timestamp = entry.Timestamp,
                OldValues = entry.OldValues,
                NewValues = entry.NewValues
            });
        }

        await base.SaveChangesAsync();
    }

    private class AuditTempEntry
    {
        public string EntityName { get; set; } = "";
        public string Action { get; set; } = "";
        public string Username { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}