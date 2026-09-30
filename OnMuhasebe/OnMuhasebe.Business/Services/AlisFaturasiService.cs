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
        private readonly IStokKartiService _stokKartiService;
        private readonly ICariService _cariService;
        public AlisFaturasiService(ApplicationDbContext context, IParametreService parametreService, IStokKartiService stokKartiService, ICariService cariService)
        {
            _context = context;
            _parametreService = parametreService;
            _stokKartiService = stokKartiService;
            _cariService = cariService;
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

            if (alisFaturasi.AlisFaturaSatirlari.Count == 0)
            {
                throw new InvalidOperationException("Faturaya en az bir kalem ekleyin.");
            }

            // Form ekranda süzülse de istek elle değiştirilebilir; seçimler sunucuda da doğrulanır.
            var cari = await _context.Cariler.FindAsync(alisFaturasi.CariId);
            if (cari == null || !cari.Aktif || !_cariService.TedarikciMi(cari))
            {
                throw new InvalidOperationException("Geçerli ve aktif bir tedarikçi seçilmelidir.");
            }

            var stokIdler = alisFaturasi.AlisFaturaSatirlari.Select(k => k.StokId).Distinct().ToList();
            var aktifStokSayisi = await _context.StokKartlari.CountAsync(s => stokIdler.Contains(s.Id) && s.Aktif);
            if (aktifStokSayisi != stokIdler.Count)
            {
                throw new InvalidOperationException("Faturadaki ürünlerden biri bulunamadı ya da pasif.");
            }

            // Id'ler veritabanında üretilir; istekle gelen değerler yok sayılır.
            alisFaturasi.Id = 0;
            foreach (var kalem in alisFaturasi.AlisFaturaSatirlari)
            {
                kalem.Id = 0;
            }

            var faturaNo = await GetYeniFaturaNoAsync();
            alisFaturasi.FaturaNo = faturaNo;
            alisFaturasi.KullaniciId = kullaniciId;
            alisFaturasi.OlusturmaTarihi = DateTime.Now;
            alisFaturasi.AraToplam = 0;
            alisFaturasi.KdvToplam = 0;

            // Tutarlar "Ondalık Basamak" parametresine göre ticari usulde (yarım yukarı) yuvarlanır;
            // Math.Round varsayılanı bankacı yuvarlamasıdır (0,365 → 0,36) ve ekrandaki hesapla uyuşmaz.
            var basamak = await _parametreService.GetOndalikBasamakAsync();

            foreach (var kalem in alisFaturasi.AlisFaturaSatirlari)
            {
                kalem.SatirTutari = Math.Round(kalem.Miktar * kalem.BirimFiyat, basamak, MidpointRounding.AwayFromZero);
                alisFaturasi.AraToplam += kalem.SatirTutari;
                alisFaturasi.KdvToplam += Math.Round(kalem.SatirTutari * kalem.KdvOrani / 100, basamak, MidpointRounding.AwayFromZero);

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
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Aynı anda kaydedilen iki fatura aynı numarayı alırsa benzersiz indeks ikincisini reddeder.
                if (await _context.AlisFaturalari.AsNoTracking().AnyAsync(f => f.FaturaNo == faturaNo))
                {
                    throw new InvalidOperationException($"{faturaNo} numarası bu sırada başka bir faturaya verildi. Faturayı tekrar kaydedin.");
                }
                throw;
            }
            return alisFaturasi;
        }

        public async Task DeleteAlisFaturasiAsync(int id)
        {
            var alisFaturasi = await _context.AlisFaturalari.FindAsync(id);
            if (alisFaturasi == null)
            {
                throw new KeyNotFoundException("Alış faturası bulunamadı.");
            }

            // Hareketler belge numarası ve türüyle bulunur; başka türde aynı numaralı belge olsa da ona dokunulmaz.
            var stokHareketleri = await _context.StokHareketler
                .Include(h => h.StokKarti)
                .Where(h => h.BelgeNo == alisFaturasi.FaturaNo && h.HareketTipi == Sabitler.HareketAlis)
                .ToListAsync();

            // Alış silinince girişler geri alınır ve stok azalır. Ürünler bu arada satıldıysa
            // negatif stok kontrolü açıkken stoğun eksiye düşmesine izin verilmez.
            if (await _parametreService.AcikMiAsync(Sabitler.ParamNegatifStokKontrolu))
            {
                foreach (var grup in stokHareketleri.GroupBy(h => h.StokId))
                {
                    var mevcut = await _stokKartiService.GetMevcutMiktarAsync(grup.Key);
                    var geriAlinacak = grup.Sum(h => h.Miktar);
                    if (mevcut < geriAlinacak)
                    {
                        var stokAdi = grup.First().StokKarti.StokAdi;
                        throw new InvalidOperationException($"{alisFaturasi.FaturaNo} silinirse {stokAdi} stoğu eksiye düşer. Mevcut: {mevcut:N2}, faturadaki giriş: {geriAlinacak:N2}");
                    }
                }
            }
            _context.StokHareketler.RemoveRange(stokHareketleri);

            var cariHareketleri = await _context.CariHareketler
                .Where(h => h.BelgeNo == alisFaturasi.FaturaNo && h.IslemTipi == Sabitler.IslemAlis)
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

            query = query.TarihAraliginda(f => f.Tarih, baslangic, bitis);

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
