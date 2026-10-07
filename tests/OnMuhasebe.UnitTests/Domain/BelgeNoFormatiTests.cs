using OnMuhasebe.Domain.Rules;

namespace OnMuhasebe.UnitTests.Domain
{
    public class BelgeNoFormatiTests
    {
        private static readonly DateTime Tarih = new(2026, 10, 7);

        [Fact]
        public void Coz_YilVeSiraAlanlariniAyirir()
        {
            var bicim = BelgeNoFormati.Coz("SAT-{yyyy}-{0000}", Tarih);

            Assert.Equal("SAT-2026-", bicim.OnEk);
            Assert.Equal("", bicim.SonEk);
            Assert.Equal(4, bicim.Basamak);
            Assert.Equal(13, bicim.NumaraUzunlugu);
        }

        [Theory]
        [InlineData("F{yy}/{000}", "F26/", "", 3)]
        [InlineData("{00000}-TAH", "", "-TAH", 5)]
        [InlineData("SF-{0000}-{yyyy}", "SF-", "-2026", 4)]
        public void Coz_IkiHaneliYilVeSonEkiDestekler(string format, string onEk, string sonEk, int basamak)
        {
            var bicim = BelgeNoFormati.Coz(format, Tarih);

            Assert.Equal((onEk, sonEk, basamak), (bicim.OnEk, bicim.SonEk, bicim.Basamak));
        }

        [Theory]
        [InlineData("SAT-{yyyy}")]
        [InlineData("SAT-{0000}-{000}")]
        [InlineData("")]
        public void Coz_TekBirSiraAlaniYoksaHataVerir(string format)
        {
            var hata = Assert.Throws<FormatException>(() => BelgeNoFormati.Coz(format, Tarih));

            Assert.Contains("{0000}", hata.Message);
        }

        [Fact]
        public void Sonraki_HicNumaraYoksaBirdenBaslar()
        {
            var bicim = BelgeNoFormati.Coz("SAT-{yyyy}-{0000}", Tarih);

            Assert.Equal("SAT-2026-0001", bicim.Sonraki([]));
        }

        [Fact]
        public void Sonraki_EnBuyukSiraNumarasininBirFazlasidir()
        {
            var bicim = BelgeNoFormati.Coz("SAT-{yyyy}-{0000}", Tarih);

            var sonraki = bicim.Sonraki(["SAT-2026-0001", "SAT-2026-0009", "SAT-2026-0003"]);

            Assert.Equal("SAT-2026-0010", sonraki);
        }

        [Fact]
        public void Sonraki_FormataUymayanNumaralariYokSayar()
        {
            var bicim = BelgeNoFormati.Coz("SAT-{yyyy}-{0000}", Tarih);

            var sonraki = bicim.Sonraki(["SAT-2025-0050", "ALS-2026-0100", null, "SAT-2026-ABCD", "SAT-2026-0002"]);

            Assert.Equal("SAT-2026-0003", sonraki);
        }

        [Fact]
        public void Sonraki_YeniYildaSiraBastanBaslar()
        {
            var bicim = BelgeNoFormati.Coz("SAT-{yyyy}-{0000}", new DateTime(2027, 1, 1));

            Assert.Equal("SAT-2027-0001", bicim.Sonraki(["SAT-2026-0042"]));
        }
    }
}
