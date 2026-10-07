using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Tahsilat / ödeme formu. Tutar serviste borç ya da alacağa yazılır, belge numarası serviste üretilir.</summary>
    public class TahsilatOdemeDto
    {
        [Required(ErrorMessage = "İşlem tipi zorunludur.")]
        public string IslemTipi { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Cari seçilmelidir.")]
        public int CariId { get; set; }

        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        public decimal? Tutar { get; set; }

        [StringLength(20, ErrorMessage = "Ödeme türü en fazla 20 karakter olabilir.")]
        public string? OdemeTuru { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        public CariHareket ToEntity() => new()
        {
            IslemTipi = IslemTipi,
            CariId = CariId,
            Tarih = Tarih,
            OdemeTuru = OdemeTuru,
            Aciklama = Aciklama
        };
    }
}
