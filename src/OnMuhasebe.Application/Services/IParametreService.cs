using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public interface IParametreService
    {
        Task<List<Parametre>> GetAllParametrelerAsync();
        Task UpdateParametrelerAsync(List<Parametre> parametreler);

        // Parametre tablosunda kayıt yoksa ya da değer okunamıyorsa dökümandaki varsayılan değer döner.
        Task<string> GetDegerAsync(string parametreKodu);
        Task<bool> AcikMiAsync(string parametreKodu);
        Task<decimal> GetSayiAsync(string parametreKodu);

        // Tutarların yuvarlanacağı ve gösterileceği basamak (0-2; kolonlar decimal(18,2)).
        Task<int> GetOndalikBasamakAsync();

        /// <summary>
        /// Formatı parametreden okuyup ("SAT-{yyyy}-{0000}" gibi) sıradaki belge numarasını üretir.
        /// mevcutNolar: aynı türdeki belgelerin kayıtlı numaraları; sorgu olarak verilir, veritabanında süzülür.
        /// </summary>
        Task<string> YeniBelgeNoAsync(string formatParametreKodu, IQueryable<string?> mevcutNolar);

        /// <summary>Sabit bir formattan (parametre olmayan belgeler için) sıradaki numarayı üretir.</summary>
        Task<string> YeniBelgeNoFormattanAsync(string format, IQueryable<string?> mevcutNolar);
    }
}
