using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKurumSeedCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { 48.566099999999999, 7.7786, "Strasbourg" });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { 48.6143, 7.7491000000000003, "Bischheim" });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { 48.582999999999998, 7.7477999999999998, "Strasbourg" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Latitude", "Longitude", "Sehir" },
                values: new object[] { null, null, null });
        }
    }
}
