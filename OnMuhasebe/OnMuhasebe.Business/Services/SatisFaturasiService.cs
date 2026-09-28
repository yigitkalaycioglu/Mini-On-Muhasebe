using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class SatisFaturasiService : ISatisFaturasiService
    {
        private readonly ApplicationDbContext _context;
        private readonly IParametreService _parametreService;
        private readonly IStokKartiService _stokKartiService;
        private readonly ICariService _cariService;
        public SatisFaturasiService(ApplicationDbContext context, IParametreService parametreService, IStokKartiService stokKartiService, ICariService cariService)
        {
            _context = context;
            _parametreService = parametreService;
            _stokKartiService = stokKartiService;
            _cariService = cariService;
        }

        public async Task<SatisFaturasi> CreateSatisFaturasiAsync(SatisFaturasi satisFaturasi, int kullaniciId)
        {
            if (satisFaturasi == null)
            {
                throw new ArgumentNullException(nameof(satisFaturasi));
            }
            if (kullaniciId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(kullaniciId));
            }

            if (satisFaturasi.SatisFaturaSatirlari.Count == 0)
            {
                throw new InvalidOperationException("Faturaya en az bir kalem ekleyin.");
            }

            // Form ekranda süzülse de istek elle değiştirilebilir; seçimler sunucuda da doğrulanır.
            var cari = await _context.Cariler.FindAsync(satisFaturasi.CariId);
            if (cari == null || !cari.Aktif || !_cariService.MusteriMi(cari))
            {
                throw new InvalidOperationException("Geçerli ve aktif bir müşteri seçilmelidir.");
            }

            var satisElemani = await _context.SatisElemanlari.FindAsync(satisFaturasi.SatisElemaniId);
            if (satisElemani == null || !satisElemani.Aktif)
            {
                throw new InvalidOperationException("Geçerli ve aktif bir satış elemanı seçilmelidir.");
            }

            var stokIdler = satisFaturasi.SatisFaturaSatirlari.Select(k => k.StokId).Distinct().ToList();
            var stokAdlari = await _context.StokKartlari
                .Where(s => stokIdler.Contains(s.Id) && s.Aktif)
                .ToDictionaryAsync(s => s.Id, s => s.StokAdi);
            if (stokAdlari.Count != stokIdler.Count)
            {
                throw new InvalidOperationException("Faturadaki ürünlerden biri bulunamadı ya da pasif.");
            }

            // Id'ler veritabanında üretilir; istekle gelen değerler yok sayılır.
            satisFaturasi.Id = 0;
            foreach (var kalem in satisFaturasi.SatisFaturaSatirlari)
            {
                kalem.Id = 0;
            }

            var faturaNo = await GetYeniFaturaNoAsync();
            satisFaturasi.FaturaNo = faturaNo;
            satisFaturasi.KullaniciId = kullaniciId;
            satisFaturasi.OlusturmaTarihi = DateTime.Now;
            satisFaturasi.AraToplam = 0;
            satisFaturasi.KdvToplam = 0;

            // Negatif stok kontrolü parametreye bağlıdır (Bölüm 8). Açıksa stok yetersizse fatura kaydedilmez.
            // Aynı üründen birden fazla kalem girilmişse kontrol toplam miktar üzerinden yapılır.
            if (await _parametreService.AcikMiAsync(Sabitler.ParamNegatifStokKontrolu))
            {
                foreach (var grup in satisFaturasi.SatisFaturaSatirlari.GroupBy(k => k.StokId))
                {
                    var istenenToplam = grup.Sum(k => k.Miktar);
                    var mevcutMiktar = await _stokKartiService.GetMevcutMiktarAsync(grup.Key);

                    if (mevcutMiktar < istenenToplam)
                    {
                        throw new InvalidOperationException($"{stokAdlari[grup.Key]} için yeterli stok yok. Mevcut: {mevcutMiktar:N2}, istenen: {istenenToplam:N2}");
                    }
                }
            }

            // Tutarlar "Ondalık Basamak" parametresine göre yuvarlanır.
            var basamak = (int)await _parametreService.GetSayiAsync(Sabitler.ParamOndalikBasamak);

            foreach (var kalem in satisFaturasi.SatisFaturaSatirlari)
            {
                kalem.SatirTutari = Math.Round(kalem.Miktar * kalem.BirimFiyat, basamak);
                satisFaturasi.AraToplam += kalem.SatirTutari;
                satisFaturasi.KdvToplam += Math.Round(kalem.SatirTutari * kalem.KdvOrani / 100, basamak);

                var stokHareket = new StokHareket
                {
                    StokId = kalem.StokId,
                    Tarih = satisFaturasi.Tarih,
                    HareketTipi = Sabitler.HareketSatis,
                    Yon = Sabitler.YonCikis,
                    Miktar = kalem.Miktar,
                    BelgeNo = faturaNo,
                    Aciklama = $"{faturaNo} numaralı satış faturası",
                    KullaniciId = kullaniciId
                };
                _context.StokHareketler.Add(stokHareket);
            }
            satisFaturasi.GenelToplam = satisFaturasi.AraToplam + satisFaturasi.KdvToplam;

            var cariHareket = new CariHareket
            {
                CariId = satisFaturasi.CariId,
                Tarih = satisFaturasi.Tarih,
                IslemTipi = Sabitler.IslemSatis,
                BelgeNo = faturaNo,
                Aciklama = $"{faturaNo} numaralı satış faturası",
                Borc = satisFaturasi.GenelToplam,
                Alacak = 0,
                KullaniciId = kullaniciId
            };
            _context.CariHareketler.Add(cariHareket);

            _context.SatisFaturalari.Add(satisFaturasi);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Aynı anda kaydedilen iki fatura aynı numarayı alırsa benzersiz indeks ikincisini reddeder.
                if (await _context.SatisFaturalari.AsNoTracking().AnyAsync(f => f.FaturaNo == faturaNo))
                {
                    throw new InvalidOperationException($"{faturaNo} numarası bu sırada başka bir faturaya verildi. Faturayı tekrar kaydedin.");
                }
                throw;
            }
            return satisFaturasi;
        }

        public async Task DeleteSatisFaturasiAsync(int id)
        {
            var satisFaturasi = await _context.SatisFaturalari.FindAsync(id);
            if (satisFaturasi == null)
            {
                throw new KeyNotFoundException("Satış faturası bulunamadı.");
            }

            // Hareketler belge numarası ve türüyle bulunur; başka türde aynı numaralı belge olsa da ona dokunulmaz.
            var stokHareketleri = await _context.StokHareketler
                .Where(h => h.BelgeNo == satisFaturasi.FaturaNo && h.HareketTipi == Sabitler.HareketSatis)
                .ToListAsync();
            _context.StokHareketler.RemoveRange(stokHareketleri);

            var cariHareketleri = await _context.CariHareketler
                .Where(h => h.BelgeNo == satisFaturasi.FaturaNo && h.IslemTipi == Sabitler.IslemSatis)
                .ToListAsync();
            _context.CariHareketler.RemoveRange(cariHareketleri);

            _context.SatisFaturalari.Remove(satisFaturasi);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SatisFaturasi>> GetAllSatisFaturalariAsync(DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.SatisFaturalari
                .Include(f => f.Cari)
                .Include(f => f.SatisElemani)
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

        /// <summary>Satış elemanı bazında fatura sayısı ve toplamlar; gruplama veritabanında yapılır.</summary>
        public async Task<List<SatisElemaniCirosu>> GetSatisElemaniCirolariAsync(DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.SatisFaturalari.AsQueryable();
            if (baslangic.HasValue)
            {
                query = query.Where(f => f.Tarih >= baslangic.Value.Date);
            }
            if (bitis.HasValue)
            {
                var bitisSonu = bitis.Value.Date.AddDays(1);
                query = query.Where(f => f.Tarih < bitisSonu);
            }

            var toplamlar = await query
                .GroupBy(f => f.SatisElemaniId)
                .Select(g => new
                {
                    SatisElemaniId = g.Key,
                    FaturaSayisi = g.Count(),
                    AraToplam = g.Sum(f => f.AraToplam),
                    KdvToplam = g.Sum(f => f.KdvToplam),
                    GenelToplam = g.Sum(f => f.GenelToplam)
                })
                .ToDictionaryAsync(x => x.SatisElemaniId);

            // Hiç satışı olmayan elemanlar da raporda sıfırla görünür.
            var elemanlar = await _context.SatisElemanlari.AsNoTracking().OrderBy(e => e.AdSoyad).ToListAsync();
            return elemanlar
                .Select(e => toplamlar.TryGetValue(e.Id, out var t)
                    ? new SatisElemaniCirosu
                    {
                        Eleman = e,
                        FaturaSayisi = t.FaturaSayisi,
                        AraToplam = t.AraToplam,
                        KdvToplam = t.KdvToplam,
                        GenelToplam = t.GenelToplam
                    }
                    : new SatisElemaniCirosu { Eleman = e })
                .ToList();
        }

        public async Task<SatisFaturasi?> GetSatisFaturasiByIdAsync(int id)
        {
            return await _context.SatisFaturalari
                .Include(f => f.Cari)
                .Include(f => f.SatisElemani)
                .Include(f => f.Kullanici)
                .Include(f => f.SatisFaturaSatirlari)
                    .ThenInclude(k => k.StokKarti)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<string> GetYeniFaturaNoAsync()
        {
            // Format "Satış Fatura No Formatı" parametresinden gelir (ör. SAT-{yyyy}-{0000}).
            return await _parametreService.YeniBelgeNoAsync(
                Sabitler.ParamSatisFaturaNoFormati,
                _context.SatisFaturalari.Select(f => (string?)f.FaturaNo));
        }
    }
}
