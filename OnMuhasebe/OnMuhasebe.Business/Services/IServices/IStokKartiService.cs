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
    }
}
