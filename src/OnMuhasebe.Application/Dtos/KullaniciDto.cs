using System.ComponentModel.DataAnnotations;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Kullanıcı düzenleme formu. Şifre bu formdan değiştirilmez (ayrı "şifre sıfırla" işlemi).</summary>
    public class KullaniciDto
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-50 karakter olmalıdır.")]
        public string KullaniciAdi { get; set; } = "";

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
        public string AdSoyad { get; set; } = "";

        [Required(ErrorMessage = "Rol zorunludur.")]
        [RegularExpression("^(Yönetici|Standart)$", ErrorMessage = "Rol 'Yönetici' veya 'Standart' olmalıdır.")]
        public string Rol { get; set; } = "";

        public bool Aktif { get; set; } = true;

        public static KullaniciDto FromEntity(Kullanici kullanici) => new()
        {
            KullaniciAdi = kullanici.KullaniciAdi,
            AdSoyad = kullanici.AdSoyad,
            Rol = kullanici.Rol,
            Aktif = kullanici.Aktif
        };

        public void ApplyTo(Kullanici kullanici)
        {
            kullanici.KullaniciAdi = KullaniciAdi;
            kullanici.AdSoyad = AdSoyad;
            kullanici.Rol = Rol;
            kullanici.Aktif = Aktif;
        }
    }
}
