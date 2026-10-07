using OnMuhasebe.Application.Models;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface ISatisFaturasiService
    {
        Task<List<SatisFaturasi>> GetAllSatisFaturalariAsync(DateTime? baslangic, DateTime? bitis);
        Task<SatisFaturasi?> GetSatisFaturasiByIdAsync(int id);
        Task<List<SatisElemaniCirosu>> GetSatisElemaniCirolariAsync(DateTime? baslangic, DateTime? bitis);
        Task<string> GetYeniFaturaNoAsync();
        Task<SatisFaturasi> CreateSatisFaturasiAsync(SatisFaturasi satisFaturasi, int kullaniciId);
        Task DeleteSatisFaturasiAsync(int id);
    }
}
