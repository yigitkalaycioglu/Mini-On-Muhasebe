using OnMuhasebe.Domain.Rules;

namespace OnMuhasebe.UnitTests.Domain
{
    public class TutarHesabiTests
    {
        [Theory]
        [InlineData("0.365", 2, "0.37")]     // bankacı yuvarlaması 0,36 verirdi
        [InlineData("0.375", 2, "0.38")]
        [InlineData("-0.365", 2, "-0.37")]
        [InlineData("2.5", 0, "3")]
        [InlineData("2.45", 1, "2.5")]
        [InlineData("1.234", 2, "1.23")]
        public void Yuvarla_YarimYukariYuvarlar(string tutar, int basamak, string beklenen)
        {
            Assert.Equal(decimal.Parse(beklenen, System.Globalization.CultureInfo.InvariantCulture),
                TutarHesabi.Yuvarla(decimal.Parse(tutar, System.Globalization.CultureInfo.InvariantCulture), basamak));
        }

        [Fact]
        public void SatirTutari_MiktarCarpiBirimFiyattirVeYuvarlanir()
        {
            // 3 × 0,365 = 1,095 → 1,10
            Assert.Equal(1.10m, TutarHesabi.SatirTutari(3, 0.365m, 2));
        }

        [Fact]
        public void Kdv_SatirTutariUzerindenHesaplanipYuvarlanir()
        {
            // 1,10 × %18 = 0,198 → 0,20
            Assert.Equal(0.20m, TutarHesabi.Kdv(1.10m, 18, 2));
            Assert.Equal(0m, TutarHesabi.Kdv(250m, 0, 2));
        }

        [Fact]
        public void SifirBasamakta_TutarlarTamSayiyaYuvarlanir()
        {
            Assert.Equal(16m, TutarHesabi.SatirTutari(10, 1.55m, 0));
            Assert.Equal(3m, TutarHesabi.Kdv(16m, 18, 0));
        }
    }
}
