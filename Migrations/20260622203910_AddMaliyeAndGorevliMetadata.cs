using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DitibStasbourg.Migrations
{
    /// <inheritdoc />
    public partial class AddMaliyeAndGorevliMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EkonomiNotu",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IbanNo",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RnaNo",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SiretNo",
                table: "Kurum",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EgitimKursBelgeleri",
                table: "Gorevli",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EsDurumu",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GorevUzatmaBitisTarihi",
                table: "Gorevli",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedUserId",
                table: "Gorevli",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Memleketi",
                table: "Gorevli",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasaportNo",
                table: "Gorevli",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasaportTuru",
                table: "Gorevli",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SicilNo",
                table: "Gorevli",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BaslangicTarihi",
                table: "Gorevlendirme",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Gorevlendirme",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GorevliFaaliyetRaporlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevliId = table.Column<int>(type: "int", nullable: false),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    RaporTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    KursTuru = table.Column<int>(type: "int", nullable: false),
                    KatilimciSayisi = table.Column<int>(type: "int", nullable: false),
                    FaaliyetDetayi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevliFaaliyetRaporlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevliFaaliyetRaporlari_Gorevli_GorevliId",
                        column: x => x.GorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GorevliFaaliyetRaporlari_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GorevliIzinler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GorevliId = table.Column<int>(type: "int", nullable: false),
                    IzinTuru = table.Column<int>(type: "int", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IzinAdresi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IzinTelefonu = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    OnayDurumu = table.Column<int>(type: "int", nullable: false),
                    OnaylayanKisi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OnayTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GorevliIzinler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GorevliIzinler_Gorevli_GorevliId",
                        column: x => x.GorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KurumButceler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DitibContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DernekContribution = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumButceler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumButceler_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KurumHavuzTakibiSet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    PersonnelGender = table.Column<int>(type: "int", nullable: false),
                    VariableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSettled = table.Column<bool>(type: "bit", nullable: false),
                    InternalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumHavuzTakibiSet", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumHavuzTakibiSet_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KurumKasaOdenekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumId = table.Column<int>(type: "int", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllocationType = table.Column<int>(type: "int", nullable: false),
                    TargetGorevliId = table.Column<int>(type: "int", nullable: true),
                    TutanakNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IslemYapan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumKasaOdenekler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumKasaOdenekler_Gorevli_TargetGorevliId",
                        column: x => x.TargetGorevliId,
                        principalTable: "Gorevli",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KurumKasaOdenekler_Kurum_KurumId",
                        column: x => x.KurumId,
                        principalTable: "Kurum",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KurumButcePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KurumButceId = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionTutanakNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurumButcePeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KurumButcePeriods_KurumButceler_KurumButceId",
                        column: x => x.KurumButceId,
                        principalTable: "KurumButceler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BaslangicTarihi", "IsActive" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "BaslangicTarihi", "IsActive" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false });

            migrationBuilder.UpdateData(
                table: "Gorevlendirme",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "BaslangicTarihi", "IsActive" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EgitimKursBelgeleri", "EsDurumu", "GorevUzatmaBitisTarihi", "LinkedUserId", "Memleketi", "PasaportNo", "PasaportTuru", "SicilNo" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EgitimKursBelgeleri", "EsDurumu", "GorevUzatmaBitisTarihi", "LinkedUserId", "Memleketi", "PasaportNo", "PasaportTuru", "SicilNo" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Gorevli",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EgitimKursBelgeleri", "EsDurumu", "GorevUzatmaBitisTarihi", "LinkedUserId", "Memleketi", "PasaportNo", "PasaportTuru", "SicilNo" },
                values: new object[] { null, null, null, null, null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EkonomiNotu", "IbanNo", "RnaNo", "SiretNo" },
                values: new object[] { null, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EkonomiNotu", "IbanNo", "RnaNo", "SiretNo" },
                values: new object[] { null, "", "", "" });

            migrationBuilder.UpdateData(
                table: "Kurum",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "EkonomiNotu", "IbanNo", "RnaNo", "SiretNo" },
                values: new object[] { null, "", "", "" });

            migrationBuilder.CreateIndex(
                name: "IX_Gorevli_SicilNo",
                table: "Gorevli",
                column: "SicilNo",
                unique: true,
                filter: "[SicilNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliFaaliyetRaporlari_GorevliId",
                table: "GorevliFaaliyetRaporlari",
                column: "GorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliFaaliyetRaporlari_KurumId",
                table: "GorevliFaaliyetRaporlari",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_GorevliIzinler_GorevliId",
                table: "GorevliIzinler",
                column: "GorevliId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumButceler_KurumId",
                table: "KurumButceler",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumButcePeriods_KurumButceId",
                table: "KurumButcePeriods",
                column: "KurumButceId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumHavuzTakibiSet_KurumId",
                table: "KurumHavuzTakibiSet",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumKasaOdenekler_KurumId",
                table: "KurumKasaOdenekler",
                column: "KurumId");

            migrationBuilder.CreateIndex(
                name: "IX_KurumKasaOdenekler_TargetGorevliId",
                table: "KurumKasaOdenekler",
                column: "TargetGorevliId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GorevliFaaliyetRaporlari");

            migrationBuilder.DropTable(
                name: "GorevliIzinler");

            migrationBuilder.DropTable(
                name: "KurumButcePeriods");

            migrationBuilder.DropTable(
                name: "KurumHavuzTakibiSet");

            migrationBuilder.DropTable(
                name: "KurumKasaOdenekler");

            migrationBuilder.DropTable(
                name: "KurumButceler");

            migrationBuilder.DropIndex(
                name: "IX_Gorevli_SicilNo",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "EkonomiNotu",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "IbanNo",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "RnaNo",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "SiretNo",
                table: "Kurum");

            migrationBuilder.DropColumn(
                name: "EgitimKursBelgeleri",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "EsDurumu",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "GorevUzatmaBitisTarihi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "LinkedUserId",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "Memleketi",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "PasaportNo",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "PasaportTuru",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "SicilNo",
                table: "Gorevli");

            migrationBuilder.DropColumn(
                name: "BaslangicTarihi",
                table: "Gorevlendirme");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Gorevlendirme");
        }
    }
}
