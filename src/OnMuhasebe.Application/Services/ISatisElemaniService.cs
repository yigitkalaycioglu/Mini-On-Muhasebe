using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface ISatisElemaniService
    {
        Task<SatisElemani?> GetSatisElemaniByIdAsync(int id);
        Task<List<SatisElemani>> GetAllSatisElemanlariAsync();
        Task<bool> IsSatisElemaniNameUniqueAsync(string adSoyad, int? excludeId = null);
        Task<SatisElemani> CreateSatisElemaniAsync(SatisElemaniDto satisElemani);
        Task UpdateSatisElemaniAsync(int id, SatisElemaniDto satisElemani);
        // true: kayıt silindi, false: bağlı kayıtları olduğu için pasife alındı.
        Task<bool> DeleteSatisElemaniAsync(int id);
    }
}
