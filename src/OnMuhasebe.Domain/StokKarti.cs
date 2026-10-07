using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class StokKarti
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Stok kodu zorunludur.")]
        [StringLength(20, ErrorMessage = "Stok kodu en fazla 20 karakter olabilir.")]
        public string StokKodu { get; set; } = null!;

        [Required(ErrorMessage = "Stok adı zorunludur.")]
        [StringLength(150, ErrorMessage = "Stok adı en fazla 150 karakter olabilir.")]
        public string StokAdi { get; set; } = null!;

        [Required(ErrorMessage = "Birim zorunludur.")]
        [StringLength(10, ErrorMessage = "Birim en fazla 10 karakter olabilir.")]
        public string Birim { get; set; } = null!; // Adet, Kg, Lt, vb.

        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public decimal KdvOrani { get; set; } // Varsayılan parametreden gelebilir

        [Range(0, double.MaxValue, ErrorMessage = "Alış fiyatı negatif olamaz.")]
        public decimal AlisFiyati { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Satış fiyatı negatif olamaz.")]
        public decimal SatisFiyati { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Kritik stok seviyesi negatif olamaz.")]
        public decimal KritikStok { get; set; } // Uyarı seviyesi

        public bool Aktif { get; set; } = true;

        // Navigation properties
        public ICollection<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; } = new List<SatisFaturaSatiri>();
        public ICollection<AlisFaturaSatiri> AlisFaturaSatirlari { get; set; } = new List<AlisFaturaSatiri>();
        public ICollection<StokHareket> StokHareketleri { get; set; } = new List<StokHareket>();
    }
}
