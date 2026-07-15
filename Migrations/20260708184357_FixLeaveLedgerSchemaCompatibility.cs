using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveLedgerSchemaCompatibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Durum",
                table: "GorevliIzinler",
                newName: "OnayDurumu");

            migrationBuilder.AddColumn<DateTime>(
                name: "FransaGirisTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FransaGirisTarihi",
                table: "Gorevli");

            migrationBuilder.RenameColumn(
                name: "OnayDurumu",
                table: "GorevliIzinler",
                newName: "Durum");
        }
    }
}
