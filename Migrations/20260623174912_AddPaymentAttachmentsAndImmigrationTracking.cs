using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAttachmentsAndImmigrationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "KurumButcePeriods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaidFromSource",
                table: "KurumButcePeriods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpirationDate",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResidencePermitExpirationDate",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisaExpirationDate",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PassportExpirationDate", "ResidencePermitExpirationDate", "VisaExpirationDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "PassportExpirationDate", "ResidencePermitExpirationDate", "VisaExpirationDate" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "PassportExpirationDate", "ResidencePermitExpirationDate", "VisaExpirationDate" },
                values: new object[] { null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "KurumButcePeriods");

            migrationBuilder.DropColumn(
                name: "PaidFromSource",
                table: "KurumButcePeriods");

            migrationBuilder.DropColumn(
                name: "PassportExpirationDate",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "ResidencePermitExpirationDate",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "VisaExpirationDate",
                table: "Gorevli");
        }
    }
}
