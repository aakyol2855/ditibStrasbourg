using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.EntityFrameworkCore;

namespace DitibStasbourg.Data
{
    public static class TestDataSeeder
    {
        public static async Task SeedTestDataAsync(ApplicationDbContext context)
        {
            // Clear existing data (keep Ref_ tables)
            await ClearExistingDataAsync(context);
            
            // Seed new test data
            await SeedKurumlarAsync(context);
            await SeedGorevlilerAsync(context);
            await SeedGorevlendirmelerAsync(context);
            await SeedDerneklerAsync(context);
            
            await context.SaveChangesAsync();
        }

        private static async Task ClearExistingDataAsync(ApplicationDbContext context)
        {
            // Clear in correct order (respecting foreign keys) ignoring global filters
            var uyeler = await context.DernekUyeleri.IgnoreQueryFilters().ToListAsync();
            context.DernekUyeleri.RemoveRange(uyeler);

            var gorevlendirmeler = await context.Gorevlendirme.IgnoreQueryFilters().ToListAsync();
            context.Gorevlendirme.RemoveRange(gorevlendirmeler);

            var gorevliler = await context.Gorevli.IgnoreQueryFilters().ToListAsync();
            context.Gorevli.RemoveRange(gorevliler);

            var kurumlar = await context.Kurum.IgnoreQueryFilters().ToListAsync();
            context.Kurum.RemoveRange(kurumlar);
            
            await context.SaveChangesAsync();
        }

        private static async Task SeedKurumlarAsync(ApplicationDbContext context)
        {
            var kurumlar = new List<Kurum>
            {
                new Kurum { Isim = "Strasbourg Merkez Camii", Tip = KurumTip.Cami, Sehir = "Strasbourg", 
                    Adres = "25 Rue du Travail, 67200 Strasbourg", AktifMi = true },
                new Kurum { Isim = "Meinau Camii", Tip = KurumTip.Cami, Sehir = "Strasbourg", 
                    Adres = "15 Rue de la Meinau, 67100 Strasbourg", AktifMi = true },
                new Kurum { Isim = "Neudorf Camii", Tip = KurumTip.Cami, Sehir = "Strasbourg", 
                    Adres = "30 Avenue du Neudorf, 67100 Strasbourg", AktifMi = true },
                new Kurum { Isim = "Mulhouse Camii", Tip = KurumTip.Cami, Sehir = "Mulhouse", 
                    Adres = "8 Rue de la Moselle, 68200 Mulhouse", AktifMi = true },
                new Kurum { Isim = "Colmar Camii", Tip = KurumTip.Cami, Sehir = "Colmar", 
                    Adres = "12 Rue des Vignerons, 68000 Colmar", AktifMi = true },
            };

            context.Kurum.AddRange(kurumlar);
            await context.SaveChangesAsync();
        }

        private static async Task SeedGorevlilerAsync(ApplicationDbContext context)
        {
            var durumIds = await context.Ref_GorevliDurums.Where(x => !x.IsDeleted).Select(x => x.Id).ToListAsync();
            var sozlesmeTipIds = await context.Ref_SozlesmeTips.Where(x => !x.IsDeleted).Select(x => x.Id).ToListAsync();
            var unvanIds = await context.Ref_Unvans.Where(x => !x.IsDeleted).Select(x => x.Id).ToListAsync();

            // Nullable IDs - if no records exist, set to null
            int? gorevliDurumId = durumIds.FirstOrDefault() != 0 ? durumIds.FirstOrDefault() : (int?)null;
            int? sozlesmeTipId = sozlesmeTipIds.FirstOrDefault() != 0 ? sozlesmeTipIds.FirstOrDefault() : (int?)null;
            int? imamUnvanId = unvanIds.FirstOrDefault() != 0 ? unvanIds.FirstOrDefault() : (int?)null;

            var gorevliler = new List<Gorevli>
            {
                new Gorevli { Ad = "Ahmet", Soyad = "Yılmaz", Email = "ahmet.yilmaz@diyanet.fr", CepTelefonu = "0612345678", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2018, 1, 15), DiyanetGirisTarihi = new DateTime(2015, 6, 1) },
                
                new Gorevli { Ad = "Mehmet", Soyad = "Kaya", Email = "mehmet.kaya@diyanet.fr", CepTelefonu = "0623456789", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2019, 3, 1), DiyanetGirisTarihi = new DateTime(2016, 9, 15) },
                
                new Gorevli { Ad = "Mustafa", Soyad = "Demir", Email = "mustafa.demir@diyanet.fr", CepTelefonu = "0634567890", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2020, 6, 1), DiyanetGirisTarihi = new DateTime(2018, 1, 1) },
                
                new Gorevli { Ad = "Ali", Soyad = "Şahin", Email = "ali.sahin@diyanet.fr", CepTelefonu = "0645678901", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2017, 9, 1), DiyanetGirisTarihi = new DateTime(2014, 3, 15) },
                
                new Gorevli { Ad = "Hasan", Soyad = "Çelik", Email = "hasan.celik@diyanet.fr", CepTelefonu = "0656789012", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2021, 1, 15), DiyanetGirisTarihi = new DateTime(2019, 6, 1) },
                
                new Gorevli { Ad = "İbrahim", Soyad = "Arslan", Email = "ibrahim.arslan@diyanet.fr", CepTelefonu = "0667890123", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2016, 5, 1), DiyanetGirisTarihi = new DateTime(2013, 10, 1) },
                
                new Gorevli { Ad = "Osman", Soyad = "Aydın", Email = "osman.aydin@diyanet.fr", CepTelefonu = "0678901234", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2022, 2, 1), DiyanetGirisTarihi = new DateTime(2020, 8, 15) },
                
                new Gorevli { Ad = "Yusuf", Soyad = "Özdemir", Email = "yusuf.ozdemir@diyanet.fr", CepTelefonu = "0689012345", 
                    Cinsiyet = "Erkek", GorevliDurumId = gorevliDurumId, SozlesmeTipId = sozlesmeTipId, UnvanId = imamUnvanId,
                    IlkGoreveBaslamaTarihi = new DateTime(2018, 11, 1), DiyanetGirisTarihi = new DateTime(2016, 4, 1) },
            };

            context.Gorevli.AddRange(gorevliler);
            await context.SaveChangesAsync();
        }

        private static async Task SeedGorevlendirmelerAsync(ApplicationDbContext context)
        {
            var gorevliler = await context.Gorevli.ToListAsync();
            var kurumlar = await context.Kurum.Where(k => k.Tip == KurumTip.Cami).ToListAsync();

            if (!gorevliler.Any() || !kurumlar.Any()) return;

            var gorevlendirmeler = new List<Gorevlendirme>();

            // Assign each görevli to a kurum
            for (int i = 0; i < gorevliler.Count && i < kurumlar.Count; i++)
            {
                gorevlendirmeler.Add(new Gorevlendirme
                {
                    GorevliId = gorevliler[i].Id,
                    KurumId = kurumlar[i % kurumlar.Count].Id,
                    Tarih = gorevliler[i].IlkGoreveBaslamaTarihi ?? DateTime.Now.AddYears(-2),
                    BitisTarihi = null // Active assignment
                });
            }

            context.Gorevlendirme.AddRange(gorevlendirmeler);
            await context.SaveChangesAsync();
        }

        private static async Task SeedDerneklerAsync(ApplicationDbContext context)
        {
            var ustKurumIds = await context.Ref_KurumTurus.Where(x => !x.IsDeleted).Select(x => x.Id).ToListAsync();
            int? ustKurumId = ustKurumIds.FirstOrDefault() != 0 ? ustKurumIds.FirstOrDefault() : (int?)null;

            var dernekler = new List<Kurum>
            {
                new Kurum { 
                    Isim = "Strasbourg Türk İslam Kültür Derneği", 
                    Tip = KurumTip.Dernek, 
                    Sehir = "Strasbourg",
                    Adres = "10 Rue de la République, 67000 Strasbourg",
                    UstKurumId = ustKurumId,
                    BaskonsoloslukBolgesi = "Strasbourg",
                    Bolge = "Grand Est",
                    KurulusKanunu = "1901",
                    CrmUyelikFormDurumu = "Var",
                    DernekBaskaniAd = "Hüseyin Acar",
                    DernekBaskaniIletisim = "0698765432",
                    DinGorevlisiAd = "Ahmet Yılmaz",
                    DinGorevlisiIletisim = "0612345678",
                    AktifMi = true
                },
                new Kurum { 
                    Isim = "Mulhouse Türk Kültür ve Dayanışma Derneği", 
                    Tip = KurumTip.Dernek, 
                    Sehir = "Mulhouse",
                    Adres = "5 Avenue de Colmar, 68200 Mulhouse",
                    UstKurumId = ustKurumId,
                    BaskonsoloslukBolgesi = "Strasbourg",
                    Bolge = "Grand Est",
                    KurulusKanunu = "1901",
                    CrmUyelikFormDurumu = "Beklemede",
                    DernekBaskaniAd = "Kemal Öztürk",
                    DernekBaskaniIletisim = "0687654321",
                    DinGorevlisiAd = "Mustafa Demir",
                    DinGorevlisiIletisim = "0634567890",
                    AktifMi = true
                },
                new Kurum { 
                    Isim = "Colmar İslam Cemiyeti", 
                    Tip = KurumTip.Dernek, 
                    Sehir = "Colmar",
                    Adres = "20 Rue des Augustins, 68000 Colmar",
                    UstKurumId = ustKurumId,
                    BaskonsoloslukBolgesi = "Strasbourg",
                    Bolge = "Grand Est",
                    KurulusKanunu = "1901",
                    CrmUyelikFormDurumu = "Yok",
                    DernekBaskaniAd = "Fatih Yıldız",
                    DernekBaskaniIletisim = "0676543210",
                    DinGorevlisiAd = "Ali Şahin",
                    DinGorevlisiIletisim = "0645678901",
                    AktifMi = true
                }
            };

            context.Kurum.AddRange(dernekler);
            await context.SaveChangesAsync();

            // Add members to dernekler
            var dernekUyeleri = new List<DernekUye>
            {
                // Strasbourg Dernek Üyeleri
                new DernekUye { KurumId = dernekler[0].Id, AdSoyad = "Mehmet Polat", Iletisim = "0611223344", AileUyeSayisi = 4, KayitTarihi = DateTime.Now.AddMonths(-6) },
                new DernekUye { KurumId = dernekler[0].Id, AdSoyad = "Ayşe Kara", Iletisim = "0622334455", AileUyeSayisi = 3, KayitTarihi = DateTime.Now.AddMonths(-3) },
                new DernekUye { KurumId = dernekler[0].Id, AdSoyad = "Zeynep Ak", Iletisim = "0633445566", AileUyeSayisi = 5, KayitTarihi = DateTime.Now.AddMonths(-1) },
                
                // Mulhouse Dernek Üyeleri
                new DernekUye { KurumId = dernekler[1].Id, AdSoyad = "Hasan Güler", Iletisim = "0644556677", AileUyeSayisi = 2, KayitTarihi = DateTime.Now.AddMonths(-8) },
                new DernekUye { KurumId = dernekler[1].Id, AdSoyad = "Fatma Yıldırım", Iletisim = "0655667788", AileUyeSayisi = 4, KayitTarihi = DateTime.Now.AddMonths(-4) },
                
                // Colmar Dernek Üyeleri
                new DernekUye { KurumId = dernekler[2].Id, AdSoyad = "İsmail Koç", Iletisim = "0666778899", AileUyeSayisi = 3, KayitTarihi = DateTime.Now.AddMonths(-2) },
                new DernekUye { KurumId = dernekler[2].Id, AdSoyad = "Elif Tunç", Iletisim = "0677889900", AileUyeSayisi = 6, KayitTarihi = DateTime.Now.AddMonths(-5) },
            };

            context.DernekUyeleri.AddRange(dernekUyeleri);
            await context.SaveChangesAsync();
        }
    }
}
