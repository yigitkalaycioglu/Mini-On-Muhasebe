using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class SatisElemani
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string AdSoyad { get; set; } = null!;

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string Telefon { get; set; } = null!;

        public bool Aktif { get; set; } = true;

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
    }
}
