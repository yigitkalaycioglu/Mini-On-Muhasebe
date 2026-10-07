namespace OnMuhasebe.Application.Dtos
{
    /// <summary>Yeni kullanıcı formu: düzenleme alanlarına ek olarak ilk şifre.</summary>
    public class KullaniciOlusturDto : KullaniciDto
    {
        public string? Sifre { get; set; }
        public string? SifreTekrar { get; set; }
    }
}
