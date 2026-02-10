using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Models;

namespace DitibStasbourg.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Gorevli> Gorevli { get; set; }
    public DbSet<Kurum> Kurum { get; set; }
    public DbSet<Gorevlendirme> Gorevlendirme { get; set; }
    
    // New Entities
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
            new Kurum { Id = 1, Isim = "Strasbourg Yunus Emre Camii", Adres = "12 Rue de la Musau", Tip = KurumTip.Cami },
            new Kurum { Id = 2, Isim = "Bischheim Fatih Camii", Adres = "3 Rue des Écoles", Tip = KurumTip.Cami },
            new Kurum { Id = 3, Isim = "Strasbourg Türk Kültür Derneği", Adres = "5 Place Kléber", Tip = KurumTip.Dernek }
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
    }
}