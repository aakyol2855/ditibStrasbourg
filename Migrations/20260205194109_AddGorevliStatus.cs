using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddGorevliStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "Gorevli",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                column: "Durum",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                column: "Durum",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                column: "Durum",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "Gorevli");
        }
    }
}
