using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseGapFillSubsystems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BudgetRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumButceId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AdditionalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RevisionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ApprovedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetRevisions_KurumButceler_KurumButceId",
                        column: x => x.KurumButceId,
                        principalTable: "KurumButceler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KurumDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeKb = table.Column<long>(type: "bigint", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumDocuments_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OverdueNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RelatedKurumId = table.Column<int>(type: "int", nullable: true),
                    RelatedGorevliId = table.Column<int>(type: "int", nullable: true),
                    RelatedBudgetPeriodId = table.Column<int>(type: "int", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsEmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OverdueNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OverdueNotifications_Gorevli_RelatedGorevliId",
                        column: x => x.RelatedGorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdueNotifications_KurumButcePeriods_RelatedBudgetPeriodId",
                        column: x => x.RelatedBudgetPeriodId,
                        principalTable: "KurumButcePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OverdueNotifications_Kurum_RelatedKurumId",
                        column: x => x.RelatedKurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRevisions_KurumButceId",
                table: "BudgetRevisions",
                column: "KurumButceId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumDocument_KurumCategory",
                table: "KurumDocuments",
                columns: new[] { "KurumId", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_ReadSeverity",
                table: "OverdueNotifications",
                columns: new[] { "IsRead", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_RelatedBudgetPeriodId",
                table: "OverdueNotifications",
                column: "RelatedBudgetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_RelatedGorevliId",
                table: "OverdueNotifications",
                column: "RelatedGorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_OverdueNotifications_RelatedKurumId",
                table: "OverdueNotifications",
                column: "RelatedKurumId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BudgetRevisions");

            migrationBuilder.DropTable(
                name: "KurumDocuments");

            migrationBuilder.DropTable(
                name: "OverdueNotifications");
        }
    }
}
