using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IKullaniciService
    {
        Task<Kullanici?> GetKullaniciByIdAsync(int id);
        Task<List<Kullanici>> GetAllKullanicilarAsync();
        Task<bool> IsKullaniciNameUniqueAsync(string kullaniciAdi, int? excludeId = null);
        Task<Kullanici> CreateKullaniciAsync(Kullanici kullanici);
        Task UpdateKullaniciAsync(Kullanici kullanici);
        Task DeleteKullaniciAsync(int id);
    }
}
