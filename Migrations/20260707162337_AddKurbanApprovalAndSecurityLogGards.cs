using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurbanApprovalAndSecurityLogGards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM GorevlendirmeNotlari;");
            migrationBuilder.Sql("DELETE FROM GorevliNotlari;");
            migrationBuilder.Sql("DELETE FROM GorevGecmisleri;");
            migrationBuilder.Sql("DELETE FROM Gorevlendirme;");
            migrationBuilder.Sql("DELETE FROM DernekUyeleri;");
            migrationBuilder.Sql("DELETE FROM KurumYonetimKuruluUyeleri;");
            migrationBuilder.Sql("DELETE FROM KurumDocuments;");
            migrationBuilder.Sql("DELETE FROM KurumKasaOdenekler;");
            migrationBuilder.Sql("DELETE FROM GorevliFaaliyetRaporlari;");
            migrationBuilder.Sql("DELETE FROM GorevliIzinler;");
            migrationBuilder.Sql("DELETE FROM Hissedarlar;");
            migrationBuilder.Sql("DELETE FROM Kurbanliklar;");
            migrationBuilder.Sql("DELETE FROM KurbanCampaignRecords;");
            migrationBuilder.Sql("DELETE FROM OverdueNotifications;");
            migrationBuilder.Sql("DELETE FROM KurumButcePeriods;");
            migrationBuilder.Sql("DELETE FROM KurumButceler;");
            migrationBuilder.Sql("DELETE FROM Gorevli;");
            migrationBuilder.Sql("DELETE FROM Kurum;");

            migrationBuilder.DeleteData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Gorevli",
                columns: new[] { "Id", "Ad", "Adres", "AnneAdi", "AskerlikDurumuId", "BabaAdi", "BasvuruTuruId", "CepTelefonu", "Cinsiyet", "DeletedAt", "Derece", "DiyanetGirisTarihi", "DogumTarihi", "DogumYeri", "Durum", "EgitimDurumuId", "EgitimKursBelgeleri", "Email", "EmeklilikTarihi", "EsDurumu", "EvTelefonu", "FotografYolu", "GorevUzatmaBitisTarihi", "GorevliDurumId", "HafizlikDurumuId", "IlkGoreveBaslamaTarihi", "IsDeleted", "Kademe", "KadroTuruId", "KanGrubuId", "LinkedUserId", "Memleketi", "MezuniyetBolum", "MezuniyetOkul", "PasaportNo", "PasaportTuru", "PassportExpirationDate", "ResidencePermitExpirationDate", "SicilNo", "Soyad", "SozlesmeTipId", "TCKimlikNo", "UnvanId", "VisaExpirationDate" },
                values: new object[,]
                {
                    { 1, "Ahmet", null, null, null, null, null, null, null, null, null, null, null, null, 0, null, null, "ahmet.yilmaz@example.com", null, null, null, null, null, null, null, null, false, null, null, null, null, null, null, null, null, null, null, null, null, "Yılmaz", null, null, null, null },
                    { 2, "Mehmet", null, null, null, null, null, null, null, null, null, null, null, null, 0, null, null, "mehmet.demir@example.com", null, null, null, null, null, null, null, null, false, null, null, null, null, null, null, null, null, null, null, null, null, "Demir", null, null, null, null },
                    { 3, "Ayşe", null, null, null, null, null, null, null, null, null, null, null, null, 0, null, null, "ayse.kaya@example.com", null, null, null, null, null, null, null, null, false, null, null, null, null, null, null, null, null, null, null, null, null, "Kaya", null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Kurum",
                columns: new[] { "Id", "Adres", "AktifMi", "BaskanMail", "BaskonsoloslukBolgesi", "Bolge", "CemaatCount", "CrmUyelikFormDurumu", "DeletedAt", "DernekBaskaniAd", "DernekBaskaniIletisim", "DinGorevlisiAd", "DinGorevlisiIletisim", "EkonomiNotu", "FrenchRegistrationName", "IbanNo", "IletisimNumarasi", "IsDeleted", "Isim", "KurulusKanunu", "Latitude", "Longitude", "Maili", "RnaNo", "Sehir", "SiretNo", "Tip", "UstKurumId" },
                values: new object[,]
                {
                    { 1, "12 Rue de la Musau", true, null, null, null, null, null, null, null, null, null, null, null, null, "", null, false, "Strasbourg Yunus Emre Camii", null, 48.566099999999999, 7.7786, null, "", "Strasbourg", "", 0, null },
                    { 2, "3 Rue des Écoles", true, null, null, null, null, null, null, null, null, null, null, null, null, "", null, false, "Bischheim Fatih Camii", null, 48.6143, 7.7491000000000003, null, "", "Bischheim", "", 0, null },
                    { 3, "5 Place Kléber", true, null, null, null, null, null, null, null, null, null, null, null, null, "", null, false, "Strasbourg Türk Kültür Derneği", null, 48.582999999999998, 7.7477999999999998, null, "", "Strasbourg", "", 1, null }
                });

            migrationBuilder.InsertData(
                table: "Gorevlendirme",
                columns: new[] { "Id", "BaslangicTarihi", "BitisTarihi", "DeletedAt", "GorevliId", "IsActive", "IsDeleted", "KurumId", "Tarih", "YerineGelecekGorevliId", "YerineGelisPlanlananBitisTarih", "YerineGelisPlanlananTarih" },
                values: new object[,]
                {
                    { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 1, false, false, 1, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 2, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 2, false, false, 2, new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null },
                    { 3, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 3, false, false, 3, new DateTime(2023, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null }
                });
        }
    }
}
