using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class AlisFaturasiService : IAlisFaturasiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParametreService _parametreService;
        public AlisFaturasiService(ApplicationDbContext context, IParametreService parametreService)
        {
            _context = context;
            _parametreService = parametreService;
        }

        public async Task<AlisFaturasi> CreateAlisFaturasiAsync(AlisFaturasi alisFaturasi, int kullaniciId)
        {
            if (alisFaturasi == null)
            {
                throw new ArgumentNullException(nameof(alisFaturasi));
            }
            if (kullaniciId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(kullaniciId));
            }

            var faturaNo = await GetYeniFaturaNoAsync();
            alisFaturasi.FaturaNo = faturaNo;
            alisFaturasi.KullaniciId = kullaniciId;
            alisFaturasi.OlusturmaTarihi = DateTime.Now;
            alisFaturasi.AraToplam = 0;
            alisFaturasi.KdvToplam = 0;

            // Tutarlar "Ondalık Basamak" parametresine göre yuvarlanır.
            var basamak = (int)await _parametreService.GetSayiAsync(Sabitler.ParamOndalikBasamak);

            foreach (var kalem in alisFaturasi.AlisFaturaSatirlari)
            {
                kalem.SatirTutari = Math.Round(kalem.Miktar * kalem.BirimFiyat, basamak);
                alisFaturasi.AraToplam += kalem.SatirTutari;
                alisFaturasi.KdvToplam += Math.Round(kalem.SatirTutari * kalem.KdvOrani / 100, basamak);

                var stokHareket = new StokHareket
                {
                    StokId = kalem.StokId,
                    Tarih = alisFaturasi.Tarih,
                    HareketTipi = Sabitler.HareketAlis,
                    Yon = Sabitler.YonGiris,
                    Miktar = kalem.Miktar,
                    BelgeNo = faturaNo,
                    Aciklama = $"{faturaNo} numaralı alış faturası",
                    KullaniciId = kullaniciId
                };
                _context.StokHareketler.Add(stokHareket);
            }
            alisFaturasi.GenelToplam = alisFaturasi.AraToplam + alisFaturasi.KdvToplam;

            var cariHareket = new CariHareket
            {
                CariId = alisFaturasi.CariId,
                Tarih = alisFaturasi.Tarih,
                IslemTipi = Sabitler.IslemAlis,
                BelgeNo = faturaNo,
                Aciklama = $"{faturaNo} numaralı alış faturası",
                Borc = 0,
                Alacak = alisFaturasi.GenelToplam,
                KullaniciId = kullaniciId
            };
            _context.CariHareketler.Add(cariHareket);

            _context.AlisFaturalari.Add(alisFaturasi);
            await _context.SaveChangesAsync();
            return alisFaturasi;
        }

        public async Task DeleteAlisFaturasiAsync(int id)
        {
            var alisFaturasi = await _context.AlisFaturalari.FindAsync(id);
            if (alisFaturasi == null)
            {
                throw new KeyNotFoundException("Alış faturası bulunamadı.");
            }

            var stokHareketleri = await _context.StokHareketler
                .Where(h => h.BelgeNo == alisFaturasi.FaturaNo)
                .ToListAsync();
            _context.StokHareketler.RemoveRange(stokHareketleri);

            var cariHareketleri = await _context.CariHareketler
                .Where(h => h.BelgeNo == alisFaturasi.FaturaNo)
                .ToListAsync();
            _context.CariHareketler.RemoveRange(cariHareketleri);

            _context.AlisFaturalari.Remove(alisFaturasi);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AlisFaturasi>> GetAllAlisFaturalariAsync(DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.AlisFaturalari
                .Include(f => f.Cari)
                .Include(f => f.Kullanici)
                .AsQueryable();

            if (baslangic.HasValue)
            {
                query = query.Where(f => f.Tarih >= baslangic.Value.Date);
            }
            if (bitis.HasValue)
            {
                // Bitiş günü dahil olsun diye ertesi günün başlangıcından küçük olanlar alınır.
                var bitisSonu = bitis.Value.Date.AddDays(1);
                query = query.Where(f => f.Tarih < bitisSonu);
            }

            return await query.OrderByDescending(f => f.Tarih).ThenByDescending(f => f.Id).ToListAsync();
        }

        public async Task<AlisFaturasi?> GetAlisFaturasiByIdAsync(int id)
        {
            return await _context.AlisFaturalari
                .Include(f => f.Cari)
                .Include(f => f.Kullanici)
                .Include(f => f.AlisFaturaSatirlari)
                    .ThenInclude(k => k.StokKarti)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<string> GetYeniFaturaNoAsync()
        {
            // Format "Alış Fatura No Formatı" parametresinden gelir (ör. ALS-{yyyy}-{0000}).
            return await _parametreService.YeniBelgeNoAsync(
                Sabitler.ParamAlisFaturaNoFormati,
                _context.AlisFaturalari.Select(f => (string?)f.FaturaNo));
        }
    }
}
