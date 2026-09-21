using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IStokKartiService
    {
        Task<StokKarti?> GetStokKartiByIdAsync(int id);
        Task<List<StokKarti>> GetAllStokKartlariAsync();
        Task<StokKarti> CreateStokKartiAsync(StokKarti stokKarti);
        Task UpdateStokKartiAsync(StokKarti stokKarti);
        Task DeleteStokKartiAsync(int id);

        // Stok miktarı hiçbir yerde saklanmaz; StokHareketler'den hesaplanır.
        decimal MevcutMiktar(StokKarti stokKarti);
        decimal ToplamGiris(StokKarti stokKarti);
        decimal ToplamCikis(StokKarti stokKarti);
        bool KritikSeviyede(StokKarti stokKarti);
        string HareketTipiAdi(string hareketTipi);
    }
}
