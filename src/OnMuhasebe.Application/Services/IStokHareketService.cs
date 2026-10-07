using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface IStokHareketService
    {
        Task<List<StokHareket>> GetAllStokHareketleriAsync(int? stokId, DateTime? baslangic, DateTime? bitis);

        // Sayım fişleri yalnızca stoğu etkiler; cari hesaba işlemez.
        Task<StokHareket> CreateSayimFisiAsync(SayimFisiDto fis, int kullaniciId);
        Task DeleteSayimFisiAsync(int id);
    }
}
