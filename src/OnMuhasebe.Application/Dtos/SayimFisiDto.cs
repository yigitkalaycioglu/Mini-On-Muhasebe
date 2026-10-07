using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Sayım fazlası / eksiği fişi formu. Yön (giriş/çıkış) ve fiş numarası serviste belirlenir.</summary>
    public class SayimFisiDto
    {
        [Required(ErrorMessage = "Hareket tipi zorunludur.")]
        public string HareketTipi { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Ürün seçilmelidir.")]
        public int StokId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Miktar sıfırdan büyük olmalıdır.")]
        public decimal Miktar { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        public StokHareket ToEntity() => new()
        {
            HareketTipi = HareketTipi,
            StokId = StokId,
            Tarih = Tarih,
            Miktar = Miktar,
            Aciklama = Aciklama
        };
    }
}
