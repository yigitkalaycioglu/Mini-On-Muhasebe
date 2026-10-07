using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "StokHareketler",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "SatisFaturalari",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "CariHareketler",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "AlisFaturalari",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_StokKartlari_StokKodu",
                table: "StokKartlari",
                column: "StokKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_BelgeNo",
                table: "StokHareketler",
                column: "BelgeNo");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_FaturaNo",
                table: "SatisFaturalari",
                column: "FaturaNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parametreler_ParametreKodu",
                table: "Parametreler",
                column: "ParametreKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar",
                column: "KullaniciAdi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cariler_CariKodu",
                table: "Cariler",
                column: "CariKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_BelgeNo",
                table: "CariHareketler",
                column: "BelgeNo");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_FaturaNo",
                table: "AlisFaturalari",
                column: "FaturaNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StokKartlari_StokKodu",
                table: "StokKartlari");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketler_BelgeNo",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturalari_FaturaNo",
                table: "SatisFaturalari");

            migrationBuilder.DropIndex(
                name: "IX_Parametreler_ParametreKodu",
                table: "Parametreler");

            migrationBuilder.DropIndex(
                name: "IX_Kullanicilar_KullaniciAdi",
                table: "Kullanicilar");

            migrationBuilder.DropIndex(
                name: "IX_Cariler_CariKodu",
                table: "Cariler");

            migrationBuilder.DropIndex(
                name: "IX_CariHareketler_BelgeNo",
                table: "CariHareketler");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturalari_FaturaNo",
                table: "AlisFaturalari");

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "StokHareketler",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "SatisFaturalari",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "CariHareketler",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "AlisFaturalari",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
