using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class Kullanici
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter olmalıdır.")]
        public string KullaniciAdi { get; set; } = null!;

        // Formdan gelmez; controller şifreyi hash'leyip burada atar.
        public string SifreHash { get; set; } = null!;

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string AdSoyad { get; set; } = null!;

        [Required(ErrorMessage = "Rol zorunludur.")]
        [RegularExpression("^(Yönetici|Standart)$", ErrorMessage = "Rol 'Yönetici' veya 'Standart' olmalıdır.")]
        public string Rol { get; set; } = null!; // "Yönetici" veya "Standart"

        public bool Aktif { get; set; } = true;

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
        public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
        public ICollection<StokHareket> StokHareketleri { get; set; } = new List<StokHareket>();
        public ICollection<CariHareket> CariHareketleri { get; set; } = new List<CariHareket>();
    }
}
