using OnMuhasebe.Infrastructure.Security;

namespace OnMuhasebe.UnitTests.Infrastructure
{
    public class Pbkdf2SifreHashleyiciTests
    {
        private readonly Pbkdf2SifreHashleyici _hashleyici = new();

        [Fact]
        public void HashOlustur_IterasyonSaltVeOzetBiciminde()
        {
            var parcalar = _hashleyici.HashOlustur("Sifre.123").Split('.');

            Assert.Equal(3, parcalar.Length);
            Assert.Equal("600000", parcalar[0]);
            Assert.Equal(16, Convert.FromBase64String(parcalar[1]).Length);
            Assert.Equal(32, Convert.FromBase64String(parcalar[2]).Length);
        }

        [Fact]
        public void HashOlustur_AyniSifreIcinHerSeferindeFarkliSaltKullanir()
        {
            Assert.NotEqual(_hashleyici.HashOlustur("123456"), _hashleyici.HashOlustur("123456"));
        }

        [Fact]
        public void Dogrula_DogruSifreyiKabulYanlisiReddeder()
        {
            var hash = _hashleyici.HashOlustur("Sifre.123");

            Assert.True(_hashleyici.Dogrula("Sifre.123", hash));
            Assert.False(_hashleyici.Dogrula("sifre.123", hash));
            Assert.False(_hashleyici.Dogrula("", hash));
        }

        [Fact]
        public void Dogrula_OrnekVeridekiOzetlerleUyumludur()
        {
            // database/seed_data.sql: admin kullanıcısı, şifre "123456". Biçim değişirse kayıtlı kullanıcılar giriş yapamaz.
            const string adminHash = "600000.Y3kSZaRgetUjROpcqMWOCA==.JfqchpyJXoV3yzqv7aq38r4LGaZhVMTVUDXUw3N1ydA=";

            Assert.True(_hashleyici.Dogrula("123456", adminHash));
            Assert.False(_hashleyici.Dogrula("1234567", adminHash));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("duz-metin-sifre")]
        [InlineData("600000.!!!.???")]
        [InlineData("abc.Y3kSZaRgetUjROpcqMWOCA==.JfqchpyJXoV3yzqv7aq38r4LGaZhVMTVUDXUw3N1ydA=")]
        public void Dogrula_GecersizYaDaEksikOzetteFalseDoner(string? kayitliHash)
        {
            Assert.False(_hashleyici.Dogrula("123456", kayitliHash));
        }
    }
}
