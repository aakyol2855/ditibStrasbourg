# DİBBYS Strasbourg – Finansal Kampanya, Bütçe & Havuz Yönetimi Subsystem Özeti

## 1. Genel Mimari Yapısı
- **Platform**: .NET 10, monolitik ASP.NET Core MVC uygulaması.  
- **Soft Delete**: `ISoftDeletable` arayüzü tüm iş varlıklarında (`Kurum`, `KurumButce`, `KurumButcePeriod`, `KurumHavuzTakibi` vb.) uygulanıyor; `IsDeleted` ve `DeletedAt` alanları global sorgu filtreleri ile otomatik hariç tutulur.  
- **Dependency Injection**: Tüm servisler (`IDibbysPdfEngine`, `MaliyeController`, `ResetAndSeedMockData` gibi) `Program.cs` içinde `AddScoped`/`AddTransient` ile kaydedildi.  
- **Kimlik & Yetkilendirme**: ASP.NET Identity-tabanlı kullanıcı/rol yönetimi korunur; yeni claim’lar `maliyeRead` ve `maliyeWrite` eklendi.

## 2. Veritabanı Şeması ve Tablo İlişkileri
```
Kurum
 ├─ KurumButce (Yıl, TotalBudget, DitibContribution = %80, DernekContribution = %20)
 │    └─ KurumButcePeriod (4 kayıt, tarihler: 20-Jan, 5-Mar, 6-Jul, 10-Oct; ScheduledAmount = DitibContribution/4)
 ├─ KurumHavuzTakibi (Yıl, PersonnelGender, VariableAmount, IsSettled)
 ├─ Gorevlendirme (tarihler, aktif/geçmiş)
 └─ … (diğer mevcut ilişkiler)

Gorevli
 ├─ GorevliIzin (izin kayıtları, OnayDurumu)
 └─ GorevliFaaliyetRaporu
```
- **Foreign Keys**: Tüm ilişkiler `Restrict` silme kuralı ile tanımlanmıştır; silme işlemleri doğru topolojik sırada (çocuk → ebeveyn) gerçekleştirilir.  
- **Enum Tipleri**: `PersonnelGender`, `PasaportTuru`, `EsDurumu`, `IzinTuru`, `OnayDurumu` vb. tipleri veri bütünlüğünü artırır.

## 3. Güvenlik ve Yetkilendirme Matrisi
| Rol / Claim | Yetki Alanı | Açıklama |
|------------|--------------|----------|
| **SuperAdmin** | `Roles = "SuperAdmin"` | `ResetAndSeedMockData` endpoint’ine tam erişim. |
| **MaliyeStaffOnly** | Policy `maliyeRead` | Maliye menüsü (`MaliyeController`) tüm okuma eylemleri. |
| **maliyeWrite** | Claim `maliyeWrite` | `MarkPaid` AJAX çağrısı ve ödeme durumu güncellemeleri. |
| **GorevliUser** (row-level) | Row‑level filter `GorevliPortalAccessFilterAttribute` | Kullanıcı sadece kendi `GorevliId` ile ilişkili kayıtları görebilir. |
| **Identity Users** | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` | **Hiç dokunulmaz**, veri silme/seed işlemleri bu tabloları dışarıda tutar. |

## 4. İş Mantığı ve Formüller
- **Bütçe Bölme (80 / 20)**:
  ```
  DitibContribution = TotalBudget * 0.80
  DernekContribution = TotalBudget * 0.20
  ```
- **Dönem Takvimi**: Her `KurumButce` için dört taksit (`KurumButcePeriod`) otomatik oluşturulur; tarihleri sabit (Jan 20, Mar 5, Jul 06, Oct 10) ve `ScheduledAmount = DitibContribution / 4`.
- **Kalan Bakiye**:
  ```
  Remaining = DitibContribution - Σ(ScheduledAmount where IsPaid = true)
  ```
- **Excel İhracı**: ClosedXML ile **Dernek Ismi | IBAN No | Dönem Tutarı | Ödeme Durumu** başlıklarıyla çalışma kitabı üretilir ve byte‑stream olarak indirilir.
- **Mock Data Seeding**: `ResetAndSeedMockData` işlem akışı:
  1. Çocuk tabloları (`KurumButcePeriod`, `KurumHavuzTakibi`, `GorevliIzin` …) siler.
  2. Ana tablolar (`Kurum`, `Gorevli`) temizler.
  3. Gerçekçi Fransız cami kurumları (BARR, BENFELD, BISCHWILLER, ALTKIRCH, MULHOUSE, SELESTAT) ve ilgili IBAN/SIRET/RNA değerleri ekler.
  4. Personel profilleri (Mehmet Garip İnanlı, Halil Acabay, Rasim Var vb.) ve atamaları oluşturur.
  5. 2024‑2026 yılları için yıllık bütçeler ve 4‑taksit takvimleri oluşturur; her bütçe için 2‑3 taksit `IsPaid = true` olarak işaretlenir.
  6. Havuz takibi kayıtları (cinsiyete göre 1200 € değişken tutar) eklenir.
  7. Transaction içinde commit/rollback mantığıyla bütünlük sağlanır.
```
