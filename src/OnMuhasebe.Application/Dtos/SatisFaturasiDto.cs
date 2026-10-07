using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Satış faturası formu. Numara, toplamlar ve işlemi yapan kullanıcı formdan gelmez; serviste atanır.</summary>
    public class SatisFaturasiDto
    {
        [Required(ErrorMessage = "Tarih zorunludur.")]
        public DateTime Tarih { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Müşteri seçilmelidir.")]
        public int CariId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Satış elemanı seçilmelidir.")]
        public int SatisElemaniId { get; set; }

        [StringLength(250, ErrorMessage = "Açıklama en fazla 250 karakter olabilir.")]
        public string? Aciklama { get; set; }

        public List<FaturaKalemiDto> Kalemler { get; set; } = new();

        public SatisFaturasi ToEntity() => new()
        {
            Tarih = Tarih,
            CariId = CariId,
            SatisElemaniId = SatisElemaniId,
            Aciklama = Aciklama,
            SatisFaturaSatirlari = Kalemler.Select(k => new SatisFaturaSatiri
            {
                StokId = k.StokId,
                Miktar = k.Miktar,
                BirimFiyat = k.BirimFiyat,
                KdvOrani = k.KdvOrani
            }).ToList()
        };
    }
}
