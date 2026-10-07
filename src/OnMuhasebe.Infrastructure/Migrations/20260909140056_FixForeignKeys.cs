using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturasiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokKartiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturasiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokKartiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokKartiId",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketler_StokKartiId",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturaSatirlari_SatisFaturasiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturaSatirlari_StokKartiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturaSatirlari_AlisFaturasiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturaSatirlari_StokKartiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropColumn(
                name: "StokKartiId",
                table: "StokHareketler");

            migrationBuilder.DropColumn(
                name: "SatisFaturasiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropColumn(
                name: "StokKartiId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropColumn(
                name: "AlisFaturasiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropColumn(
                name: "StokKartiId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_StokId",
                table: "StokHareketler",
                column: "StokId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_SatisFaturaId",
                table: "SatisFaturaSatirlari",
                column: "SatisFaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_StokId",
                table: "SatisFaturaSatirlari",
                column: "StokId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_AlisFaturaId",
                table: "AlisFaturaSatirlari",
                column: "AlisFaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_StokId",
                table: "AlisFaturaSatirlari",
                column: "StokId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturaId",
                table: "AlisFaturaSatirlari",
                column: "AlisFaturaId",
                principalTable: "AlisFaturalari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokId",
                table: "AlisFaturaSatirlari",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturaId",
                table: "SatisFaturaSatirlari",
                column: "SatisFaturaId",
                principalTable: "SatisFaturalari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokId",
                table: "SatisFaturaSatirlari",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokId",
                table: "StokHareketler",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturaId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturaId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokId",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_StokHareketler_StokId",
                table: "StokHareketler");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturaSatirlari_SatisFaturaId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_SatisFaturaSatirlari_StokId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturaSatirlari_AlisFaturaId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropIndex(
                name: "IX_AlisFaturaSatirlari_StokId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.AddColumn<int>(
                name: "StokKartiId",
                table: "StokHareketler",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SatisFaturasiId",
                table: "SatisFaturaSatirlari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StokKartiId",
                table: "SatisFaturaSatirlari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AlisFaturasiId",
                table: "AlisFaturaSatirlari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StokKartiId",
                table: "AlisFaturaSatirlari",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_StokKartiId",
                table: "StokHareketler",
                column: "StokKartiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_SatisFaturasiId",
                table: "SatisFaturaSatirlari",
                column: "SatisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_StokKartiId",
                table: "SatisFaturaSatirlari",
                column: "StokKartiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_AlisFaturasiId",
                table: "AlisFaturaSatirlari",
                column: "AlisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_StokKartiId",
                table: "AlisFaturaSatirlari",
                column: "StokKartiId");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturasiId",
                table: "AlisFaturaSatirlari",
                column: "AlisFaturasiId",
                principalTable: "AlisFaturalari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokKartiId",
                table: "AlisFaturaSatirlari",
                column: "StokKartiId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturasiId",
                table: "SatisFaturaSatirlari",
                column: "SatisFaturasiId",
                principalTable: "SatisFaturalari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokKartiId",
                table: "SatisFaturaSatirlari",
                column: "StokKartiId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokKartiId",
                table: "StokHareketler",
                column: "StokKartiId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
