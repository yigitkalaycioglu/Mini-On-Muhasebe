using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class CariHareket
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Cari seçilmelidir.")]
        public int CariId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "İşlem tipi zorunludur.")]
        [RegularExpression("^(Satis|Alis|Tahsilat|Odeme)$", ErrorMessage = "Geçersiz işlem tipi.")]
        public string IslemTipi { get; set; } = null!; // "Satis", "Alis", "Tahsilat", "Odeme"

        // Tahsilat/ödeme ekranında controller "tutar" alanından hesaplayıp Borc veya Alacak'a atar; formdan doğrudan gelmez.
        public decimal Borc { get; set; }
        public decimal Alacak { get; set; }

        [StringLength(20, ErrorMessage = "Ödeme türü en fazla 20 karakter olabilir.")]
        public string? OdemeTuru { get; set; } // "Nakit", "Havale", "Çek" (yalnızca tahsilat/ödemede)

        // Formdan gelmez; controller otomatik üretip atar.
        public string? BelgeNo { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        // Formdan gelmez; oturum açan kullanıcıdan controller atar.
        public int KullaniciId { get; set; }

        // Navigation properties
        public Cari Cari { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
    }
}
