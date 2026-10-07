using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Stok kartı ekleme ve düzenleme formu. Mevcut miktar formdan gelmez, hareketlerden hesaplanır.</summary>
    public class StokKartiDto
    {
        [Required(ErrorMessage = "Stok kodu zorunludur.")]
        [StringLength(20, ErrorMessage = "Stok kodu en fazla 20 karakter olabilir.")]
        public string StokKodu { get; set; } = "";

        [Required(ErrorMessage = "Stok adı zorunludur.")]
        [StringLength(150, ErrorMessage = "Stok adı en fazla 150 karakter olabilir.")]
        public string StokAdi { get; set; } = "";

        [Required(ErrorMessage = "Birim zorunludur.")]
        [StringLength(10, ErrorMessage = "Birim en fazla 10 karakter olabilir.")]
        public string Birim { get; set; } = "";

        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public decimal KdvOrani { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Alış fiyatı negatif olamaz.")]
        public decimal AlisFiyati { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı negatif olamaz.")]
        public decimal SatisFiyati { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Kritik stok seviyesi negatif olamaz.")]
        public decimal KritikStok { get; set; }

        public bool Aktif { get; set; } = true;

        public static StokKartiDto FromEntity(StokKarti stok) => new()
        {
            StokKodu = stok.StokKodu,
            StokAdi = stok.StokAdi,
            Birim = stok.Birim,
            KdvOrani = stok.KdvOrani,
            AlisFiyati = stok.AlisFiyati,
            SatisFiyati = stok.SatisFiyati,
            KritikStok = stok.KritikStok,
            Aktif = stok.Aktif
        };

        public void ApplyTo(StokKarti stok)
        {
            stok.StokKodu = StokKodu;
            stok.StokAdi = StokAdi;
            stok.Birim = Birim;
            stok.KdvOrani = KdvOrani;
            stok.AlisFiyati = AlisFiyati;
            stok.SatisFiyati = SatisFiyati;
            stok.KritikStok = KritikStok;
            stok.Aktif = Aktif;
        }
    }
}
