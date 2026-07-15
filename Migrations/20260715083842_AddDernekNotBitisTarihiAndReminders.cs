using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddDernekNotBitisTarihiAndReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RelatedDernekNotId",
                table: "OverdueNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedGorevliBelgeId",
                table: "OverdueNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BitisTarihi",
                table: "DernekNotlari",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_RelatedDernekNotId",
                table: "OverdueNotifications",
                column: "RelatedDernekNotId");

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_RelatedGorevliBelgeId",
                table: "OverdueNotifications",
                column: "RelatedGorevliBelgeId");

            migrationBuilder.AddForeignKey(
                name: "FK_OverdueNotifications_DernekNotlari_RelatedDernekNotId",
                table: "OverdueNotifications",
                column: "RelatedDernekNotId",
                principalTable: "DernekNotlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OverdueNotifications_GorevliBelgeleri_RelatedGorevliBelgeId",
                table: "OverdueNotifications",
                column: "RelatedGorevliBelgeId",
                principalTable: "GorevliBelgeleri",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OverdueNotifications_DernekNotlari_RelatedDernekNotId",
                table: "OverdueNotifications");

            migrationBuilder.DropForeignKey(
                name: "FK_OverdueNotifications_GorevliBelgeleri_RelatedGorevliBelgeId",
                table: "OverdueNotifications");

            migrationBuilder.DropIndex(
                name: "IX_OverdueNotifications_RelatedDernekNotId",
                table: "OverdueNotifications");

            migrationBuilder.DropIndex(
                name: "IX_OverdueNotifications_RelatedGorevliBelgeId",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "RelatedDernekNotId",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "RelatedGorevliBelgeId",
                table: "OverdueNotifications");

            migrationBuilder.DropColumn(
                name: "BitisTarihi",
                table: "DernekNotlari");
        }
    }
}
