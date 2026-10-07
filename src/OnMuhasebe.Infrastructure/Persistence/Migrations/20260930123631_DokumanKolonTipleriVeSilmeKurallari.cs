using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DokumanKolonTipleriVeSilmeKurallari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturalari_Cariler_CariId",
                table: "AlisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturalari_Kullanicilar_KullaniciId",
                table: "AlisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Kullanicilar_KullaniciId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_Cariler_CariId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_Kullanicilar_KullaniciId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_SatisElemanlari_SatisElemaniId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_Kullanicilar_KullaniciId",
                table: "StokHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokId",
                table: "StokHareketler");

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "StokKartlari",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Yon",
                table: "StokHareketler",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "StokHareketler",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "HareketTipi",
                table: "StokHareketler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "StokHareketler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "SatisFaturaSatirlari",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "SatisFaturalari",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "SatisFaturalari",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "SatisFaturalari",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "SifreHash",
                table: "Kullanicilar",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Kullanicilar",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "CariHareketler",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "IslemTipi",
                table: "CariHareketler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "CariHareketler",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "AlisFaturaSatirlari",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "AlisFaturalari",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "AlisFaturalari",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "AlisFaturalari",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturalari_Cariler_CariId",
                table: "AlisFaturalari",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturalari_Kullanicilar_KullaniciId",
                table: "AlisFaturalari",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokId",
                table: "AlisFaturaSatirlari",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Kullanicilar_KullaniciId",
                table: "CariHareketler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_Cariler_CariId",
                table: "SatisFaturalari",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_Kullanicilar_KullaniciId",
                table: "SatisFaturalari",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_SatisElemanlari_SatisElemaniId",
                table: "SatisFaturalari",
                column: "SatisElemaniId",
                principalTable: "SatisElemanlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokId",
                table: "SatisFaturaSatirlari",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketler_Kullanicilar_KullaniciId",
                table: "StokHareketler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokId",
                table: "StokHareketler",
                column: "StokId",
                principalTable: "StokKartlari",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturalari_Cariler_CariId",
                table: "AlisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturalari_Kullanicilar_KullaniciId",
                table: "AlisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_AlisFaturaSatirlari_StokKartlari_StokId",
                table: "AlisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_CariHareketler_Kullanicilar_KullaniciId",
                table: "CariHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_Cariler_CariId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_Kullanicilar_KullaniciId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturalari_SatisElemanlari_SatisElemaniId",
                table: "SatisFaturalari");

            migrationBuilder.DropForeignKey(
                name: "FK_SatisFaturaSatirlari_StokKartlari_StokId",
                table: "SatisFaturaSatirlari");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_Kullanicilar_KullaniciId",
                table: "StokHareketler");

            migrationBuilder.DropForeignKey(
                name: "FK_StokHareketler_StokKartlari_StokId",
                table: "StokHareketler");

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "StokKartlari",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Yon",
                table: "StokHareketler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "StokHareketler",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "HareketTipi",
                table: "StokHareketler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "StokHareketler",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "SatisFaturaSatirlari",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "SatisFaturalari",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "SatisFaturalari",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "SatisFaturalari",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "SifreHash",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "Kullanicilar",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "CariHareketler",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "IslemTipi",
                table: "CariHareketler",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "BelgeNo",
                table: "CariHareketler",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "KdvOrani",
                table: "AlisFaturaSatirlari",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Tarih",
                table: "AlisFaturalari",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OlusturmaTarihi",
                table: "AlisFaturalari",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "FaturaNo",
                table: "AlisFaturalari",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturalari_Cariler_CariId",
                table: "AlisFaturalari",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AlisFaturalari_Kullanicilar_KullaniciId",
                table: "AlisFaturalari",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
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
                name: "FK_CariHareketler_Cariler_CariId",
                table: "CariHareketler",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CariHareketler_Kullanicilar_KullaniciId",
                table: "CariHareketler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_Cariler_CariId",
                table: "SatisFaturalari",
                column: "CariId",
                principalTable: "Cariler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_Kullanicilar_KullaniciId",
                table: "SatisFaturalari",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SatisFaturalari_SatisElemanlari_SatisElemaniId",
                table: "SatisFaturalari",
                column: "SatisElemaniId",
                principalTable: "SatisElemanlari",
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
                name: "FK_StokHareketler_Kullanicilar_KullaniciId",
                table: "StokHareketler",
                column: "KullaniciId",
                principalTable: "Kullanicilar",
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
    }
}
