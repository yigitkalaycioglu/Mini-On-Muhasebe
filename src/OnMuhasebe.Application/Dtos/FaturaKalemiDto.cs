using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Satış ve alış faturası formundaki bir kalem. Satır tutarı serviste hesaplanır.</summary>
    public class FaturaKalemiDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Ürün seçilmelidir.")]
        public int StokId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal Miktar { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Birim fiyat negatif olamaz.")]
        public decimal BirimFiyat { get; set; }

        [Range(0, 100, ErrorMessage = "KDV oranı 0-100 arasında olmalıdır.")]
        public decimal KdvOrani { get; set; }
    }
}
