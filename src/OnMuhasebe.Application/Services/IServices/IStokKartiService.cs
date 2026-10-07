using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IStokKartiService
    {
        Task<StokKarti?> GetStokKartiByIdAsync(int id);
        Task<List<StokKarti>> GetAllStokKartlariAsync();
        Task<StokKarti> CreateStokKartiAsync(StokKarti stokKarti);
        Task UpdateStokKartiAsync(StokKarti stokKarti);
        // true: kayıt silindi, false: bağlı kayıtları olduğu için pasife alındı.
        Task<bool> DeleteStokKartiAsync(int id);
        Task<decimal> GetMevcutMiktarAsync(int stokId);
        Task<int> GetKritikStokSayisiAsync();
        Task<List<StokBakiyesi>> GetStokBakiyeleriAsync(bool yalnizcaAktif = false);

        // Stok miktarı hiçbir yerde saklanmaz; StokHareketler'den hesaplanır.
        decimal MevcutMiktar(StokKarti stokKarti);
        decimal ToplamGiris(StokKarti stokKarti);
        decimal ToplamCikis(StokKarti stokKarti);
        bool KritikSeviyede(StokKarti stokKarti);
        bool KritikSeviyede(StokBakiyesi bakiye);
        string HareketTipiAdi(string hareketTipi);
    }
}
