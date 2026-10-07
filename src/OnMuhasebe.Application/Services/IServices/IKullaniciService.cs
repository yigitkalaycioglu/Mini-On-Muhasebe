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
        // islemYapanId: oturumdaki kullanıcı; kendi yetkisini kaldırması ve son yöneticinin kaldırılması engellenir.
        Task UpdateKullaniciAsync(Kullanici kullanici, int islemYapanId);
        // true: kayıt silindi, false: bağlı kayıtları olduğu için pasife alındı.
        Task<bool> DeleteKullaniciAsync(int id, int islemYapanId);

        Task SifreSifirlaAsync(int id, string yeniSifre);
    }
}
