using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class Parametre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Parametre kodu zorunludur.")]
        [StringLength(50, ErrorMessage = "Parametre kodu en fazla 50 karakter olabilir.")]
        public string ParametreKodu { get; set; } = null!; // Benzersiz

        [Required(ErrorMessage = "Parametre değeri zorunludur.")]
        [StringLength(250, ErrorMessage = "Parametre değeri en fazla 250 karakter olabilir.")]
        public string ParametreDegeri { get; set; } = null!;

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }
    }
}
