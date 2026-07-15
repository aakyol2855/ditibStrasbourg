using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurbanApprovalAndAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogType",
                table: "SystemAuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "SystemAuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SystemAuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "KurbanCampaignRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogType",
                table: "SystemAuditLogs");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "SystemAuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SystemAuditLogs");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "KurbanCampaignRecords");
        }
    }
}
