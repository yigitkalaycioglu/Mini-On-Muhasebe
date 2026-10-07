using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;

namespace OnMuhasebe.IntegrationTests
{
    /// <summary>
    /// Dokümanın iş kuralları servisler üzerinden, gerçek veritabanıyla. Her işlem ayrı bir kapsamda (yeni DbContext)
    /// yapılır; testler kendi cari ve stok kartlarını oluşturduğu için birbirini etkilemez.
    /// </summary>
    [Collection(VeritabaniKoleksiyonu.Ad)]
    public class IsKuraliTestleri(UygulamaFabrikasi fabrika)
    {
        private const int Yonetici = 1;   // seed_data.sql: admin

        private async Task<T> Servis<TServis, T>(Func<TServis, Task<T>> islem) where TServis : notnull
        {
            using var scope = fabrika.Services.CreateScope();
            return await islem(scope.ServiceProvider.GetRequiredService<TServis>());
        }

        private static string Kod(string onEk) => onEk + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

        private Task<int> CariEkleAsync(byte tip) =>
            Servis<ICariService, int>(async s => (await s.CreateCariAsync(new CariDto { CariKodu = Kod("C"), Unvan = "Test Carisi", CariTipi = tip })).Id);

        private Task<int> StokEkleAsync() =>
            Servis<IStokKartiService, int>(async s => (await s.CreateStokKartiAsync(new StokKartiDto
            {
                StokKodu = Kod("S"), StokAdi = "Test Ürünü", Birim = "Adet", KdvOrani = 20, AlisFiyati = 10, SatisFiyati = 15, KritikStok = 2
            })).Id);

        private Task<decimal> MevcutAsync(int stokId) => Servis<IStokKartiService, decimal>(s => s.GetMevcutMiktarAsync(stokId));

        private Task<int> CariHareketSayisiAsync() => Servis<IApplicationDbContext, int>(db => db.CariHareketler.CountAsync());

        private Task<decimal> BakiyeAsync(int cariId) =>
            Servis<ICariService, decimal>(async s => s.Bakiye((await s.GetCariByIdAsync(cariId))!));

        private Task<int> AlisAsync(int tedarikci, int stok, decimal miktar) =>
            Servis<IAlisFaturasiService, int>(async s => (await s.CreateAlisFaturasiAsync(new AlisFaturasiDto
            {
                CariId = tedarikci, Tarih = DateTime.Today, Kalemler = [new FaturaKalemiDto { StokId = stok, Miktar = miktar, BirimFiyat = 10, KdvOrani = 20 }]
            }, Yonetici)).Id);

        private Task<int> SatisAsync(int musteri, int stok, decimal miktar) =>
            Servis<ISatisFaturasiService, int>(async s => (await s.CreateSatisFaturasiAsync(new SatisFaturasiDto
            {
                CariId = musteri, SatisElemaniId = 1, Tarih = DateTime.Today,
                Kalemler = [new FaturaKalemiDto { StokId = stok, Miktar = miktar, BirimFiyat = 15, KdvOrani = 20 }]
            }, Yonetici)).Id);

        [Fact]
        public async Task Faturalar_StoguVeCariBakiyeyiGunceller_SilinceGeriAlir()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var tedarikci = await CariEkleAsync(Sabitler.CariTipiTedarikci);
            var musteri = await CariEkleAsync(Sabitler.CariTipiMusteri);
            var stok = await StokEkleAsync();

            await AlisAsync(tedarikci, stok, 10);
            var satis = await SatisAsync(musteri, stok, 4);

            Assert.Equal(6, await MevcutAsync(stok));
            Assert.Equal(72m, await BakiyeAsync(musteri));       // 4 × 15 = 60 + %20 KDV → müşteri bize borçlu
            Assert.Equal(-120m, await BakiyeAsync(tedarikci));   // 10 × 10 = 100 + %20 KDV → biz tedarikçiye borçluyuz

            await Servis<ISatisFaturasiService, bool>(async s => { await s.DeleteSatisFaturasiAsync(satis); return true; });

            Assert.Equal(10, await MevcutAsync(stok));
            Assert.Equal(0m, await BakiyeAsync(musteri));
        }

        [Fact]
        public async Task YetersizStokla_SatisYapilamaz()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var tedarikci = await CariEkleAsync(Sabitler.CariTipiTedarikci);
            var musteri = await CariEkleAsync(Sabitler.CariTipiMusteri);
            var stok = await StokEkleAsync();
            await AlisAsync(tedarikci, stok, 3);

            var hata = await Assert.ThrowsAsync<IsKuraliException>(() => SatisAsync(musteri, stok, 5));

            Assert.Contains("yeterli stok yok", hata.Message);
            Assert.Equal(3, await MevcutAsync(stok));
        }

        [Fact]
        public async Task SayimFisi_YalnizcaStoguEtkiler()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var stok = await StokEkleAsync();
            var cariHareketSayisi = await CariHareketSayisiAsync();

            var fis = await Servis<IStokHareketService, string>(async s => (await s.CreateSayimFisiAsync(new SayimFisiDto
            {
                HareketTipi = Sabitler.HareketSayimFazlasi, StokId = stok, Tarih = DateTime.Today, Miktar = 7
            }, Yonetici)).BelgeNo!);

            Assert.StartsWith("SF-", fis);
            Assert.Equal(7, await MevcutAsync(stok));
            Assert.Equal(cariHareketSayisi, await CariHareketSayisiAsync());
        }

        [Fact]
        public async Task Tahsilat_MusteriyiAlacaklandirir_TedarikcidenYapilamaz()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var musteri = await CariEkleAsync(Sabitler.CariTipiMusteri);
            var tedarikci = await CariEkleAsync(Sabitler.CariTipiTedarikci);

            await Servis<ITahsilatOdemeService, int>(async s => (await s.CreateTahsilatOdemeAsync(new TahsilatOdemeDto
            {
                IslemTipi = Sabitler.IslemTahsilat, CariId = musteri, Tarih = DateTime.Today, Tutar = 50.005m, OdemeTuru = "Nakit"
            }, Yonetici)).Id);

            Assert.Equal(-50.01m, await BakiyeAsync(musteri));   // tutar parametredeki 2 basamağa yarım yukarı yuvarlanır
            await Assert.ThrowsAsync<IsKuraliException>(() => Servis<ITahsilatOdemeService, int>(async s => (await s.CreateTahsilatOdemeAsync(new TahsilatOdemeDto
            {
                IslemTipi = Sabitler.IslemTahsilat, CariId = tedarikci, Tarih = DateTime.Today, Tutar = 10, OdemeTuru = "Nakit"
            }, Yonetici)).Id));
        }

        [Fact]
        public async Task FaturaNumaralari_FormataGoreSiraylaUretilir()
        {
            UygulamaFabrikasi.VeritabaniGerekli();
            var tedarikci = await CariEkleAsync(Sabitler.CariTipiTedarikci);
            var stok = await StokEkleAsync();

            var ilk = await Servis<IAlisFaturasiService, string>(s => s.GetYeniFaturaNoAsync());
            await AlisAsync(tedarikci, stok, 1);
            var sonraki = await Servis<IAlisFaturasiService, string>(s => s.GetYeniFaturaNoAsync());

            Assert.Matches(@"^ALS-\d{4}-\d{4}$", ilk);
            Assert.Equal(int.Parse(ilk[^4..]) + 1, int.Parse(sonraki[^4..]));
        }

        [Fact]
        public async Task SonAktifYonetici_PasifeAlinamaz()
        {
            UygulamaFabrikasi.VeritabaniGerekli();

            var hata = await Assert.ThrowsAsync<IsKuraliException>(() => Servis<IKullaniciService, bool>(async s =>
            {
                await s.UpdateKullaniciAsync(Yonetici, new KullaniciDto { KullaniciAdi = "admin", AdSoyad = "Admin Kullanıcı", Rol = Sabitler.RolYonetici, Aktif = false }, islemYapanId: 2);
                return true;
            }));

            Assert.Equal("Sistemde en az bir aktif yönetici kalmalıdır.", hata.Message);
        }
    }
}
