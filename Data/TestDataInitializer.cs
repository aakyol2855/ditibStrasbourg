using Microsoft.EntityFrameworkCore;
using DitibStasbourg.Data;
using DitibStasbourg.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DitibStasbourg.Data
{
    public static class TestDataInitializer
    {
        public static async Task SeedMockDataAsync(ApplicationDbContext context)
        {
            // First force-clean any corrupted ghost/empty/numeric rows from database
            await CleanCorruptedDataAsync(context);

            // ── Ghost-record repair: fix rows that were imported with Tip=Cami by mistake ──
            // Any Kurum row that carries a DernekBaskaniAd but was saved as KurumTip.Cami
            // was misclassified by an older import path. Repair them silently on every startup.
            await RepairGhostKurumTypesAsync(context);

            if (await context.Kurum.AnyAsync(k => k.BaskonsoloslukBolgesi == "MOCK_DATA")) return;

            // 1. Pristine Associations (Dernekler) from "DERNEK ASIL İSİM VE ADRESLERİ.xlsx"
            var mockAssociations = new List<Kurum>
            {
                new Kurum { Isim = "Association Culturelle Turque D'Altkirch", Sehir = "Altkirch", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "14a route de thann 68130 Altkirch", DernekBaskaniAd = "Ahmet Yılmaz", DernekBaskaniIletisim = "+33 6 12 34 56 78", IletisimNumarasi = "+33 3 89 11 22 33", Maili = "altkirch@ditib.fr", BaskanMail = "ahmet.yilmaz@altkirch-ditib.fr" },
                new Kurum { Isim = "Association Amicale et Culturelle Franco Turque de Bar-le-Duc", Sehir = "Bar-le-Duc", Bolge = "Meuse", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "31 rue des Romains 55000 Bar le Duc", DernekBaskaniAd = "Mehmet Demir", DernekBaskaniIletisim = "+33 6 23 45 67 89", IletisimNumarasi = "+33 3 29 22 33 44", Maili = "barleduc@ditib.fr", BaskanMail = "mehmet.demir@barleduc-ditib.fr" },
                new Kurum { Isim = "Association Culturelle Franco-Turque de Barr", Sehir = "Barr", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "26 Rue Paul Degermann 67140 BARR", DernekBaskaniAd = "Mustafa Yıldız", DernekBaskaniIletisim = "+33 6 34 56 78 90", IletisimNumarasi = "+33 3 88 33 44 55", Maili = "barr@ditib.fr", BaskanMail = "mustafa.yildiz@barr-ditib.fr" },
                new Kurum { Isim = "Association culturelle et cultuelle franco-turque de Benfeld", Sehir = "Huttenheim", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "6 Route de Strasbourg 67230 Huttenheim", DernekBaskaniAd = "Hasan Kaya", DernekBaskaniIletisim = "+33 6 45 67 89 01", IletisimNumarasi = "+33 3 88 44 55 66", Maili = "benfeld@ditib.fr", BaskanMail = "hasan.kaya@benfeld-ditib.fr" },
                new Kurum { Isim = "ASSOCIATION CULTURELLE TURQUE FRANÇAISE DE PLANOISE (Besançon)", Sehir = "Besançon", Bolge = "Doubs", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "1 Rue Louis Garnier 25000 BESANÇON", DernekBaskaniAd = "Yusuf Çelik", DernekBaskaniIletisim = "+33 6 56 78 90 12", IletisimNumarasi = "+33 3 81 55 66 77", Maili = "besancon@ditib.fr", BaskanMail = "yusuf.celik@besancon-ditib.fr" },
                new Kurum { Isim = "Association Culturelle Franco-Turque de Bischwiller", Sehir = "Bischwiller", Bolge = "Bas-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "38 rue de Rohrwiller 67240 Bischwiller", DernekBaskaniAd = "Ali Şahin", DernekBaskaniIletisim = "+33 6 67 89 01 23", IletisimNumarasi = "+33 3 88 66 77 88", Maili = "bischwiller@ditib.fr", BaskanMail = "ali.sahin@bischwiller-ditib.fr" },
                new Kurum { Isim = "Association culturelle Franco-Turque du pays de Bitche", Sehir = "Bitche", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "52 rue des Tilleuls 57230 Bitche", DernekBaskaniAd = "Hüseyin Aydın", DernekBaskaniIletisim = "+33 6 78 90 12 34", IletisimNumarasi = "+33 3 87 77 88 99", Maili = "bitche@ditib.fr", BaskanMail = "huseyin.aydin@bitche-ditib.fr" },
                new Kurum { Isim = "Association des travailleurs franco-turcs de Boulay-Moselle", Sehir = "Boulay-Moselle", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "26 Bis rue du General de Rascas 57220 BOULAY-MOSELLE", DernekBaskaniAd = "İbrahim Aslan", DernekBaskaniIletisim = "+33 6 89 01 23 45", IletisimNumarasi = "+33 3 87 88 99 00", Maili = "boulay@ditib.fr", BaskanMail = "ibrahim.aslan@boulay-ditib.fr" },
                new Kurum { Isim = "Association culturelle et cultuelle franco turque Bouzonville", Sehir = "Bouzonville", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "3 Impasse du Porche 57320 Bouzonville", DernekBaskaniAd = "Süleyman Koç", DernekBaskaniIletisim = "+33 6 90 12 34 56", IletisimNumarasi = "+33 3 87 99 00 11", Maili = "bouzonville@ditib.fr", BaskanMail = "suleyman.koc@bouzonville-ditib.fr" },
                new Kurum { Isim = "ASS FRANCO TURQUE DE BRUYERES", Sehir = "Bruyères", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "13 Rue De Verdun 88600 BRUYERES", DernekBaskaniAd = "Ömer Bulut", DernekBaskaniIletisim = "+33 6 01 23 45 67", IletisimNumarasi = "+33 3 29 00 11 22", Maili = "bruyeres@ditib.fr", BaskanMail = "omer.bulut@bruyeres-ditib.fr" },
                new Kurum { Isim = "Amicale franco turque de la plaine (Chatenois)", Sehir = "Châtenois", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "17 HLM Les Patureaux 88170 CHATENOIS", DernekBaskaniAd = "Osman Polat", DernekBaskaniIletisim = "+33 6 12 34 56 70", IletisimNumarasi = "+33 3 29 11 22 33", Maili = "chatenois@ditib.fr", BaskanMail = "osman.polat@chatenois-ditib.fr" },
                new Kurum { Isim = "Association franco-turque de Colmar", Sehir = "Colmar", Bolge = "Haut-Rhin", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "73 Rue de la Fecht 68000 Colmar", DernekBaskaniAd = "Zekeriya Özkan", DernekBaskaniIletisim = "+33 6 23 45 67 80", IletisimNumarasi = "+33 3 89 22 33 44", Maili = "colmar@ditib.fr", BaskanMail = "zekeriya.ozkan@colmar-ditib.fr" },
                new Kurum { Isim = "Association culturelle franco-turque de Creutzwald", Sehir = "Creutzwald", Bolge = "Moselle", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "31 Impasse du Boesader 57150 Creutzwald", DernekBaskaniAd = "Yakup Arslan", DernekBaskaniIletisim = "+33 6 34 56 78 91", IletisimNumarasi = "+33 3 87 33 44 55", Maili = "creutzwald@ditib.fr", BaskanMail = "yakup.arslan@creutzwald-ditib.fr" },
                new Kurum { Isim = "ASSOCIATION FRANCO-TURQUE DES VOSGES (Epinal)", Sehir = "Épinal", Bolge = "Vosges", Tip = KurumTip.Dernek, BaskonsoloslukBolgesi = "MOCK_DATA", Adres = "24 bis rue des villes jumelées Epinal 88000 Épinal", DernekBaskaniAd = "Mustafa Yılmaz", DernekBaskaniIletisim = "+33 6 45 67 89 02", IletisimNumarasi = "+33 3 29 44 55 66", Maili = "epinal@ditib.fr", BaskanMail = "mustafa.yilmaz@epinal-ditib.fr" }
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
        /// Repairs misclassified Kurum records that were imported through old pipelines
        /// which defaulted Tip to KurumTip.Cami (0) instead of KurumTip.Dernek (1).
        ///
        /// Detection rule: KurumTip.Cami rows that have a non-null DernekBaskaniAd
        /// are structurally association records, not mosques. This method reclassifies
        /// them to KurumTip.Dernek so they appear in the Dernek İşlemleri viewport.
        ///
        /// Idempotent: safe to run on every application startup.
        /// </summary>
        private static async Task RepairGhostKurumTypesAsync(ApplicationDbContext context)
        {
            var ghosts = await context.Kurum
                .Where(k => k.Tip == KurumTip.Cami && k.DernekBaskaniAd != null && k.DernekBaskaniAd != string.Empty)
                .ToListAsync();

            if (ghosts.Count == 0) return;

            foreach (var ghost in ghosts)
                ghost.Tip = KurumTip.Dernek;

            await context.SaveChangesAsync();
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
        public static async Task PurgeMockDataAsync(ApplicationDbContext context, IMemoryCache? cache = null)
        {
            if (cache is MemoryCache concreteCache)
            {
                concreteCache.Clear();
            }

            // Also clean up any corrupted data in addition to mock data
            await CleanCorruptedDataAsync(context, cache);

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

        public static async Task CleanCorruptedDataAsync(ApplicationDbContext context, IMemoryCache? cache = null)
        {
            // --- 1. Clean Corrupted Kurum entities ---
            var allKurum = await context.Kurum.IgnoreQueryFilters().ToListAsync();
            var corruptKurumIds = new List<int>();

            foreach (var k in allKurum)
            {
                bool isCorrupt = false;
                if (string.IsNullOrWhiteSpace(k.Isim))
                {
                    isCorrupt = true;
                }
                else
                {
                    var trimmed = k.Isim.Trim();
                    if (int.TryParse(trimmed, out _) || double.TryParse(trimmed, out _) || 
                        trimmed.Contains("Sıra No", StringComparison.OrdinalIgnoreCase) || 
                        trimmed.Contains("S.N.", StringComparison.OrdinalIgnoreCase) || 
                        trimmed.Equals("No", StringComparison.OrdinalIgnoreCase) || 
                        trimmed.Equals("S.No", StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrupt = true;
                    }
                }

                if (k.Tip != KurumTip.Cami && k.Tip != KurumTip.Dernek)
                {
                    isCorrupt = true;
                }

                if (isCorrupt)
                {
                    corruptKurumIds.Add(k.Id);
                }
            }

            if (corruptKurumIds.Count > 0)
            {
                var assignmentsToDelete = await context.Gorevlendirme.IgnoreQueryFilters()
                    .Where(g => corruptKurumIds.Contains(g.KurumId))
                    .ToListAsync();

                if (assignmentsToDelete.Count > 0)
                {
                    var assignmentIds = assignmentsToDelete.Select(g => g.Id).ToList();
                    var notesToDelete = await context.GorevlendirmeNotlari.IgnoreQueryFilters()
                        .Where(n => assignmentIds.Contains(n.GorevlendirmeId))
                        .ToListAsync();

                    context.GorevlendirmeNotlari.RemoveRange(notesToDelete);
                    context.Gorevlendirme.RemoveRange(assignmentsToDelete);
                }

                var dernekUyeleriToDelete = await context.DernekUyeleri.IgnoreQueryFilters()
                    .Where(du => corruptKurumIds.Contains(du.KurumId))
                    .ToListAsync();
                context.DernekUyeleri.RemoveRange(dernekUyeleriToDelete);

                var kurumToDelete = allKurum.Where(k => corruptKurumIds.Contains(k.Id)).ToList();
                context.Kurum.RemoveRange(kurumToDelete);
            }

            // --- 2. Clean Corrupted Gorevli entities ---
            var allGorevli = await context.Gorevli.IgnoreQueryFilters().ToListAsync();
            var corruptGorevliIds = new List<int>();

            foreach (var g in allGorevli)
            {
                bool isCorrupt = false;
                if (string.IsNullOrWhiteSpace(g.Ad) || string.IsNullOrWhiteSpace(g.Soyad))
                {
                    isCorrupt = true;
                }
                else
                {
                    var adTrim = g.Ad.Trim();
                    var soyTrim = g.Soyad.Trim();
                    if (int.TryParse(adTrim, out _) || int.TryParse(soyTrim, out _) ||
                        adTrim.Contains("Sıra No", StringComparison.OrdinalIgnoreCase) ||
                        soyTrim.Contains("Sıra No", StringComparison.OrdinalIgnoreCase) ||
                        adTrim.Contains("S.N.", StringComparison.OrdinalIgnoreCase) ||
                        soyTrim.Contains("S.N.", StringComparison.OrdinalIgnoreCase) ||
                        adTrim.Equals("Ad", StringComparison.OrdinalIgnoreCase) ||
                        soyTrim.Equals("Soyad", StringComparison.OrdinalIgnoreCase))
                    {
                        isCorrupt = true;
                    }
                }

                if (isCorrupt)
                {
                    corruptGorevliIds.Add(g.Id);
                }
            }

            if (corruptGorevliIds.Count > 0)
            {
                var assignmentsToDelete = await context.Gorevlendirme.IgnoreQueryFilters()
                    .Where(g => corruptGorevliIds.Contains(g.GorevliId) || (g.YerineGelecekGorevliId.HasValue && corruptGorevliIds.Contains(g.YerineGelecekGorevliId.Value)))
                    .ToListAsync();

                if (assignmentsToDelete.Count > 0)
                {
                    var assignmentIds = assignmentsToDelete.Select(g => g.Id).ToList();
                    var notesToDelete = await context.GorevlendirmeNotlari.IgnoreQueryFilters()
                        .Where(n => assignmentIds.Contains(n.GorevlendirmeId))
                        .ToListAsync();

                    context.GorevlendirmeNotlari.RemoveRange(notesToDelete);
                    context.Gorevlendirme.RemoveRange(assignmentsToDelete);
                }

                var historyToDelete = await context.GorevGecmisleri.IgnoreQueryFilters()
                    .Where(h => corruptGorevliIds.Contains(h.GorevliId) || (h.YerineGelenGorevliId.HasValue && corruptGorevliIds.Contains(h.YerineGelenGorevliId.Value)))
                    .ToListAsync();
                context.GorevGecmisleri.RemoveRange(historyToDelete);

                var notesToDelete2 = await context.GorevliNotlari.IgnoreQueryFilters()
                    .Where(n => corruptGorevliIds.Contains(n.GorevliId))
                    .ToListAsync();
                context.GorevliNotlari.RemoveRange(notesToDelete2);

                var gorevliToDelete = allGorevli.Where(g => corruptGorevliIds.Contains(g.Id)).ToList();
                context.Gorevli.RemoveRange(gorevliToDelete);
            }

            await context.SaveChangesAsync();

            if (cache is MemoryCache concreteCache)
            {
                concreteCache.Clear();
            }
        }
    }
}
