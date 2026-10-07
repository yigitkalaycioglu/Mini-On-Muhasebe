using OnMuhasebe.Application.Dtos;

namespace OnMuhasebe.Web.ViewModels
{
    /// <summary>Kullanıcı düzenleme ekranı. Id formdan değil adresten gelir; form alanları DTO'da.</summary>
    public class KullaniciDuzenleViewModel
    {
        public int Id { get; set; }
        public KullaniciDto Form { get; set; } = new();
    }
}
