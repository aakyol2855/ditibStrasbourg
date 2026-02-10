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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
            new Gorevlendirme { Id = 1, GorevliId = 1, KurumId = 1, Tarih = new DateTime(2023, 1, 5) },
            new Gorevlendirme { Id = 2, GorevliId = 2, KurumId = 2, Tarih = new DateTime(2023, 2, 20) },
            new Gorevlendirme { Id = 3, GorevliId = 3, KurumId = 3, Tarih = new DateTime(2023, 3, 15) }
        );
    }
}