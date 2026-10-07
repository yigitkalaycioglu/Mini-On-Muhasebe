using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Satış elemanı ekleme ve düzenleme formu.</summary>
    public class SatisElemaniDto
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "Telefon zorunludur.")]
        [StringLength(20, ErrorMessage = "Telefon en fazla 20 karakter olabilir.")]
        public string Telefon { get; set; } = "";

        public bool Aktif { get; set; } = true;

        public static SatisElemaniDto FromEntity(SatisElemani eleman) => new()
        {
            AdSoyad = eleman.AdSoyad,
            Telefon = eleman.Telefon,
            Aktif = eleman.Aktif
        };

        public void ApplyTo(SatisElemani eleman)
        {
            eleman.AdSoyad = AdSoyad;
            eleman.Telefon = Telefon;
            eleman.Aktif = Aktif;
        }
    }
}
