using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface ITahsilatOdemeService
    {
        Task<List<CariHareket>> GetAllTahsilatOdemelerAsync(DateTime? baslangic, DateTime? bitis);

        // Tahsilat: müşteri alacaklandırılır (borcu azalır). Ödeme: tedarikçi borçlandırılır (borcumuz azalır).
        Task<CariHareket> CreateTahsilatOdemeAsync(TahsilatOdemeDto islem, int kullaniciId);
        Task DeleteTahsilatOdemeAsync(int id);
    }
}
