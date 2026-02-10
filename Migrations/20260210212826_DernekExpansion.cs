using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class DernekExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaskonsoloslukBolgesi",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bolge",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CrmUyelikFormDurumu",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DernekBaskaniAd",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DernekBaskaniIletisim",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DinGorevlisiAd",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DinGorevlisiIletisim",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KurulusKanunu",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DernekUyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Iletisim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AileUyeSayisi = table.Column<int>(type: "int", nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KurumId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DernekUyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DernekUyeleri_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BaskonsoloslukBolgesi", "Bolge", "CrmUyelikFormDurumu", "DernekBaskaniAd", "DernekBaskaniIletisim", "DinGorevlisiAd", "DinGorevlisiIletisim", "KurulusKanunu" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BaskonsoloslukBolgesi", "Bolge", "CrmUyelikFormDurumu", "DernekBaskaniAd", "DernekBaskaniIletisim", "DinGorevlisiAd", "DinGorevlisiIletisim", "KurulusKanunu" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BaskonsoloslukBolgesi", "Bolge", "CrmUyelikFormDurumu", "DernekBaskaniAd", "DernekBaskaniIletisim", "DinGorevlisiAd", "DinGorevlisiIletisim", "KurulusKanunu" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_DernekUyeleri_KurumId",
                table: "DernekUyeleri",
                column: "KurumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DernekUyeleri");

            migrationBuilder.DropColumn(
                name: "BaskonsoloslukBolgesi",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Bolge",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "CrmUyelikFormDurumu",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "DernekBaskaniAd",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "DernekBaskaniIletisim",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "DinGorevlisiAd",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "DinGorevlisiIletisim",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "KurulusKanunu",
                table: "Kurum");
        }
    }
}
