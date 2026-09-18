using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface ICariService
    {
        Task<Cari?> GetCariByIdAsync(int id);
        Task<List<Cari>> GetAllCarilerAsync();
        Task<Cari?> GetCariEkstresiAsync(int id, DateTime? baslangic, DateTime? bitis);
        Task<decimal> GetDevirBakiyeAsync(int cariId, DateTime? baslangic);
        Task<Cari> CreateCariAsync(Cari cari);
        Task UpdateCariAsync(Cari cari);
        Task DeleteCariAsync(int id);
    }
}
