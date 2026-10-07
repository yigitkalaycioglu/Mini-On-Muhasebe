using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class StokHareket
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ürün seçilmelidir.")]
        public int StokId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Required(ErrorMessage = "Hareket tipi zorunludur.")]
        [RegularExpression("^(Satis|Alis|SayimFazlasi|SayimEksigi)$", ErrorMessage = "Geçersiz hareket tipi.")]
        public string HareketTipi { get; set; } = null!; // "Satis", "Alis", "SayimFazlasi", "SayimEksigi"

        // Formdan gelmez; controller HareketTipi'ne göre atar (Giris/Cikis).
        public string Yon { get; set; } = null!; // "Giris" veya "Cikis"

        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal Miktar { get; set; }

        // Formdan gelmez; controller otomatik üretip atar.
        public string? BelgeNo { get; set; } // İlgili fatura/fiş no

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        // Formdan gelmez; oturum açan kullanıcıdan controller atar.
        public int KullaniciId { get; set; }

        // Navigation properties
        public StokKarti StokKarti { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
    }
}
