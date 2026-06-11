using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewKurumFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BaskanMail",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IletisimNumarasi",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Maili",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BaskanMail", "IletisimNumarasi", "Maili" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BaskanMail", "IletisimNumarasi", "Maili" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BaskanMail", "IletisimNumarasi", "Maili" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaskanMail",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "IletisimNumarasi",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Maili",
                table: "Kurum");
        }
    }
}
