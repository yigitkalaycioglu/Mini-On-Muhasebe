using System.Net;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.Application.Services;

namespace OnMuhasebe.IntegrationTests
{
    /// <summary>Ekranlar, yetkilendirme, güvenlik başlıkları ve form bağlama uçtan uca (HTTP üzerinden).</summary>
    [Collection(VeritabaniKoleksiyonu.Ad)]
    public class EkranTestleri(UygulamaFabrikasi fabrika)
    {
        public static TheoryData<string> YoneticiEkranlari =>
        [
            "/", "/Cari", "/Cari/Create", "/Cari/Edit/1", "/Cari/Ekstre/1", "/StokKarti", "/StokKarti/Create", "/StokKarti/Edit/1",
            "/SatisElemani", "/SatisElemani/Create", "/SatisElemani/Edit/1", "/SatisFaturasi", "/SatisFaturasi/Create",
            "/SatisFaturasi/Detay/1", "/AlisFaturasi", "/AlisFaturasi/Create", "/AlisFaturasi/Detay/1", "/StokHareket",
            "/StokHareket/SayimFisi", "/TahsilatOdeme", "/TahsilatOdeme/Create", "/Rapor", "/Rapor/CariBakiye",
            "/Rapor/StokDurum", "/Rapor/KritikStok", "/Rapor/SatisElemaniSatis", "/Kullanici", "/Kullanici/Create",
            "/Kullanici/Edit/1", "/Parametre"
        ];

        [Theory]
        [InlineData("/")]
        [InlineData("/Cari")]
        [InlineData("/Rapor/StokDurum")]
        public async Task GirisYapilmadan_KorunanSayfa_GirisEkraninaYonlendirir(string yol)
        {
            UygulamaFabrikasi.VeritabaniGerekli();

            var yanit = await new TarayiciOturumu(fabrika).GetAsync(yol);

            Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
            Assert.StartsWith("/Account/Login", TarayiciOturumu.YonlendirmeYolu(yanit));
        }

        [Theory]
        [MemberData(nameof(YoneticiEkranlari))]
        public async Task Yonetici_ButunEkranlariAcabilir(string yol)
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var oturum = new TarayiciOturumu(fabrika);
            await oturum.GirisYapAsync("admin", "123456");

            var yanit = await oturum.GetAsync(yol);

            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
        }

        [Theory]
        [InlineData("/Kullanici")]
        [InlineData("/Parametre")]
        public async Task StandartKullanici_YonetimEkranlarinaGiremez(string yol)
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var oturum = new TarayiciOturumu(fabrika);
            await oturum.GirisYapAsync("ayse.yilmaz", "123456");

            var yanit = await oturum.GetAsync(yol);

            Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
            Assert.StartsWith("/Account/AccessDenied", TarayiciOturumu.YonlendirmeYolu(yanit));
        }

        [Theory]
        [InlineData("/Cari/Edit/99999")]
        [InlineData("/SatisFaturasi/Detay/99999")]
        [InlineData("/YokBoyleBirSayfa")]
        public async Task OlmayanKayit_404Doner(string yol)
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var oturum = new TarayiciOturumu(fabrika);
            await oturum.GirisYapAsync("admin", "123456");

            var yanit = await oturum.GetAsync(yol);

            Assert.Equal(HttpStatusCode.NotFound, yanit.StatusCode);
        }

        [Fact]
        public async Task HataliSifreyle_GirisYapilamaz()
        {
            UygulamaFabrikasi.VeritabaniGerekli();

            var yanit = await new TarayiciOturumu(fabrika).FormGonderAsync("/Account/Login", "/Account/Login",
                ("kullaniciAdi", "admin"), ("sifre", "yanlis-sifre"));

            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
            Assert.Contains("Kullanıcı adı veya şifre hatalı.", await TarayiciOturumu.MetinAsync(yanit));
        }

        [Fact]
        public async Task GuvenlikBasliklari_HerYanittaBulunur()
        {
            UygulamaFabrikasi.VeritabaniGerekli();

            var yanit = await new TarayiciOturumu(fabrika).GetAsync("/Account/Login");

            var csp = string.Join(" ", yanit.Headers.GetValues("Content-Security-Policy"));
            Assert.Contains("script-src 'self' 'nonce-", csp);
            Assert.Contains("frame-ancestors 'none'", csp);
            Assert.Equal("DENY", yanit.Headers.GetValues("X-Frame-Options").Single());
            Assert.Equal("nosniff", yanit.Headers.GetValues("X-Content-Type-Options").Single());
        }

        [Fact]
        public async Task StokKarti_FormdanEklenir()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var oturum = new TarayiciOturumu(fabrika);
            await oturum.GirisYapAsync("admin", "123456");

            var yanit = await oturum.FormGonderAsync("/StokKarti/Create", "/StokKarti/Create",
                ("StokKodu", "ENT-001"), ("StokAdi", "Entegrasyon Ürünü"), ("Birim", "Adet"), ("KdvOrani", "20"),
                ("AlisFiyati", "10,50"), ("SatisFiyati", "15,75"), ("KritikStok", "2"), ("Aktif", "true"));

            Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
            var liste = await oturum.SayfaAsync("/StokKarti");
            Assert.Contains("ENT-001", liste);
            Assert.Contains("Stok kartı kaydedildi.", liste);
        }

        [Fact]
        public async Task DuzenlemeFormu_FormdanGelenIdyiDikkateAlmaz()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var oturum = new TarayiciOturumu(fabrika);
            await oturum.GirisYapAsync("admin", "123456");

            // Adres 1 numaralı cariyi gösterirken formun içine başka bir carinin Id'si eklenir (overposting denemesi).
            var yanit = await oturum.FormGonderAsync("/Cari/Edit/1", "/Cari/Edit/1",
                ("Id", "2"), ("CariKodu", "M0001"), ("Unvan", "ABC Ticaret (güncel)"), ("CariTipi", "1"), ("Aktif", "true"));

            Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
            using var scope = fabrika.Services.CreateScope();
            var cariler = scope.ServiceProvider.GetRequiredService<ICariService>();
            Assert.Equal("ABC Ticaret (güncel)", (await cariler.GetCariByIdAsync(1))!.Unvan);
            Assert.Equal("Yılmaz Gıda A.Ş.", (await cariler.GetCariByIdAsync(2))!.Unvan);
        }
    }
}
