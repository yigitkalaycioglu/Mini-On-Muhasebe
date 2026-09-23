using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface IStokHareketService
    {
        Task<List<StokHareket>> GetAllStokHareketleriAsync(int? stokId, DateTime? baslangic, DateTime? bitis);

        // Sayım fişleri yalnızca stoğu etkiler; cari hesaba işlemez.
        Task<StokHareket> CreateSayimFisiAsync(StokHareket sayimFisi, int kullaniciId);
        Task DeleteSayimFisiAsync(int id);
    }
}
