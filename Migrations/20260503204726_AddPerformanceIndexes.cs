using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Sehir",
                table: "Kurum",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bolge",
                table: "Kurum",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Soyad",
                table: "Gorevli",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Gorevli",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Gorevli",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Kurum_Geo",
                table: "Kurum",
                columns: new[] { "Bolge", "Sehir" });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_Search",
                table: "Gorevli",
                columns: new[] { "Ad", "Soyad", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_TCKimlikNo",
                table: "Gorevli",
                column: "TCKimlikNo",
                unique: true,
                filter: "[TCKimlikNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Gorevlendirme_Filters",
                table: "Gorevlendirme",
                columns: new[] { "Tarih", "BitisTarihi", "KurumId", "GorevliId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Kurum_Geo",
                table: "Kurum");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_Search",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_TCKimlikNo",
                table: "Gorevli");

            migrationBuilder.DropIndex(
                name: "IX_Gorevlendirme_Filters",
                table: "Gorevlendirme");

            migrationBuilder.AlterColumn<string>(
                name: "Sehir",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bolge",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Soyad",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Ad",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
