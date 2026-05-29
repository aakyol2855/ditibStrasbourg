using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;

namespace DitibStasbourg.Data
{
    public static class TestDataInitializer
    {
        public static async Task SeedMockDataAsync(ApplicationDbContext context)
        {
            if (await context.Kurum.AnyAsync(k => k.BaskonsoloslukBolgesi == "MOCK_DATA")) return;

            // 1. Mock Associations (Dernekler)
            var mockAssociations = new List<Kurum>
            {
                new Kurum { Isim = "DITIB Strasbourg Centre", Sehir = "Strasbourg", Bolge = "strasbourg-centre", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "1 Rue de l'Observatoire, 67000 Strasbourg" },
                new Kurum { Isim = "DITIB Neudorf", Sehir = "Strasbourg", Bolge = "strasbourg-centre", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "10 Rue de Neufeld, 67100 Strasbourg" },
                new Kurum { Isim = "DITIB Cronenbourg", Sehir = "Strasbourg", Bolge = "strasbourg-centre", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "5 Rue de l'Abbé Lemire, 67200 Strasbourg" },
                new Kurum { Isim = "DITIB Schiltigheim", Sehir = "Schiltigheim", Bolge = "schiltigheim", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "12 Rue de Wissembourg, 67300 Schiltigheim" },
                new Kurum { Isim = "DITIB Bischheim", Sehir = "Bischheim", Bolge = "bischheim", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "3 Rue de l'Industrie, 67800 Bischheim" },
                new Kurum { Isim = "DITIB Illkirch", Sehir = "Illkirch", Bolge = "illkirch", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "20 Route de Lyon, 67400 Illkirch-Graffenstaden" },
                new Kurum { Isim = "DITIB Lingolsheim", Sehir = "Lingolsheim", Bolge = "lingolsheim", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "45 Rue du Maréchal Foch, 67380 Lingolsheim" },
                new Kurum { Isim = "DITIB Haguenau", Sehir = "Haguenau", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "2 Rue du Général Gérard, 67500 Haguenau" },
                new Kurum { Isim = "DITIB Selestat", Sehir = "Selestat", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "15 Rue de l'Hôpital, 67600 Sélestat" },
                new Kurum { Isim = "DITIB Colmar", Sehir = "Colmar", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "30 Rue du Grillenbreit, 68000 Colmar" },
                new Kurum { Isim = "DITIB Mulhouse", Sehir = "Mulhouse", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "100 Rue de Bâle, 68100 Mulhouse" }
            };

            await context.Kurum.AddRangeAsync(mockAssociations);
            await context.SaveChangesAsync();

            // 2. Mock Personnel (Gorevliler)
            var names = new[] { "Ahmet", "Mehmet", "Ali", "Veli", "Ayşe", "Fatma", "Zeynep", "Mustafa", "Hüseyin", "İbrahim" };
            var surnames = new[] { "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Öztürk", "Aydın", "Özkan", "Aslan" };
            var bloodTypes = await context.Ref_KanGruplari.ToListAsync();
            var titles = await context.Ref_Unvans.ToListAsync();

            var mockStaff = new List<Gorevli>();
            var rnd = new Random();

            for (int i = 0; i < 50; i++)
            {
                var staff = new Gorevli
                {
                    Ad = names[rnd.Next(names.Length)],
                    Soyad = $"{surnames[rnd.Next(surnames.Length)]} (Test {i})",
                    Email = $"testuser{i}@example.com",
                    Cinsiyet = i % 3 == 0 ? "K" : "E",
                    KanGrubuId = bloodTypes.Count > 0 ? bloodTypes[rnd.Next(bloodTypes.Count)].Id : null,
                    UnvanId = titles.Count > 0 ? titles[rnd.Next(titles.Count)].Id : null,
                    TCKimlikNo = "1234567890" + (i % 10),
                    CepTelefonu = "+33 6 00 00 00 " + i.ToString("D2")
                };
                mockStaff.Add(staff);
            }

            await context.Gorevli.AddRangeAsync(mockStaff);
            await context.SaveChangesAsync();

            // 3. Mock Assignments
            var mockAssignments = new List<Gorevlendirme>();
            for (int i = 0; i < 30; i++)
            {
                var assignment = new Gorevlendirme
                {
                    GorevliId = mockStaff[rnd.Next(mockStaff.Count)].Id,
                    KurumId = mockAssociations[rnd.Next(mockAssociations.Count)].Id,
                    Tarih = DateTime.Now.AddMonths(-rnd.Next(1, 12)),
                    BitisTarihi = i % 5 == 0 ? DateTime.Now.AddMonths(-1) : DateTime.Now.AddMonths(rnd.Next(1, 24))
                };
                mockAssignments.Add(assignment);
            }

            await context.Gorevlendirme.AddRangeAsync(mockAssignments);
            await context.SaveChangesAsync();
        }

        public static async Task PurgeMockDataAsync(ApplicationDbContext context)
        {
            var mockAssocs = await context.Kurum.Where(k => k.BaskonsoloslukBolgesi == "MOCK_DATA").ToListAsync();
            var mockAssocIds = mockAssocs.Select(a => a.Id).ToList();

            var mockAssignments = await context.Gorevlendirme.Where(g => mockAssocIds.Contains(g.KurumId)).ToListAsync();
            context.Gorevlendirme.RemoveRange(mockAssignments);

            var mockStaff = await context.Gorevli.Where(g => g.Soyad.Contains("(Test ")).ToListAsync();
            context.Gorevli.RemoveRange(mockStaff);

            context.Kurum.RemoveRange(mockAssocs);

            await context.SaveChangesAsync();
        }
    }
}
