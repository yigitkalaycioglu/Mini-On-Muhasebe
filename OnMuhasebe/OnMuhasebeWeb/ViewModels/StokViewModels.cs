using OnMuhasebe.Models;

namespace OnMuhasebeWeb.ViewModels
{
    // Ekranların ihtiyaç duyduğu hazır veriler. Hesaplar servis katmanında yapılır,
    // buraya sonuç olarak gelir; view hiçbir hesap yapmaz.

    public class StokSatiriViewModel
    {
        public StokKarti Stok { get; set; } = null!;
        public decimal Mevcut { get; set; }
        public bool Kritik { get; set; }
    }

    public class StokListesiViewModel
    {
        public List<StokSatiriViewModel> Satirlar { get; set; } = new();
        public int KritikSayisi { get; set; }
    }

    public class StokOzetiViewModel
    {
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
        public decimal Mevcut { get; set; }
        public bool Kritik { get; set; }
    }

    public class StokDurumSatiriViewModel
    {
        public StokKarti Stok { get; set; } = null!;
        public decimal Giris { get; set; }
        public decimal Cikis { get; set; }
        public decimal Mevcut { get; set; }
        public decimal Deger { get; set; }
        public string Seviye { get; set; } = "";
    }

    public class StokDurumRaporViewModel
    {
        public List<StokDurumSatiriViewModel> Satirlar { get; set; } = new();
        public int KritikSayisi { get; set; }
        public int TukenenSayisi { get; set; }
        public decimal ToplamDeger { get; set; }
    }

    public class KritikStokSatiriViewModel
    {
        public StokKarti Stok { get; set; } = null!;
        public decimal Mevcut { get; set; }
        public decimal Eksik { get; set; }
        public decimal Maliyet { get; set; }
    }

    public class KritikStokRaporViewModel
    {
        public List<KritikStokSatiriViewModel> Satirlar { get; set; } = new();
        public decimal ToplamMaliyet { get; set; }
    }

    public class StokHareketSatiriViewModel
    {
        public StokHareket Hareket { get; set; } = null!;
        public string TipAdi { get; set; } = "";
        public string TipSinifi { get; set; } = "";
        public decimal? GirisMiktari { get; set; }
        public decimal? CikisMiktari { get; set; }
    }

    public class StokHareketListesiViewModel
    {
        public List<StokHareketSatiriViewModel> Satirlar { get; set; } = new();
        public StokKarti? FiltreliStok { get; set; }
        public decimal ToplamGiris { get; set; }
        public decimal ToplamCikis { get; set; }
    }

    public class SayimFisiStokViewModel
    {
        public StokKarti Stok { get; set; } = null!;
        public decimal Mevcut { get; set; }
    }
}
