using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddGorevlendirmeEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BitisTarihi",
                table: "Gorevlendirme",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 1,
                column: "BitisTarihi",
                value: null);

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 2,
                column: "BitisTarihi",
                value: null);

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 3,
                column: "BitisTarihi",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "Gorevlendirme");
        }
    }
}
