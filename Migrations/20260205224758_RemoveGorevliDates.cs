using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGorevliDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "Gorevli");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BitisTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BaslangicTarihi", "BitisTarihi" },
                values: new object[] { new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BaslangicTarihi", "BitisTarihi" },
                values: new object[] { new DateTime(2023, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BaslangicTarihi", "BitisTarihi" },
                values: new object[] { new DateTime(2023, 3, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), null });
        }
    }
}
