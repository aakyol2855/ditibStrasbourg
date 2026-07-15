using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaExtensions_DernekGorevli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCami",
                table: "Kurum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLojman",
                table: "Kurum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMustemilat",
                table: "Kurum",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LojmanKapasite",
                table: "Kurum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MustemilatKapasite",
                table: "Kurum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Agno",
                table: "Gorevli",
                type: "decimal(3,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diller",
                table: "Gorevli",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Universite",
                table: "Gorevli",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DernekGorselleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DernekId = table.Column<int>(type: "int", nullable: false),
                    GorselYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GorselTipi = table.Column<int>(type: "int", nullable: false),
                    YuklenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YukleyenKullanici = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DernekGorselleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DernekGorselleri_Kurum_DernekId",
                        column: x => x.DernekId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DernekNotlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DernekId = table.Column<int>(type: "int", nullable: false),
                    NotIcerigi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    KayitTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EkleyenKullanici = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DernekNotlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DernekNotlari_Kurum_DernekId",
                        column: x => x.DernekId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GorevliBelgeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevliId = table.Column<int>(type: "int", nullable: false),
                    BelgeTipi = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SeriNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GecerlilikTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DosyaYolu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    YukleyenKullanici = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YuklenmeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevliBelgeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevliBelgeleri_Gorevli_GorevliId",
                        column: x => x.GorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DernekGorselleri_DernekId",
                table: "DernekGorselleri",
                column: "DernekId");

            migrationBuilder.CreateIndex(
                name: "IX_DernekNotlari_DernekId",
                table: "DernekNotlari",
                column: "DernekId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliBelge_GorevliBelgeTipi",
                table: "GorevliBelgeleri",
                columns: new[] { "GorevliId", "BelgeTipi" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DernekGorselleri");

            migrationBuilder.DropTable(
                name: "DernekNotlari");

            migrationBuilder.DropTable(
                name: "GorevliBelgeleri");

            migrationBuilder.DropColumn(
                name: "HasCami",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "HasLojman",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "HasMustemilat",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "LojmanKapasite",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "MustemilatKapasite",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Agno",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "Diller",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "Universite",
                table: "Gorevli");
        }
    }
}
