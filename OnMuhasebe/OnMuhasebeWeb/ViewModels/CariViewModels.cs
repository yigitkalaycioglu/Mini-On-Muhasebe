using OnMuhasebe.Models;

namespace OnMuhasebeWeb.ViewModels
{
    // Ekranların ihtiyaç duyduğu hazır veriler. Hesaplar servis katmanında yapılır,
    // buraya sonuç olarak gelir; view hiçbir hesap yapmaz.

    public class CariSatiriViewModel
    {
        public Cari Cari { get; set; } = null!;
        public string TipAdi { get; set; } = "";
        public decimal Bakiye { get; set; }
    }

    public class CariListesiViewModel
    {
        public List<CariSatiriViewModel> Satirlar { get; set; } = new();
    }

    public class CariOzetiViewModel
    {
        public decimal ToplamBorc { get; set; }
        public decimal ToplamAlacak { get; set; }
        public decimal Bakiye { get; set; }
    }

    public class EkstreSatiriViewModel
    {
        public CariHareket Hareket { get; set; } = null!;
        public string IslemAdi { get; set; } = "";
        public decimal YuruyenBakiye { get; set; }
    }

    public class CariEkstreViewModel
    {
        public Cari? Cari { get; set; }
        public List<Cari> Cariler { get; set; } = new();
        public List<EkstreSatiriViewModel> Satirlar { get; set; } = new();
        public decimal DevirBakiye { get; set; }
        public decimal ToplamBorc { get; set; }
        public decimal ToplamAlacak { get; set; }
        public decimal SonBakiye { get; set; }
    }

    public class CariBakiyeSatiriViewModel
    {
        public Cari Cari { get; set; } = null!;
        public string TipAdi { get; set; } = "";
        public decimal Borc { get; set; }
        public decimal Alacak { get; set; }
        public decimal Bakiye { get; set; }
    }

    public class CariBakiyeRaporViewModel
    {
        public List<CariBakiyeSatiriViewModel> Satirlar { get; set; } = new();
        public decimal Alacagimiz { get; set; }
        public decimal Borcumuz { get; set; }
    }

    public class TahsilatOdemeSatiriViewModel
    {
        public CariHareket Hareket { get; set; } = null!;
        public string IslemAdi { get; set; } = "";
        public bool Tahsilat { get; set; }
        public decimal Tutar { get; set; }
    }

    public class TahsilatOdemeListesiViewModel
    {
        public List<TahsilatOdemeSatiriViewModel> Satirlar { get; set; } = new();
        public decimal ToplamTahsilat { get; set; }
        public decimal ToplamOdeme { get; set; }
        public decimal Net { get; set; }
    }

    public class TahsilatOdemeCariViewModel
    {
        public Cari Cari { get; set; } = null!;
        public string UygunIslemler { get; set; } = "";
    }
}
