using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class SatisFaturasi
    {
        public int Id { get; set; }

        // Formdan gelmez; controller otomatik üretip atar.
        public string FaturaNo { get; set; } = null!; // Benzersiz, otomatik

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Müşteri seçilmelidir.")]
        public int CariId { get; set; } // Müşteri

        [Range(1, int.MaxValue, ErrorMessage = "Satış elemanı seçilmelidir.")]
        public int SatisElemaniId { get; set; }

        // AraToplam, KdvToplam, GenelToplam kalemlerden sunucuda hesaplanır; formdan gelmez.
        public decimal AraToplam { get; set; } // KDV hariç
        public decimal KdvToplam { get; set; }
        public decimal GenelToplam { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        // Formdan gelmez; oturum açan kullanıcıdan controller atar.
        public int KullaniciId { get; set; }
        public DateTime OlusturmaTarihi { get; set; }

        // Navigation properties
        public Cari Cari { get; set; } = null!;
        public SatisElemani SatisElemani { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
        public ICollection<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; } = new List<SatisFaturaSatiri>();
    }
}
