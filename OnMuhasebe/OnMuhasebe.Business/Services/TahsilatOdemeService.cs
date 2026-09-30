using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class TahsilatOdemeService : ITahsilatOdemeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParametreService _parametreService;
        private readonly ICariService _cariService;
        public TahsilatOdemeService(ApplicationDbContext context, IParametreService parametreService, ICariService cariService)
        {
            _context = context;
            _parametreService = parametreService;
            _cariService = cariService;
        }

        private static bool TahsilatVeyaOdeme(string islemTipi) =>
            islemTipi == Sabitler.IslemTahsilat || islemTipi == Sabitler.IslemOdeme;

        public async Task<List<CariHareket>> GetAllTahsilatOdemelerAsync(DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.CariHareketler
                .Include(h => h.Cari)
                .Include(h => h.Kullanici)
                .Where(h => h.IslemTipi == Sabitler.IslemTahsilat || h.IslemTipi == Sabitler.IslemOdeme);

            query = query.TarihAraliginda(h => h.Tarih, baslangic, bitis);

            return await query.OrderByDescending(h => h.Tarih).ThenByDescending(h => h.Id).ToListAsync();
        }

        public async Task<CariHareket> CreateTahsilatOdemeAsync(CariHareket hareket, decimal tutar, int kullaniciId)
        {
            if (hareket == null)
            {
                throw new ArgumentNullException(nameof(hareket));
            }
            if (kullaniciId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(kullaniciId));
            }
            if (!TahsilatVeyaOdeme(hareket.IslemTipi))
            {
                throw new InvalidOperationException("İşlem Tahsilat veya Ödeme olmalıdır.");
            }

            // Tutar "Ondalık Basamak" parametresine göre yuvarlanır.
            var basamak = await _parametreService.GetOndalikBasamakAsync();
            tutar = Math.Round(tutar, basamak, MidpointRounding.AwayFromZero);
            if (tutar <= 0)
            {
                throw new InvalidOperationException("Tutar sıfırdan büyük olmalıdır.");
            }

            if (string.IsNullOrEmpty(hareket.OdemeTuru) || !Sabitler.OdemeTurleri.Contains(hareket.OdemeTuru))
            {
                throw new InvalidOperationException("Ödeme türü Nakit, Havale veya Çek olmalıdır.");
            }

            var tahsilat = hareket.IslemTipi == Sabitler.IslemTahsilat;

            // Tahsilat müşteriden, ödeme tedarikçiye yapılır.
            var cari = await _context.Cariler.FindAsync(hareket.CariId);
            if (cari == null || !cari.Aktif)
            {
                throw new InvalidOperationException("Geçerli ve aktif bir cari seçilmelidir.");
            }
            if (tahsilat && !_cariService.MusteriMi(cari))
            {
                throw new InvalidOperationException("Tahsilat yalnızca müşteri carilerinden yapılabilir.");
            }
            if (!tahsilat && !_cariService.TedarikciMi(cari))
            {
                throw new InvalidOperationException("Ödeme yalnızca tedarikçi carilerine yapılabilir.");
            }

            hareket.Borc = tahsilat ? 0 : tutar;
            hareket.Alacak = tahsilat ? tutar : 0;

            // Tahsilat/ödeme belge numarası için dökümanda parametre yok; sabit format kullanılır (TAH-/ODE-).
            var tip = hareket.IslemTipi;
            hareket.BelgeNo = await _parametreService.YeniBelgeNoFormattanAsync(
                tahsilat ? Sabitler.TahsilatNoFormati : Sabitler.OdemeNoFormati,
                _context.CariHareketler.Where(h => h.IslemTipi == tip).Select(h => h.BelgeNo));
            hareket.Id = 0; // Id veritabanında üretilir
            hareket.KullaniciId = kullaniciId;

            _context.CariHareketler.Add(hareket);
            await _context.SaveChangesAsync();
            return hareket;
        }

        public async Task DeleteTahsilatOdemeAsync(int id)
        {
            var hareket = await _context.CariHareketler.FindAsync(id);
            if (hareket == null)
            {
                throw new KeyNotFoundException("Kayıt bulunamadı.");
            }

            // Fatura hareketleri faturayla birlikte yaşar; tek başına silinirse fatura ile cari tutarsız kalır.
            if (!TahsilatVeyaOdeme(hareket.IslemTipi))
            {
                throw new InvalidOperationException("Fatura hareketleri buradan silinemez; ilgili faturayı silin.");
            }

            _context.CariHareketler.Remove(hareket);
            await _context.SaveChangesAsync();
        }
    }
}
