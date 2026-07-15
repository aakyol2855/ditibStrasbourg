using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurumIdToKurbanCampaignRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KurumId",
                table: "KurbanCampaignRecords",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KurbanCampaignRecords_KurumId",
                table: "KurbanCampaignRecords",
                column: "KurumId");

            migrationBuilder.AddForeignKey(
                name: "FK_KurbanCampaignRecords_Kurum_KurumId",
                table: "KurbanCampaignRecords",
                column: "KurumId",
                principalTable: "Kurum",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KurbanCampaignRecords_Kurum_KurumId",
                table: "KurbanCampaignRecords");

            migrationBuilder.DropIndex(
                name: "IX_KurbanCampaignRecords_KurumId",
                table: "KurbanCampaignRecords");

            migrationBuilder.DropColumn(
                name: "KurumId",
                table: "KurbanCampaignRecords");
        }
    }
}
