namespace OnMuhasebe.Business
{
    /// <summary>
    /// Veri tabanına yazılan sabit değerler. Modeller yalnızca veri taşıdığı için
    /// bu değerler tek merkezden buradan kullanılır.
    /// </summary>
    public static class Sabitler
    {
        // Kullanicilar.Rol
        public const string RolYonetici = "Yönetici";
        public const string RolStandart = "Standart";

        // Cariler.CariTipi
        public const byte CariTipiMusteri = 1;
        public const byte CariTipiTedarikci = 2;
        public const byte CariTipiHerIkisi = 3;

        // StokHareketler.HareketTipi
        public const string HareketSatis = "Satis";
        public const string HareketAlis = "Alis";
        public const string HareketSayimFazlasi = "SayimFazlasi";
        public const string HareketSayimEksigi = "SayimEksigi";

        // StokHareketler.Yon
        public const string YonGiris = "Giris";
        public const string YonCikis = "Cikis";

        // CariHareketler.IslemTipi
        public const string IslemSatis = "Satis";
        public const string IslemAlis = "Alis";
        public const string IslemTahsilat = "Tahsilat";
        public const string IslemOdeme = "Odeme";
    }
}
