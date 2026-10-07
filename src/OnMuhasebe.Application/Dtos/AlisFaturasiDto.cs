using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Alış faturası formu. Numara, toplamlar ve işlemi yapan kullanıcı formdan gelmez; serviste atanır.</summary>
    public class AlisFaturasiDto
    {
        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Tedarikçi seçilmelidir.")]
        public int CariId { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        public List<FaturaKalemiDto> Kalemler { get; set; } = new();

        public AlisFaturasi ToEntity() => new()
        {
            Tarih = Tarih,
            CariId = CariId,
            Aciklama = Aciklama,
            AlisFaturaSatirlari = Kalemler.Select(k => new AlisFaturaSatiri
            {
                StokId = k.StokId,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani
            }).ToList()
        };
    }
}
