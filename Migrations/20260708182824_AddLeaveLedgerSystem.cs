using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveLedgerSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OnayDurumu",
                table: "GorevliIzinler",
                newName: "ToplamGun");

            migrationBuilder.AddColumn<int>(
                name: "Durum",
                table: "GorevliIzinler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EvrakNo",
                table: "GorevliIzinler",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualEntryByAdmin",
                table: "GorevliIzinler",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TalepTarihi",
                table: "GorevliIzinler",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Durum",
                table: "GorevliIzinler");

            migrationBuilder.DropColumn(
                name: "EvrakNo",
                table: "GorevliIzinler");

            migrationBuilder.DropColumn(
                name: "IsManualEntryByAdmin",
                table: "GorevliIzinler");

            migrationBuilder.DropColumn(
                name: "TalepTarihi",
                table: "GorevliIzinler");

            migrationBuilder.RenameColumn(
                name: "ToplamGun",
                table: "GorevliIzinler",
                newName: "OnayDurumu");
        }
    }
}
