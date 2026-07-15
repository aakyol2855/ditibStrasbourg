using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurbanCampaignRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KurbanCampaignRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bolge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cami = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FysSorumlusu = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DigerAdet = table.Column<int>(type: "int", nullable: false),
                    DigerMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrAdet = table.Column<int>(type: "int", nullable: false),
                    TrMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Havale = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cek = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Nakit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stripe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cihaz = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ToplamOdenen = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TutanakNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Yil = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurbanCampaignRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KurbanCampaignRecords");
        }
    }
}
