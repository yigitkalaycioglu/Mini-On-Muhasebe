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
