using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class FixSystemicAccrualAndIdentityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrenchNationalId",
                table: "Gorevli",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrenchNationalId",
                table: "Gorevli");
        }
    }
}
