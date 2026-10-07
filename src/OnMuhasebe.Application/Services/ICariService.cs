using OnMuhasebe.Application.Models;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface ICariService
    {
        Task<Cari?> GetCariByIdAsync(int id);
        Task<List<Cari>> GetAllCarilerAsync();
        Task<Cari?> GetCariEkstresiAsync(int id, DateTime? baslangic, DateTime? bitis);
        Task<decimal> GetDevirBakiyeAsync(int cariId, DateTime? baslangic);
        Task<List<CariBakiyesi>> GetCariBakiyeleriAsync();
        Task<Cari> CreateCariAsync(Cari cari);
        Task UpdateCariAsync(Cari cari);
        // true: kayıt silindi, false: bağlı kayıtları olduğu için pasife alındı.
        Task<bool> DeleteCariAsync(int id);

        // Cari bakiye hiçbir yerde saklanmaz; CariHareketler'den hesaplanır.
        decimal Bakiye(Cari cari);
        decimal HareketEtkisi(CariHareket hareket);
        decimal ToplamBorc(Cari cari);
        decimal ToplamAlacak(Cari cari);
        bool MusteriMi(Cari cari);
        bool TedarikciMi(Cari cari);
        string CariTipiAdi(byte cariTipi);
        string IslemTipiAdi(string islemTipi);
    }
}
