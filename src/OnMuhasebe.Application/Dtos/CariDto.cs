using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Cari kartı ekleme ve düzenleme formu. Id ve bakiye formdan gelmez.</summary>
    public class CariDto
    {
        [Required(ErrorMessage = "Cari kodu zorunludur.")]
        [StringLength(20, ErrorMessage = "Cari kodu en fazla 20 karakter olabilir.")]
        public string CariKodu { get; set; } = "";

        [Required(ErrorMessage = "Unvan zorunludur.")]
        [StringLength(150, ErrorMessage = "Unvan en fazla 150 karakter olabilir.")]
        public string Unvan { get; set; } = "";

        [Range(1, 3, ErrorMessage = "Cari tipi seçilmelidir.")]
        public byte CariTipi { get; set; }

        [StringLength(50, ErrorMessage = "Vergi dairesi en fazla 50 karakter olabilir.")]
        public string? VergiDairesi { get; set; }

        [StringLength(20, ErrorMessage = "Vergi/TC no en fazla 20 karakter olabilir.")]
        public string? VergiNo { get; set; }

        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string? Telefon { get; set; }

        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        public string? Adres { get; set; }

        public bool Aktif { get; set; } = true;

        public static CariDto FromEntity(Cari cari) => new()
        {
            CariKodu = cari.CariKodu,
            Unvan = cari.Unvan,
            CariTipi = cari.CariTipi,
            VergiDairesi = cari.VergiDairesi,
            VergiNo = cari.VergiNo,
            Telefon = cari.Telefon,
            Adres = cari.Adres,
            Aktif = cari.Aktif
        };

        public void ApplyTo(Cari cari)
        {
            cari.CariKodu = CariKodu;
            cari.Unvan = Unvan;
            cari.CariTipi = CariTipi;
            cari.VergiDairesi = VergiDairesi;
            cari.VergiNo = VergiNo;
            cari.Telefon = Telefon;
            cari.Adres = Adres;
            cari.Aktif = Aktif;
        }
    }
}
