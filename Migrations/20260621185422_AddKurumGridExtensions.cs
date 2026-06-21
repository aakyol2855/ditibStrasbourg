using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddKurumGridExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CemaatCount",
                table: "Kurum",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrenchRegistrationName",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ref_YonetimRols",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ref_YonetimRols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KurumYonetimKuruluUyeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YonetimRolId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumYonetimKuruluUyeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumYonetimKuruluUyeleri_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KurumYonetimKuruluUyeleri_Ref_YonetimRols_YonetimRolId",
                        column: x => x.YonetimRolId,
                        principalTable: "Ref_YonetimRols",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CemaatCount", "FrenchRegistrationName" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CemaatCount", "FrenchRegistrationName" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CemaatCount", "FrenchRegistrationName" },
                values: new object[] { null, null });

            migrationBuilder.InsertData(
                table: "Ref_YonetimRols",
                columns: new[] { "Id", "Ad", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Başkan", false },
                    { 2, "Sekreter", false },
                    { 3, "Muhasip", false },
                    { 4, "Üye", false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_KurumYonetimKuruluUyeleri_KurumId",
                table: "KurumYonetimKuruluUyeleri",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumYonetimKuruluUyeleri_YonetimRolId",
                table: "KurumYonetimKuruluUyeleri",
                column: "YonetimRolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KurumYonetimKuruluUyeleri");

            migrationBuilder.DropTable(
                name: "Ref_YonetimRols");

            migrationBuilder.DropColumn(
                name: "CemaatCount",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "FrenchRegistrationName",
                table: "Kurum");
        }
    }
}
