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
    public DbSet<DashboardPreference> DashboardPreferences { get; set; }
    public DbSet<SystemAuditLog> SystemAuditLogs { get; set; }
    public DbSet<Ref_YonetimRol> Ref_YonetimRols { get; set; }
    public DbSet<KurumYonetimKuruluUyesi> KurumYonetimKuruluUyeleri { get; set; }

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

        // Seed Users (Personnel)
        builder.Entity<Gorevli>().HasData(
            new Gorevli { Id = 1, Ad = "Ahmet", Soyad = "Yılmaz", Email = "ahmet.yilmaz@example.com" },
            new Gorevli { Id = 2, Ad = "Mehmet", Soyad = "Demir", Email = "mehmet.demir@example.com" },
            new Gorevli { Id = 3, Ad = "Ayşe", Soyad = "Kaya", Email = "ayse.kaya@example.com" }
        );

        // Seed Institutions
        builder.Entity<Kurum>().HasData(
            new Kurum { Id = 1, Isim = "Strasbourg Yunus Emre Camii", Adres = "12 Rue de la Musau", Sehir = "Strasbourg", Tip = KurumTip.Cami, Latitude = 48.5661, Longitude = 7.7786 },
            new Kurum { Id = 2, Isim = "Bischheim Fatih Camii", Adres = "3 Rue des Écoles", Sehir = "Bischheim", Tip = KurumTip.Cami, Latitude = 48.6143, Longitude = 7.7491 },
            new Kurum { Id = 3, Isim = "Strasbourg Türk Kültür Derneği", Adres = "5 Place Kléber", Sehir = "Strasbourg", Tip = KurumTip.Dernek, Latitude = 48.5830, Longitude = 7.7478 }
        );

        // Seed Assignments
        builder.Entity<Gorevlendirme>().HasData(
            new Gorevlendirme { Id = 1, GorevliId = 1, KurumId = 1, Tarih = new DateTime(2023, 1, 1) },
            new Gorevlendirme { Id = 2, GorevliId = 2, KurumId = 2, Tarih = new DateTime(2023, 2, 1) },
            new Gorevlendirme { Id = 3, GorevliId = 3, KurumId = 3, Tarih = new DateTime(2023, 3, 1) }
        );

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