using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class SatisFaturaSatiri
    {
        public int Id { get; set; }

        // Formdan gelmez; controller faturayı kaydederken atar.
        public int SatisFaturaId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Ürün seçilmelidir.")]
        public int StokId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal Miktar { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
        public decimal BirimFiyat { get; set; }

        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public decimal KdvOrani { get; set; }

        // Miktar × BirimFiyat; sunucuda hesaplanır, formdan gelmez.
        public decimal SatirTutari { get; set; } // Miktar × BirimFiyat (KDV hariç)

        // Navigation properties
        public SatisFaturasi SatisFaturasi { get; set; } = null!;
        public StokKarti StokKarti { get; set; } = null!;
    }
}
