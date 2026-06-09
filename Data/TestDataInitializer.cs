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

            // 1. Pristine Associations (Dernekler) from "DERNEK ASIL İSİM VE ADRESLERİ.xlsx"
            var mockAssociations = new List<Kurum>
            {
                new Kurum { Isim = "Association Culturelle Turque D'Altkirch", Sehir = "Altkirch", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "14a route de thann 68130 Altkirch" },
                new Kurum { Isim = "Association Amicale et Culturelle Franco Turque de Bar-le-Duc", Sehir = "Bar-le-Duc", Bolge = "Meuse", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "31 rue des Romains 55000 Bar le Duc" },
                new Kurum { Isim = "Association Culturelle Franco-Turque de Barr", Sehir = "Barr", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "26 Rue Paul Degermann 67140 BARR" },
                new Kurum { Isim = "Association culturelle et cultuelle franco-turque de Benfeld", Sehir = "Huttenheim", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "6 Route de Strasbourg 67230 Huttenheim" },
                new Kurum { Isim = "ASSOCIATION CULTURELLE TURQUE FRANÇAISE DE PLANOISE (Besançon)", Sehir = "Besançon", Bolge = "Doubs", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "1 Rue Louis Garnier 25000 BESANÇON" },
                new Kurum { Isim = "Association Culturelle Franco-Turque de Bischwiller", Sehir = "Bischwiller", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "38 rue de Rohrwiller 67240 Bischwiller" },
                new Kurum { Isim = "Association culturelle Franco-Turque du pays de Bitche", Sehir = "Bitche", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "52 rue des Tilleuls 57230 Bitche" },
                new Kurum { Isim = "Association des travailleurs franco-turcs de Boulay-Moselle", Sehir = "Boulay-Moselle", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "26 Bis rue du General de Rascas 57220 BOULAY-MOSELLE" },
                new Kurum { Isim = "Association culturelle et cultuelle franco turque Bouzonville", Sehir = "Bouzonville", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "3 Impasse du Porche 57320 Bouzonville" },
                new Kurum { Isim = "ASS FRANCO TURQUE DE BRUYERES", Sehir = "Bruyères", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "13 Rue De Verdun 88600 BRUYERES" },
                new Kurum { Isim = "Amicale franco turque de la plaine (Chatenois)", Sehir = "Châtenois", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "17 HLM Les Patureaux 88170 CHATENOIS" },
                new Kurum { Isim = "Association franco-turque de Colmar", Sehir = "Colmar", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "73 Rue de la Fecht 68000 Colmar" },
                new Kurum { Isim = "Association culturelle franco-turque de Creutzwald", Sehir = "Creutzwald", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "31 Impasse du Boesader 57150 Creutzwald" },
                new Kurum { Isim = "ASSOCIATION FRANCO-TURQUE DES VOSGES (Epinal)", Sehir = "Épinal", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "24 bis rue des villes jumelées Epinal 88000 Épinal" }
            };

            await context.Kurum.AddRangeAsync(mockAssociations);
            await context.SaveChangesAsync();

            // 2. Mock Personnel (Gorevliler) - Professional, realistic names
            var firstNames = new[] { "Ahmet", "Mehmet", "Ali", "Veli", "Hasan", "Hüseyin", "Yusuf", "Mustafa", "İbrahim", "Hamza", "Ömer", "Osman", "Süleyman", "Zekeriya", "Yakup" };
            var lastNames = new[] { "Yılmaz", "Kaya", "Demir", "Çelik", "Şahin", "Yıldız", "Öztürk", "Aydın", "Özkan", "Aslan", "Polat", "Bulut", "Koç", "Yıldırım", "Arslan" };
            var bloodTypes = await context.Ref_KanGruplari.ToListAsync();
            var titles = await context.Ref_Unvans.ToListAsync();

            var mockStaff = new List<Gorevli>();
            var rnd = new Random();

            for (int i = 0; i < 30; i++)
            {
                var fName = firstNames[rnd.Next(firstNames.Length)];
                var lName = lastNames[rnd.Next(lastNames.Length)];
                var staff = new Gorevli
                {
                    Ad = fName,
                    Soyad = lName,
                    Email = $"{fName.ToLower()}.{lName.ToLower()}{i}@ditibstrasbourg.fr",
                    Cinsiyet = i % 4 == 0 ? "K" : "E",
                    KanGrubuId = bloodTypes.Count > 0 ? bloodTypes[rnd.Next(bloodTypes.Count)].Id : null,
                    UnvanId = titles.Count > 0 ? titles[rnd.Next(titles.Count)].Id : null,
                    TCKimlikNo = "100200300" + i.ToString("D2"),
                    CepTelefonu = "+33 6 11 22 33 " + i.ToString("D2")
                };
                mockStaff.Add(staff);
            }

            await context.Gorevli.AddRangeAsync(mockStaff);
            await context.SaveChangesAsync();

            // 3. Mock Assignments (Gorevlendirmeler)
            var mockAssignments = new List<Gorevlendirme>();
            for (int i = 0; i < 20; i++)
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

            // 4. Mock System Audit Logs - Professional, corporate-grade baseline
            if (!await context.SystemAuditLogs.AnyAsync())
            {
                var mockLogs = new List<SystemAuditLog>
                {
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-5), LogLevel = "Information", Username = "System_Deamon", Action = "Sistem başarıyla başlatıldı ve güvenli çalışma ortamı oluşturuldu.", IpAddress = "127.0.0.1", Component = "ApplicationDbContext" },
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-12), LogLevel = "Information", Username = "System_Deamon", Action = "DİTİB Strasbourg veri entegrasyonu ve şube şablonları başarıyla yüklendi.", IpAddress = "127.0.0.1", Component = "TestDataInitializer" },
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-15), LogLevel = "Information", Username = "aakyol", Action = "Güvenli bağlantı ve şube yetkilendirmesi sağlandı.", IpAddress = "192.168.1.100", Component = "AdminController" },
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-20), LogLevel = "Information", Username = "System_Deamon", Action = "Erişim kontrol kuralları ve rol şablonları güncellendi.", IpAddress = "127.0.0.1", Component = "DynamicClaimsTransformation" },
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-30), LogLevel = "Information", Username = "koroglu", Action = "Veritabanı denetim izi (Audit Trail) izleme servisi aktifleştirildi.", IpAddress = "192.168.1.105", Component = "ISystemAuditLogService" },
                    new SystemAuditLog { Timestamp = DateTime.UtcNow.AddMinutes(-45), LogLevel = "Information", Username = "System_Deamon", Action = "Excel parça parça veri içe aktarma kanalı (Memory-Optimized Pipeline) hazırlandı.", IpAddress = "127.0.0.1", Component = "DataMaintenanceService" }
                };
                await context.SystemAuditLogs.AddRangeAsync(mockLogs);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Purges all data seeded by <see cref="SeedMockDataAsync"/> from the database.
        ///
        /// Deletion order strictly follows the foreign-key dependency graph to avoid
        /// SqlException "REFERENCE constraint conflict" crashes:
        ///
        ///   Level 1 — leaf children (no dependents):
        ///     GorevlendirmeNot  (→ Gorevlendirme)
        ///     GorevliNot        (→ Gorevli)
        ///
        ///   Level 2 — intermediate children (Restrict FKs must be nullified first):
        ///     GorevGecmisi.YerineGelenGorevliId  [Restrict] → nullified before row delete
        ///     Gorevlendirme.YerineGelecekGorevliId [Restrict] → nullified before row delete
        ///
        ///   Level 3 — parent rows:
        ///     Gorevlendirme, GorevGecmisi, Gorevli
        ///
        ///   Level 4 — association children then parents:
        ///     DernekUyeleri → Kurum
        ///
        ///   Level 5 — isolated tables:
        ///     SystemAuditLogs (bulk ExecuteDeleteAsync — no FK references)
        /// </summary>
        public static async Task PurgeMockDataAsync(ApplicationDbContext context)
        {
            // ── Identify mock roots ───────────────────────────────────────────
            // Mock Kurum rows are tagged with BaskonsoloslukBolgesi == "MOCK_DATA"
            var mockAssocIds = await context.Kurum
                .Where(k => k.BaskonsoloslukBolgesi == "MOCK_DATA")
                .Select(k => k.Id)
                .ToListAsync();

            // Mock Gorevli rows were seeded with Id > 3 (first 3 are HasData seeds)
            var mockStaffIds = await context.Gorevli
                .Where(g => g.Id > 3)
                .Select(g => g.Id)
                .ToListAsync();

            if (mockAssocIds.Count == 0 && mockStaffIds.Count == 0)
                return; // Nothing to purge

            // ── Wrap everything in a single atomic transaction ────────────────
            await using var tx = await context.Database.BeginTransactionAsync();
            try
            {
                // ── Level 1a: GorevlendirmeNot ─────────────────────────────────
                // These are children of Gorevlendirme rows that reference mock staff
                // or mock associations. Must be removed before their parent Gorevlendirme.
                var gorevlendirmeIdsToDelete = await context.Gorevlendirme
                    .Where(g => mockStaffIds.Contains(g.GorevliId)
                             || (g.YerineGelecekGorevliId.HasValue && mockStaffIds.Contains(g.YerineGelecekGorevliId.Value))
                             || mockAssocIds.Contains(g.KurumId))
                    .Select(g => g.Id)
                    .ToListAsync();

                if (gorevlendirmeIdsToDelete.Count > 0)
                {
                    var gorevlendirmeNotlar = await context.GorevlendirmeNotlari
                        .Where(n => gorevlendirmeIdsToDelete.Contains(n.GorevlendirmeId))
                        .ToListAsync();
                    context.GorevlendirmeNotlari.RemoveRange(gorevlendirmeNotlar);
                    await context.SaveChangesAsync();
                }

                // ── Level 1b: GorevliNot ───────────────────────────────────────
                if (mockStaffIds.Count > 0)
                {
                    var gorevliNotlar = await context.GorevliNotlari
                        .Where(n => mockStaffIds.Contains(n.GorevliId))
                        .ToListAsync();
                    context.GorevliNotlari.RemoveRange(gorevliNotlar);
                    await context.SaveChangesAsync();
                }

                // ── Level 2a: Nullify GorevGecmisi.YerineGelenGorevliId [Restrict] ──
                // Cannot delete a Gorevli while another GorevGecmisi row points to it
                // via the Restrict-constrained YerineGelenGorevliId column.
                if (mockStaffIds.Count > 0)
                {
                    var gecmislerWithRestrictRef = await context.GorevGecmisleri
                        .Where(g => g.YerineGelenGorevliId.HasValue
                                 && mockStaffIds.Contains(g.YerineGelenGorevliId.Value))
                        .ToListAsync();

                    foreach (var gecmis in gecmislerWithRestrictRef)
                        gecmis.YerineGelenGorevliId = null;

                    if (gecmislerWithRestrictRef.Count > 0)
                        await context.SaveChangesAsync();

                    // Now delete ALL GorevGecmisi rows owned by mock staff
                    var ownedGecmisler = await context.GorevGecmisleri
                        .Where(g => mockStaffIds.Contains(g.GorevliId))
                        .ToListAsync();
                    context.GorevGecmisleri.RemoveRange(ownedGecmisler);
                    await context.SaveChangesAsync();
                }

                // ── Level 2b: Nullify Gorevlendirme.YerineGelecekGorevliId [Restrict] ──
                // This self-referencing FK on Gorevlendirme also carries Restrict behavior.
                if (mockStaffIds.Count > 0)
                {
                    var assignmentsWithRestrictRef = await context.Gorevlendirme
                        .Where(g => g.YerineGelecekGorevliId.HasValue
                                 && mockStaffIds.Contains(g.YerineGelecekGorevliId.Value))
                        .ToListAsync();

                    foreach (var a in assignmentsWithRestrictRef)
                        a.YerineGelecekGorevliId = null;

                    if (assignmentsWithRestrictRef.Count > 0)
                        await context.SaveChangesAsync();
                }

                // ── Level 3a: Gorevlendirme ────────────────────────────────────
                // All Restrict references are now nullified. Safe to delete.
                if (gorevlendirmeIdsToDelete.Count > 0)
                {
                    var gorevlendirmeler = await context.Gorevlendirme
                        .Where(g => gorevlendirmeIdsToDelete.Contains(g.Id))
                        .ToListAsync();
                    context.Gorevlendirme.RemoveRange(gorevlendirmeler);
                    await context.SaveChangesAsync();
                }

                // ── Level 3b: Gorevli ──────────────────────────────────────────
                if (mockStaffIds.Count > 0)
                {
                    var mockStaff = await context.Gorevli
                        .Where(g => mockStaffIds.Contains(g.Id))
                        .ToListAsync();
                    context.Gorevli.RemoveRange(mockStaff);
                    await context.SaveChangesAsync();
                }

                // ── Level 4a: DernekUyeleri ────────────────────────────────────
                if (mockAssocIds.Count > 0)
                {
                    var uyeler = await context.DernekUyeleri
                        .Where(u => mockAssocIds.Contains(u.KurumId))
                        .ToListAsync();
                    context.DernekUyeleri.RemoveRange(uyeler);
                    await context.SaveChangesAsync();

                    // ── Level 4b: Kurum ────────────────────────────────────────
                    var mockAssocs = await context.Kurum
                        .Where(k => mockAssocIds.Contains(k.Id))
                        .ToListAsync();
                    context.Kurum.RemoveRange(mockAssocs);
                    await context.SaveChangesAsync();
                }

                // ── Level 5: SystemAuditLogs ───────────────────────────────────
                // No FK dependents. Use ExecuteDeleteAsync to avoid loading
                // the entire log table into memory.
                await context.SystemAuditLogs.ExecuteDeleteAsync();

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
