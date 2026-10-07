namespace OnMuhasebe.Domain.Rules
{
    /// <summary>
    /// Tutarlar "Ondalık Basamak" parametresine göre ticari usulde (yarım yukarı) yuvarlanır;
    /// Math.Round varsayılanı bankacı yuvarlamasıdır (0,365 → 0,36) ve ekrandaki hesapla uyuşmaz.
    /// </summary>
    public static class TutarHesabi
    {
        public static decimal Yuvarla(decimal tutar, int basamak) =>
            Math.Round(tutar, basamak, MidpointRounding.AwayFromZero);

        /// <summary>Satır tutarı = Miktar × Birim fiyat (KDV hariç).</summary>
        public static decimal SatirTutari(decimal miktar, decimal birimFiyat, int basamak) =>
            Yuvarla(miktar * birimFiyat, basamak);

        /// <summary>Satırın KDV tutarı; fatura KDV toplamı satır satır yuvarlanmış KDV'lerin toplamıdır.</summary>
        public static decimal Kdv(decimal satirTutari, decimal kdvOrani, int basamak) =>
            Yuvarla(satirTutari * kdvOrani / 100, basamak);
    }
}
