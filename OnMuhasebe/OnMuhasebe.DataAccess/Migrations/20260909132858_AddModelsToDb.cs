using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnMuhasebe.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddModelsToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cariler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CariKodu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unvan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CariTipi = table.Column<byte>(type: "tinyint", nullable: false),
                    VergiDairesi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VergiNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cariler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SifreHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parametreler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParametreKodu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParametreDegeri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametreler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SatisElemanlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisElemanlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StokKartlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokKodu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StokAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlisFiyati = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatisFiyati = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KritikStok = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokKartlari", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlisFaturalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CariId = table.Column<int>(type: "int", nullable: false),
                    AraToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenelToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisFaturalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFaturalari_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisFaturalari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CariHareketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CariId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IslemTipi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Borc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Alacak = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OdemeTuru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BelgeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CariHareketler_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CariHareketler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturalari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CariId = table.Column<int>(type: "int", nullable: false),
                    SatisElemaniId = table.Column<int>(type: "int", nullable: false),
                    AraToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GenelToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturalari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_Cariler_CariId",
                        column: x => x.CariId,
                        principalTable: "Cariler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturalari_SatisElemanlari_SatisElemaniId",
                        column: x => x.SatisElemaniId,
                        principalTable: "SatisElemanlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StokHareketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HareketTipi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Yon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BelgeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    StokKartiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareketler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StokHareketler_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StokHareketler_StokKartlari_StokKartiId",
                        column: x => x.StokKartiId,
                        principalTable: "StokKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlisFaturaSatirlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlisFaturaId = table.Column<int>(type: "int", nullable: false),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AlisFaturasiId = table.Column<int>(type: "int", nullable: false),
                    StokKartiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlisFaturaSatirlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlisFaturaSatirlari_AlisFaturalari_AlisFaturasiId",
                        column: x => x.AlisFaturasiId,
                        principalTable: "AlisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlisFaturaSatirlari_StokKartlari_StokKartiId",
                        column: x => x.StokKartiId,
                        principalTable: "StokKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SatisFaturaSatirlari",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SatisFaturaId = table.Column<int>(type: "int", nullable: false),
                    StokId = table.Column<int>(type: "int", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatisFaturasiId = table.Column<int>(type: "int", nullable: false),
                    StokKartiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SatisFaturaSatirlari", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SatisFaturaSatirlari_SatisFaturalari_SatisFaturasiId",
                        column: x => x.SatisFaturasiId,
                        principalTable: "SatisFaturalari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SatisFaturaSatirlari_StokKartlari_StokKartiId",
                        column: x => x.StokKartiId,
                        principalTable: "StokKartlari",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_CariId",
                table: "AlisFaturalari",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturalari_KullaniciId",
                table: "AlisFaturalari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_AlisFaturasiId",
                table: "AlisFaturaSatirlari",
                column: "AlisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_AlisFaturaSatirlari_StokKartiId",
                table: "AlisFaturaSatirlari",
                column: "StokKartiId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariId",
                table: "CariHareketler",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_KullaniciId",
                table: "CariHareketler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_CariId",
                table: "SatisFaturalari",
                column: "CariId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_KullaniciId",
                table: "SatisFaturalari",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturalari_SatisElemaniId",
                table: "SatisFaturalari",
                column: "SatisElemaniId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_SatisFaturasiId",
                table: "SatisFaturaSatirlari",
                column: "SatisFaturasiId");

            migrationBuilder.CreateIndex(
                name: "IX_SatisFaturaSatirlari_StokKartiId",
                table: "SatisFaturaSatirlari",
                column: "StokKartiId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_KullaniciId",
                table: "StokHareketler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketler_StokKartiId",
                table: "StokHareketler",
                column: "StokKartiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlisFaturaSatirlari");

            migrationBuilder.DropTable(
                name: "CariHareketler");

            migrationBuilder.DropTable(
                name: "Parametreler");

            migrationBuilder.DropTable(
                name: "SatisFaturaSatirlari");

            migrationBuilder.DropTable(
                name: "StokHareketler");

            migrationBuilder.DropTable(
                name: "AlisFaturalari");

            migrationBuilder.DropTable(
                name: "SatisFaturalari");

            migrationBuilder.DropTable(
                name: "StokKartlari");

            migrationBuilder.DropTable(
                name: "Cariler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "SatisElemanlari");
        }
    }
}
