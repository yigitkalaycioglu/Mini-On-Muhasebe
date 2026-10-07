using OnMuhasebe.Application.Dtos;

namespace OnMuhasebe.Web.ViewModels
{
    /// <summary>Satış elemanı düzenleme ekranı. Id formdan değil adresten gelir; form alanları DTO'da.</summary>
    public class SatisElemaniDuzenleViewModel
    {
        public int Id { get; set; }
        public SatisElemaniDto Form { get; set; } = new();
    }
}
