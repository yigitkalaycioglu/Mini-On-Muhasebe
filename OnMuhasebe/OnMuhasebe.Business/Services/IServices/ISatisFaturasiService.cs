using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface ISatisFaturasiService
    {
        Task<List<SatisFaturasi>> GetAllSatisFaturalariAsync(DateTime? baslangic, DateTime? bitis);
        Task<SatisFaturasi?> GetSatisFaturasiByIdAsync(int id);
        Task<string> GetYeniFaturaNoAsync();
        Task<SatisFaturasi> CreateSatisFaturasiAsync(SatisFaturasi satisFaturasi, int kullaniciId);
        Task DeleteSatisFaturasiAsync(int id);
    }
}
