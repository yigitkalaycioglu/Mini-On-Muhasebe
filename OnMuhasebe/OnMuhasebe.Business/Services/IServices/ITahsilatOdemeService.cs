using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services.IServices
{
    public interface ITahsilatOdemeService
    {
        Task<List<CariHareket>> GetAllTahsilatOdemelerAsync(DateTime? baslangic, DateTime? bitis);

        // Tahsilat: müşteri alacaklandırılır (borcu azalır). Ödeme: tedarikçi borçlandırılır (borcumuz azalır).
        Task<CariHareket> CreateTahsilatOdemeAsync(CariHareket hareket, decimal tutar, int kullaniciId);
        Task DeleteTahsilatOdemeAsync(int id);
    }
}
