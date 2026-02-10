using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class ExpandGorevliModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnneAdi",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AskerlikDurumuId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BabaAdi",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CepTelefonu",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Derece",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiyanetGirisTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DogumTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DogumYeri",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EgitimDurumuId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvTelefonu",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotografYolu",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HafizlikDurumuId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IlkGoreveBaslamaTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kademe",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KadroTuruId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KanGrubuId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MezuniyetBolum",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MezuniyetOkul",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TCKimlikNo",
                table: "Gorevli",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnvanId",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ref_AskerlikDurumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_AskerlikDurumlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_EgitimDurumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_EgitimDurumlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_HafizlikDurumlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_HafizlikDurumlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_KadroTurleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_KadroTurleri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_KanGruplari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_KanGruplari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ref_Unvans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_Unvans", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AnneAdi", "AskerlikDurumuId", "BabaAdi", "CepTelefonu", "Derece", "DiyanetGirisTarihi", "DogumTarihi", "DogumYeri", "EgitimDurumuId", "EvTelefonu", "FotografYolu", "HafizlikDurumuId", "IlkGoreveBaslamaTarihi", "Kademe", "KadroTuruId", "KanGrubuId", "MezuniyetBolum", "MezuniyetOkul", "TCKimlikNo", "UnvanId" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AnneAdi", "AskerlikDurumuId", "BabaAdi", "CepTelefonu", "Derece", "DiyanetGirisTarihi", "DogumTarihi", "DogumYeri", "EgitimDurumuId", "EvTelefonu", "FotografYolu", "HafizlikDurumuId", "IlkGoreveBaslamaTarihi", "Kademe", "KadroTuruId", "KanGrubuId", "MezuniyetBolum", "MezuniyetOkul", "TCKimlikNo", "UnvanId" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AnneAdi", "AskerlikDurumuId", "BabaAdi", "CepTelefonu", "Derece", "DiyanetGirisTarihi", "DogumTarihi", "DogumYeri", "EgitimDurumuId", "EvTelefonu", "FotografYolu", "HafizlikDurumuId", "IlkGoreveBaslamaTarihi", "Kademe", "KadroTuruId", "KanGrubuId", "MezuniyetBolum", "MezuniyetOkul", "TCKimlikNo", "UnvanId" },
                values: new object[] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_AskerlikDurumuId",
                table: "Gorevli",
                column: "AskerlikDurumuId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_EgitimDurumuId",
                table: "Gorevli",
                column: "EgitimDurumuId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_HafizlikDurumuId",
                table: "Gorevli",
                column: "HafizlikDurumuId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_KadroTuruId",
                table: "Gorevli",
                column: "KadroTuruId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_KanGrubuId",
                table: "Gorevli",
                column: "KanGrubuId");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_UnvanId",
                table: "Gorevli",
                column: "UnvanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_AskerlikDurumlari_AskerlikDurumuId",
                table: "Gorevli",
                column: "AskerlikDurumuId",
                principalTable: "Ref_AskerlikDurumlari",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_EgitimDurumlari_EgitimDurumuId",
                table: "Gorevli",
                column: "EgitimDurumuId",
                principalTable: "Ref_EgitimDurumlari",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_HafizlikDurumlari_HafizlikDurumuId",
                table: "Gorevli",
                column: "HafizlikDurumuId",
                principalTable: "Ref_HafizlikDurumlari",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_KadroTurleri_KadroTuruId",
                table: "Gorevli",
                column: "KadroTuruId",
                principalTable: "Ref_KadroTurleri",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_KanGruplari_KanGrubuId",
                table: "Gorevli",
                column: "KanGrubuId",
                principalTable: "Ref_KanGruplari",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gorevli_Ref_Unvans_UnvanId",
                table: "Gorevli",
                column: "UnvanId",
                principalTable: "Ref_Unvans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_AskerlikDurumlari_AskerlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_EgitimDurumlari_EgitimDurumuId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_HafizlikDurumlari_HafizlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_KadroTurleri_KadroTuruId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_KanGruplari_KanGrubuId",
                table: "Gorevli");

            migrationBuilder.DropForeignKey(
                name: "FK_Gorevli_Ref_Unvans_UnvanId",
                table: "Gorevli");

            migrationBuilder.DropTable(
                name: "Ref_AskerlikDurumlari");

            migrationBuilder.DropTable(
                name: "Ref_EgitimDurumlari");

            migrationBuilder.DropTable(
                name: "Ref_HafizlikDurumlari");

            migrationBuilder.DropTable(
                name: "Ref_KadroTurleri");

            migrationBuilder.DropTable(
                name: "Ref_KanGruplari");

            migrationBuilder.DropTable(
                name: "Ref_Unvans");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_AskerlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_EgitimDurumuId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_HafizlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_KadroTuruId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_KanGrubuId",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_UnvanId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "AnneAdi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "AskerlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "BabaAdi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "CepTelefonu",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "Derece",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "DiyanetGirisTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "DogumTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "DogumYeri",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "EgitimDurumuId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "EvTelefonu",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "FotografYolu",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "HafizlikDurumuId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "IlkGoreveBaslamaTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "Kademe",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "KadroTuruId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "KanGrubuId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "MezuniyetBolum",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "MezuniyetOkul",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "TCKimlikNo",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "UnvanId",
                table: "Gorevli");
        }
    }
}
