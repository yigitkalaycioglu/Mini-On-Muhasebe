using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Models
{
    public class Cari
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Cari kodu zorunludur.")]
        [StringLength(20, ErrorMessage = "Cari kodu en fazla 20 karakter olabilir.")]
        public string CariKodu { get; set; } = null!;

        [Required(ErrorMessage = "Unvan zorunludur.")]
        [StringLength(150, ErrorMessage = "Unvan en fazla 150 karakter olabilir.")]
        public string Unvan { get; set; } = null!;

        [Range(1, 3, ErrorMessage = "Cari tipi seçilmelidir.")]
        public byte CariTipi { get; set; } // 1=Müşteri, 2=Tedarikçi, 3=Her ikisi

        [StringLength(50, ErrorMessage = "Vergi dairesi en fazla 50 karakter olabilir.")]
        public string? VergiDairesi { get; set; }

        [StringLength(20, ErrorMessage = "Vergi/TC no en fazla 20 karakter olabilir.")]
        public string? VergiNo { get; set; }

        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Telefon { get; set; }

        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string? Adres { get; set; }

        public bool Aktif { get; set; } = true;

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
        public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
        public ICollection<CariHareket> CariHareketleri { get; set; } = new List<CariHareket>();
    }
}
