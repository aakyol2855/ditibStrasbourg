using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class SystemRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AktifMi",
                table: "Kurum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sehir",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UstKurumId",
                table: "Kurum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cinsiyet",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GorevliDurumId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SozlesmeTipId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GorevGecmisleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevliId = table.Column<int>(type: "int", nullable: false),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YerineGelenGorevliId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevGecmisleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevGecmisleri_Gorevli_GorevliId",
                        column: x => x.GorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GorevGecmisleri_Gorevli_YerineGelenGorevliId",
                        column: x => x.YerineGelenGorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GorevGecmisleri_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GorevliNotlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevliId = table.Column<int>(type: "int", nullable: false),
                    NotIcerik = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YazanKisiId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevliNotlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevliNotlari_AspNetUsers_YazanKisiId",
                        column: x => x.YazanKisiId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GorevliNotlari_Gorevli_GorevliId",
                        column: x => x.GorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ref_GorevliDurums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Renk = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_GorevliDurums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_KurumTurus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_KurumTurus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_SozlesmeTips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_SozlesmeTips", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Cinsiyet", "GorevliDurumId", "SozlesmeTipId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Cinsiyet", "GorevliDurumId", "SozlesmeTipId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Cinsiyet", "GorevliDurumId", "SozlesmeTipId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AktifMi", "Sehir", "UstKurumId" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AktifMi", "Sehir", "UstKurumId" },
                values: new object[] { true, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AktifMi", "Sehir", "UstKurumId" },
                values: new object[] { true, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Kurum_UstKurumId",
                table: "Kurum",
                column: "UstKurumId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_GorevliDurumId",
                table: "Gorevli",
                column: "GorevliDurumId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_SozlesmeTipId",
                table: "Gorevli",
                column: "SozlesmeTipId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevGecmisleri_GorevliId",
                table: "GorevGecmisleri",
                column: "GorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevGecmisleri_KurumId",
                table: "GorevGecmisleri",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevGecmisleri_YerineGelenGorevliId",
                table: "GorevGecmisleri",
                column: "YerineGelenGorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliNotlari_GorevliId",
                table: "GorevliNotlari",
                column: "GorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliNotlari_YazanKisiId",
                table: "GorevliNotlari",
                column: "YazanKisiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_GorevliDurums_GorevliDurumId",
                table: "Gorevli",
                column: "GorevliDurumId",
                principalTable: "Ref_GorevliDurums",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_SozlesmeTips_SozlesmeTipId",
                table: "Gorevli",
                column: "SozlesmeTipId",
                principalTable: "Ref_SozlesmeTips",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Kurum_Ref_KurumTurus_UstKurumId",
                table: "Kurum",
                column: "UstKurumId",
                principalTable: "Ref_KurumTurus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_GorevliDurums_GorevliDurumId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_SozlesmeTips_SozlesmeTipId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Kurum_Ref_KurumTurus_UstKurumId",
                table: "Kurum");

            migrationBuilder.DropTable(
                name: "GorevGecmisleri");

            migrationBuilder.DropTable(
                name: "GorevliNotlari");

            migrationBuilder.DropTable(
                name: "Ref_GorevliDurums");

            migrationBuilder.DropTable(
                name: "Ref_KurumTurus");

            migrationBuilder.DropTable(
                name: "Ref_SozlesmeTips");

            migrationBuilder.DropIndex(
                name: "IX_Kurum_UstKurumId",
                table: "Kurum");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_GorevliDurumId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_SozlesmeTipId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "AktifMi",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Sehir",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "UstKurumId",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Cinsiyet",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "GorevliDurumId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "SozlesmeTipId",
                table: "Gorevli");
        }
    }
}
