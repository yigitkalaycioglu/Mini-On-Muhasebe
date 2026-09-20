using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IKullaniciService
    {
        Task<Kullanici?> GetKullaniciByIdAsync(int id);
        Task<Kullanici?> DogrulaAsync(string kullaniciAdi, string sifre);
        Task<List<Kullanici>> GetAllKullanicilarAsync();
        Task<bool> IsKullaniciNameUniqueAsync(string kullaniciAdi, int? excludeId = null);
        Task<Kullanici> CreateKullaniciAsync(Kullanici kullanici, string sifre);
        Task UpdateKullaniciAsync(Kullanici kullanici);
        Task DeleteKullaniciAsync(int id);

        Task SifreSifirlaAsync(int id, string yeniSifre);
    }
}
