using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsResolved",
                table: "OverdueNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "KurumButcePeriodId",
                table: "OverdueNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "OverdueNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_KurumButcePeriodId",
                table: "OverdueNotifications",
                column: "KurumButcePeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_OverdueNotifications_KurumButcePeriods_KurumButcePeriodId",
                table: "OverdueNotifications",
                column: "KurumButcePeriodId",
                principalTable: "KurumButcePeriods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OverdueNotifications_KurumButcePeriods_KurumButcePeriodId",
                table: "OverdueNotifications");

            migrationBuilder.DropIndex(
                name: "IX_OverdueNotifications_KurumButcePeriodId",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "IsResolved",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "KurumButcePeriodId",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "OverdueNotifications");
        }
    }
}
