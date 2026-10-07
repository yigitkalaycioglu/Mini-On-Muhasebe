using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Web.ViewModels
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
        public int TukenenSayisi { get; set; }
        public decimal ToplamMaliyet { get; set; }
    }

    public class StokHareketSatiriViewModel
    {
        public StokHareket Hareket { get; set; } = null!;
        public string TipAdi { get; set; } = "";
        public string TipSinifi { get; set; } = "";
        public decimal? GirisMiktari { get; set; }
        public decimal? CikisMiktari { get; set; }

        // Yalnızca sayım fişleri bu ekrandan silinebilir; fatura hareketleri faturayla silinir.
        public bool SayimFisi { get; set; }
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

    public class SatisElemaniSatisSatiriViewModel
    {
        public SatisElemani Eleman { get; set; } = null!;
        public int FaturaSayisi { get; set; }
        public decimal AraToplam { get; set; }
        public decimal KdvToplam { get; set; }
        public decimal GenelToplam { get; set; }
        public decimal Pay { get; set; }
    }

    public class SatisElemaniSatisRaporViewModel
    {
        public List<SatisElemaniSatisSatiriViewModel> Satirlar { get; set; } = new();
        public int ToplamFaturaSayisi { get; set; }
        public decimal ToplamAra { get; set; }
        public decimal ToplamKdv { get; set; }
        public decimal ToplamGenel { get; set; }
    }

    /// <summary>Stok kartı düzenleme ekranı: form alanları, kayıtlı karttan hesaplanan stok özeti.</summary>
    public class StokKartiDuzenleViewModel
    {
        public int Id { get; set; }
        public StokKartiDto Form { get; set; } = new();
        public StokOzetiViewModel Ozet { get; set; } = new();
    }
}
