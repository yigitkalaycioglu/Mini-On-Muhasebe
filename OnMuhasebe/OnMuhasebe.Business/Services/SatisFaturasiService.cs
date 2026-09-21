using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class SatisFaturasiService : ISatisFaturasiService
    {
        private readonly ApplicationDbContext _context;
        public SatisFaturasiService(ApplicationDbContext context)
        {
            _context = context;
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

            var faturaNo = await GetYeniFaturaNoAsync();
            satisFaturasi.FaturaNo = faturaNo;
            satisFaturasi.KullaniciId = kullaniciId;
            satisFaturasi.OlusturmaTarihi = DateTime.Now;
            satisFaturasi.AraToplam = 0;
            satisFaturasi.KdvToplam = 0;

            // Aynı üründen birden fazla kalem girilmişse, stok kontrolü toplam miktar üzerinden yapılmalı.
            var stokIdler = satisFaturasi.SatisFaturaSatirlari.Select(k => k.StokId).Distinct().ToList();
            var stokKartlari = await _context.StokKartlari
                .Where(s => stokIdler.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.StokAdi);

            foreach (var grup in satisFaturasi.SatisFaturaSatirlari.GroupBy(k => k.StokId))
            {
                var istenenToplam = grup.Sum(k => k.Miktar);
                var mevcutMiktar = await _context.StokHareketler
                    .Where(h => h.StokId == grup.Key)
                    .SumAsync(StokKartiService.Etki);

                if (mevcutMiktar < istenenToplam)
                {
                    var stokAdi = stokKartlari.TryGetValue(grup.Key, out var ad) ? ad : "Ürün";
                    throw new InvalidOperationException($"{stokAdi} için yeterli stok yok. Mevcut: {mevcutMiktar}, istenen: {istenenToplam}");
                }
            }

            foreach (var kalem in satisFaturasi.SatisFaturaSatirlari)
            {
                kalem.SatirTutari = kalem.Miktar * kalem.BirimFiyat;
                satisFaturasi.AraToplam += kalem.SatirTutari;
                satisFaturasi.KdvToplam += kalem.SatirTutari * kalem.KdvOrani / 100;

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
            await _context.SaveChangesAsync();
            return satisFaturasi;
        }

        public async Task DeleteSatisFaturasiAsync(int id)
        {
            var satisFaturasi = await _context.SatisFaturalari.FindAsync(id);
            if (satisFaturasi == null)
            {
                throw new KeyNotFoundException("Satış faturası bulunamadı.");
            }

            var stokHareketleri = await _context.StokHareketler
                .Where(h => h.BelgeNo == satisFaturasi.FaturaNo)
                .ToListAsync();
            _context.StokHareketler.RemoveRange(stokHareketleri);

            var cariHareketleri = await _context.CariHareketler
                .Where(h => h.BelgeNo == satisFaturasi.FaturaNo)
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
            int dynamicYear = DateTime.Now.Year;
            string prefix = $"SAT-{dynamicYear}-";

            var faturaNolar = await _context.SatisFaturalari
                .Where(f => f.FaturaNo.StartsWith(prefix))
                .Select(f => f.FaturaNo)
                .ToListAsync();

            var sonSira = 0;
            foreach (var no in faturaNolar)
            {
                if (int.TryParse(no.Substring(prefix.Length), out var sira) && sira > sonSira)
                {
                    sonSira = sira;
                }
            }

            return $"{prefix}{(sonSira + 1):D4}";
        }
    }
}
