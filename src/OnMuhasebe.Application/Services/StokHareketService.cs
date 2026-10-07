using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Extensions;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public class StokHareketService : IStokHareketService
    {
        private readonly IApplicationDbContext _context;
        private readonly IParametreService _parametreService;
        private readonly IStokKartiService _stokKartiService;
        public StokHareketService(IApplicationDbContext context, IParametreService parametreService, IStokKartiService stokKartiService)
        {
            _context = context;
            _parametreService = parametreService;
            _stokKartiService = stokKartiService;
        }

        public async Task<List<StokHareket>> GetAllStokHareketleriAsync(int? stokId, DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.StokHareketler
                .Include(h => h.StokKarti)
                .Include(h => h.Kullanici)
                .AsQueryable();

            if (stokId.HasValue)
            {
                query = query.Where(h => h.StokId == stokId.Value);
            }
            query = query.TarihAraliginda(h => h.Tarih, baslangic, bitis);

            return await query.OrderByDescending(h => h.Tarih).ThenByDescending(h => h.Id).ToListAsync();
        }

        public async Task<StokHareket> CreateSayimFisiAsync(StokHareket sayimFisi, int kullaniciId)
        {
            if (sayimFisi == null)
            {
                throw new ArgumentNullException(nameof(sayimFisi));
            }
            if (kullaniciId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(kullaniciId));
            }

            // Sayım fazlası stoğu artırır (giriş), sayım eksiği azaltır (çıkış).
            string formatParametresi;
            if (sayimFisi.HareketTipi == Sabitler.HareketSayimFazlasi)
            {
                sayimFisi.Yon = Sabitler.YonGiris;
                formatParametresi = Sabitler.ParamSayimFazlasiFisNoFormati;
            }
            else if (sayimFisi.HareketTipi == Sabitler.HareketSayimEksigi)
            {
                sayimFisi.Yon = Sabitler.YonCikis;
                formatParametresi = Sabitler.ParamSayimEksigiFisNoFormati;
            }
            else
            {
                throw new IsKuraliException("Fiş türü Sayım Fazlası veya Sayım Eksiği olmalıdır.");
            }

            if (sayimFisi.Miktar <= 0)
            {
                throw new IsKuraliException("Miktar sıfırdan büyük olmalıdır.");
            }

            var stokKarti = await _context.StokKartlari.FindAsync(sayimFisi.StokId);
            if (stokKarti == null || !stokKarti.Aktif)
            {
                throw new IsKuraliException("Geçerli ve aktif bir ürün seçilmelidir.");
            }

            // Negatif stok kontrolü açıksa sayım eksiği mevcut miktarı aşamaz.
            if (sayimFisi.Yon == Sabitler.YonCikis && await _parametreService.AcikMiAsync(Sabitler.ParamNegatifStokKontrolu))
            {
                var mevcut = await _stokKartiService.GetMevcutMiktarAsync(sayimFisi.StokId);
                if (mevcut < sayimFisi.Miktar)
                {
                    throw new IsKuraliException($"{stokKarti.StokAdi} için yeterli stok yok. Mevcut: {mevcut:N2}, sayım eksiği: {sayimFisi.Miktar:N2}");
                }
            }

            // Fiş numarası aynı türdeki fişler arasında sıralı üretilir (ör. SF-2026-0003).
            var tip = sayimFisi.HareketTipi;
            sayimFisi.BelgeNo = await _parametreService.YeniBelgeNoAsync(
                formatParametresi,
                _context.StokHareketler.Where(h => h.HareketTipi == tip).Select(h => h.BelgeNo));
            sayimFisi.Id = 0; // Id veritabanında üretilir
            sayimFisi.KullaniciId = kullaniciId;

            _context.StokHareketler.Add(sayimFisi);
            await _context.SaveChangesAsync();
            return sayimFisi;
        }

        public async Task DeleteSayimFisiAsync(int id)
        {
            var hareket = await _context.StokHareketler
                .Include(h => h.StokKarti)
                .FirstOrDefaultAsync(h => h.Id == id);
            if (hareket == null)
            {
                throw new KayitBulunamadiException("Stok hareketi bulunamadı.");
            }

            // Fatura hareketleri faturayla birlikte yaşar; tek başına silinirse fatura ile stok tutarsız kalır.
            if (hareket.HareketTipi != Sabitler.HareketSayimFazlasi && hareket.HareketTipi != Sabitler.HareketSayimEksigi)
            {
                throw new IsKuraliException("Fatura hareketleri buradan silinemez; ilgili faturayı silin.");
            }

            // Sayım fazlası silinirse stok azalır; negatif stok kontrolü açıksa eksiye düşürülmez.
            if (hareket.Yon == Sabitler.YonGiris && await _parametreService.AcikMiAsync(Sabitler.ParamNegatifStokKontrolu))
            {
                var mevcut = await _stokKartiService.GetMevcutMiktarAsync(hareket.StokId);
                if (mevcut < hareket.Miktar)
                {
                    throw new IsKuraliException($"{hareket.BelgeNo} silinirse {hareket.StokKarti.StokAdi} stoğu eksiye düşer. Mevcut: {mevcut:N2}");
                }
            }

            _context.StokHareketler.Remove(hareket);
            await _context.SaveChangesAsync();
        }
    }
}
