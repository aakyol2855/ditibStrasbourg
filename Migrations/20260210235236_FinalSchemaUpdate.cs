using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class FinalSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevlendirme_Gorevli_GorevliId",
                table: "Gorevlendirme");

            migrationBuilder.AddColumn<string>(
                name: "Adres",
                table: "Gorevli",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmeklilikTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YerineGelecekGorevliId",
                table: "Gorevlendirme",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "YerineGelisPlanlananBitisTarih",
                table: "Gorevlendirme",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "YerineGelisPlanlananTarih",
                table: "Gorevlendirme",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GorevlendirmeNotlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevlendirmeId = table.Column<int>(type: "int", nullable: false),
                    NotIcerik = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YazanKisiId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevlendirmeNotlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevlendirmeNotlari_AspNetUsers_YazanKisiId",
                        column: x => x.YazanKisiId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GorevlendirmeNotlari_Gorevlendirme_GorevlendirmeId",
                        column: x => x.GorevlendirmeId,
                        principalTable: "Gorevlendirme",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Tarih", "YerineGelecekGorevliId", "YerineGelisPlanlananBitisTarih", "YerineGelisPlanlananTarih" },
                values: new object[] { new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Tarih", "YerineGelecekGorevliId", "YerineGelisPlanlananBitisTarih", "YerineGelisPlanlananTarih" },
                values: new object[] { new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Tarih", "YerineGelecekGorevliId", "YerineGelisPlanlananBitisTarih", "YerineGelisPlanlananTarih" },
                values: new object[] { new DateTime(2023, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Adres", "EmeklilikTarihi" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Adres", "EmeklilikTarihi" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Adres", "EmeklilikTarihi" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevlendirme_YerineGelecekGorevliId",
                table: "Gorevlendirme",
                column: "YerineGelecekGorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevlendirmeNotlari_GorevlendirmeId",
                table: "GorevlendirmeNotlari",
                column: "GorevlendirmeId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevlendirmeNotlari_YazanKisiId",
                table: "GorevlendirmeNotlari",
                column: "YazanKisiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevlendirme_Gorevli_GorevliId",
                table: "Gorevlendirme",
                column: "GorevliId",
                principalTable: "Gorevli",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevlendirme_Gorevli_YerineGelecekGorevliId",
                table: "Gorevlendirme",
                column: "YerineGelecekGorevliId",
                principalTable: "Gorevli",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevlendirme_Gorevli_GorevliId",
                table: "Gorevlendirme");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevlendirme_Gorevli_YerineGelecekGorevliId",
                table: "Gorevlendirme");

            migrationBuilder.DropTable(
                name: "GorevlendirmeNotlari");

            migrationBuilder.DropIndex(
                name: "IX_Gorevlendirme_YerineGelecekGorevliId",
                table: "Gorevlendirme");

            migrationBuilder.DropColumn(
                name: "Adres",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "EmeklilikTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "YerineGelecekGorevliId",
                table: "Gorevlendirme");

            migrationBuilder.DropColumn(
                name: "YerineGelisPlanlananBitisTarih",
                table: "Gorevlendirme");

            migrationBuilder.DropColumn(
                name: "YerineGelisPlanlananTarih",
                table: "Gorevlendirme");

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 1,
                column: "Tarih",
                value: new DateTime(2023, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 2,
                column: "Tarih",
                value: new DateTime(2023, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 3,
                column: "Tarih",
                value: new DateTime(2023, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevlendirme_Gorevli_GorevliId",
                table: "Gorevlendirme",
                column: "GorevliId",
                principalTable: "Gorevli",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
