using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurumCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Kurum",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Kurum",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Latitude", "Longitude" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Kurum");
        }
    }
}
