namespace OnMuhasebe.Domain
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

        // CariHareketler.OdemeTuru
        public static readonly string[] OdemeTurleri = { "Nakit", "Havale", "Çek" };

        // Tahsilat/ödeme belge numaraları için dökümanda parametre tanımlı değil; sabit format kullanılır.
        public const string TahsilatNoFormati = "TAH-{yyyy}-{0000}";
        public const string OdemeNoFormati = "ODE-{yyyy}-{0000}";

        // Parametreler.ParametreKodu (Bölüm 9)
        public const string ParamFirmaUnvani = "FirmaUnvani";
        public const string ParamVergiDairesiNo = "VergiDairesiNo";
        public const string ParamVarsayilanKdvOrani = "VarsayilanKdvOrani";
        public const string ParamParaBirimi = "ParaBirimi";
        public const string ParamOndalikBasamak = "OndalikBasamak";
        public const string ParamNegatifStokKontrolu = "NegatifStokKontrolu";
        public const string ParamSatisFaturaNoFormati = "SatisFaturaNoFormati";
        public const string ParamAlisFaturaNoFormati = "AlisFaturaNoFormati";
        public const string ParamSayimFazlasiFisNoFormati = "SayimFazlasiFisNoFormati";
        public const string ParamSayimEksigiFisNoFormati = "SayimEksigiFisNoFormati";
        public const string ParamKritikStokUyarisi = "KritikStokUyarisi";

        // Açık/kapalı türündeki parametrelerin değerleri
        public const string DegerAcik = "Acik";
        public const string DegerKapali = "Kapali";
    }
}
