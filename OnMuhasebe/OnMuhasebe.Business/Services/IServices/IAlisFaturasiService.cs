using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IAlisFaturasiService
    {
        Task<List<AlisFaturasi>> GetAllAlisFaturalariAsync(DateTime? baslangic, DateTime? bitis);
        Task<AlisFaturasi?> GetAlisFaturasiByIdAsync(int id);
        Task<string> GetYeniFaturaNoAsync();
        Task<AlisFaturasi> CreateAlisFaturasiAsync(AlisFaturasi alisFaturasi, int kullaniciId);
        Task DeleteAlisFaturasiAsync(int id);
    }
}
