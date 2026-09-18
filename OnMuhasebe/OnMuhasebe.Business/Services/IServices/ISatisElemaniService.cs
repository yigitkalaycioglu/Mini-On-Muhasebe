using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface ISatisElemaniService
    {
        Task<SatisElemani?> GetSatisElemaniByIdAsync(int id);
        Task<List<SatisElemani>> GetAllSatisElemanlariAsync();
        Task<bool> IsSatisElemaniNameUniqueAsync(string adSoyad, int? excludeId = null);
        Task<SatisElemani> CreateSatisElemaniAsync(SatisElemani satisElemani);
        Task UpdateSatisElemaniAsync(SatisElemani satisElemani);
        Task DeleteSatisElemaniAsync(int id);
    }
}
