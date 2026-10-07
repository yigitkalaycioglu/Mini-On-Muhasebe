using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>
    /// Parametreler ekranındaki bir satır. Kaydederken yalnızca değer kullanılır; kod ve açıklama ekranda
    /// gösterilir, kod formdan değiştirilse bile servis kayıttaki koda göre doğrular.
    /// </summary>
    public class ParametreDegeriDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Parametre kodu zorunludur.")]
        [StringLength(50, ErrorMessage = "Parametre kodu en fazla 50 karakter olabilir.")]
        public string ParametreKodu { get; set; } = "";

        [Required(ErrorMessage = "Parametre değeri zorunludur.")]
        [StringLength(250, ErrorMessage = "Parametre değeri en fazla 250 karakter olabilir.")]
        public string ParametreDegeri { get; set; } = "";

        public string? Aciklama { get; set; }

        public static ParametreDegeriDto FromEntity(Parametre parametre) => new()
        {
            Id = parametre.Id,
            ParametreKodu = parametre.ParametreKodu,
            ParametreDegeri = parametre.ParametreDegeri,
            Aciklama = parametre.Aciklama
        };
    }
}
