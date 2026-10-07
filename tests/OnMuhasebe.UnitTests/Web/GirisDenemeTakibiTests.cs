using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using OnMuhasebe.Web.Security;

namespace OnMuhasebe.UnitTests.Web
{
    public class GirisDenemeTakibiTests
    {
        private readonly FakeTimeProvider _zaman = new(new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.Zero));

        private GirisDenemeTakibi Takip(int enFazlaHata = 5, int kilitDakika = 5) =>
            new(new MemoryCache(new MemoryCacheOptions()), _zaman,
                Options.Create(new GirisGuvenligiAyarlari { EnFazlaHataliDeneme = enFazlaHata, KilitSuresi = TimeSpan.FromMinutes(kilitDakika) }));

        [Fact]
        public void BesinciHataliDenemede_KullaniciAdiKilitlenir()
        {
            var takip = Takip();

            for (var i = 1; i <= 4; i++)
            {
                Assert.False(takip.HataliDenemeKaydet("admin"));
                Assert.Null(takip.KalanKilitSuresi("admin"));
            }

            Assert.True(takip.HataliDenemeKaydet("admin"));
            Assert.Equal(TimeSpan.FromMinutes(5), takip.KalanKilitSuresi("admin"));
        }

        [Fact]
        public void KilitSuresiDolunca_GirisAcilirVeSayimBastanBaslar()
        {
            var takip = Takip();
            for (var i = 0; i < 5; i++)
            {
                takip.HataliDenemeKaydet("admin");
            }

            _zaman.Advance(TimeSpan.FromMinutes(4));
            Assert.Equal(TimeSpan.FromMinutes(1), takip.KalanKilitSuresi("admin"));

            _zaman.Advance(TimeSpan.FromMinutes(1));
            Assert.Null(takip.KalanKilitSuresi("admin"));
            Assert.False(takip.HataliDenemeKaydet("admin"));
        }

        [Fact]
        public void BasariliGiris_SayaciSifirlar()
        {
            var takip = Takip();
            for (var i = 0; i < 4; i++)
            {
                takip.HataliDenemeKaydet("admin");
            }

            takip.Sifirla("admin");

            Assert.False(takip.HataliDenemeKaydet("admin"));
        }

        [Fact]
        public void KullaniciAdi_BuyukKucukHarfVeBosluktanBagimsizSayilir()
        {
            var takip = Takip(enFazlaHata: 2);

            takip.HataliDenemeKaydet("Admin");

            Assert.True(takip.HataliDenemeKaydet("  admin "));
            Assert.NotNull(takip.KalanKilitSuresi("ADMIN"));
        }

        [Fact]
        public void FarkliKullaniciAdlari_BirbiriniEtkilemez()
        {
            var takip = Takip(enFazlaHata: 1);

            Assert.True(takip.HataliDenemeKaydet("admin"));

            Assert.Null(takip.KalanKilitSuresi("ayse.yilmaz"));
        }

        [Fact]
        public void Sinirlar_AyarlardanOkunur()
        {
            var takip = Takip(enFazlaHata: 3, kilitDakika: 30);

            takip.HataliDenemeKaydet("admin");
            takip.HataliDenemeKaydet("admin");

            Assert.True(takip.HataliDenemeKaydet("admin"));
            Assert.Equal(TimeSpan.FromMinutes(30), takip.KalanKilitSuresi("admin"));
        }
    }
}
