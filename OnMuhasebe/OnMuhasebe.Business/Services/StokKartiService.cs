using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class StokKartiService : IStokKartiService
    {
        private readonly ApplicationDbContext _context;
        public StokKartiService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // Stok hesaplamaları
        // Miktar hiçbir yerde saklanmaz, her zaman StokHareketler'den hesaplanır.
        // Hesabın tek tanımı burasıdır; controller'lar ViewModel kurarken buradan alır.
        // ---------------------------------------------------------------

        /// <summary>Tek hareketin stoğa etkisi: giriş artı, çıkış eksi. EF Core bunu SQL'e çevirir.</summary>
        public static readonly Expression<Func<StokHareket, decimal>> Etki =
            h => h.Yon == Sabitler.YonGiris ? h.Miktar : -h.Miktar;

        private static readonly Func<StokHareket, decimal> EtkiFonksiyonu = Etki.Compile();

        /// <summary>Stok kartının mevcut miktarı. StokHareketleri yüklenmemişse 0 döner.</summary>
        public decimal MevcutMiktar(StokKarti stokKarti) => stokKarti.StokHareketleri.Sum(EtkiFonksiyonu);

        public decimal ToplamGiris(StokKarti stokKarti) =>
            stokKarti.StokHareketleri.Where(h => h.Yon == Sabitler.YonGiris).Sum(h => h.Miktar);

        public decimal ToplamCikis(StokKarti stokKarti) =>
            stokKarti.StokHareketleri.Where(h => h.Yon == Sabitler.YonCikis).Sum(h => h.Miktar);

        /// <summary>Mevcut miktar kritik seviyeye eşit ya da altındaysa uyarı verilir.</summary>
        public bool KritikSeviyede(StokKarti stokKarti) => KritikMi(MevcutMiktar(stokKarti), stokKarti.KritikStok);

        public bool KritikSeviyede(StokBakiyesi bakiye) => KritikMi(bakiye.Mevcut, bakiye.Stok.KritikStok);

        // Kritik seviye kuralının tek tanımı (GetKritikStokSayisiAsync aynı karşılaştırmayı SQL'de yapar).
        private static bool KritikMi(decimal mevcut, decimal kritikStok) => mevcut <= kritikStok;

        public string HareketTipiAdi(string hareketTipi) => hareketTipi switch
        {
            Sabitler.HareketSatis => "Satış",
            Sabitler.HareketAlis => "Alış",
            Sabitler.HareketSayimFazlasi => "Sayım Fazlası",
            Sabitler.HareketSayimEksigi => "Sayım Eksiği",
            _ => hareketTipi
        };

        /// <summary>Tek ürünün mevcut miktarı; toplama veritabanında yapılır (satış ve sayım eksiği kontrolü için).</summary>
        public async Task<decimal> GetMevcutMiktarAsync(int stokId)
        {
            return await _context.StokHareketler
                .Where(h => h.StokId == stokId)
                .SumAsync(Etki);
        }

        /// <summary>Kritik seviyedeki aktif ürün sayısı; hareketler belleğe alınmadan veritabanında hesaplanır.</summary>
        public async Task<int> GetKritikStokSayisiAsync()
        {
            return await _context.StokKartlari
                .Where(s => s.Aktif)
                .CountAsync(s => s.StokHareketleri.AsQueryable().Sum(Etki) <= s.KritikStok);
        }

        /// <summary>
        /// Her kartın giriş, çıkış ve mevcut miktarı. Hareketler belleğe alınmaz;
        /// toplamlar veritabanında hesaplanıp kartla birlikte gelir (liste ve rapor ekranları için).
        /// </summary>
        public async Task<List<StokBakiyesi>> GetStokBakiyeleriAsync(bool yalnizcaAktif = false)
        {
            var query = _context.StokKartlari.AsNoTracking();
            if (yalnizcaAktif)
            {
                query = query.Where(s => s.Aktif);
            }

            return await query
                .OrderBy(s => s.StokKodu)
                .Select(s => new StokBakiyesi
                {
                    Stok = s,
                    Giris = s.StokHareketleri.Where(h => h.Yon == Sabitler.YonGiris).Sum(h => h.Miktar),
                    Cikis = s.StokHareketleri.Where(h => h.Yon == Sabitler.YonCikis).Sum(h => h.Miktar),
                    Mevcut = s.StokHareketleri.AsQueryable().Sum(Etki)
                })
                .ToListAsync();
        }

        public async Task<List<StokKarti>> GetAllStokKartlariAsync()
        {
            return await _context.StokKartlari
                .Include(s => s.StokHareketleri)
                .ToListAsync();
        }

        public async Task<StokKarti?> GetStokKartiByIdAsync(int id)
        {
            return await _context.StokKartlari
                .Include(s => s.StokHareketleri)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<StokKarti> CreateStokKartiAsync(StokKarti stokKarti)
        {
            stokKarti.Id = 0; // Id veritabanında üretilir
            if (await _context.StokKartlari.AnyAsync(s => s.StokKodu == stokKarti.StokKodu))
            {
                throw new InvalidOperationException("Bu stok kodu zaten kayıtlı.");
            }

            _context.StokKartlari.Add(stokKarti);
            await _context.SaveChangesAsync();
            return stokKarti;
        }

        public async Task UpdateStokKartiAsync(StokKarti stokKarti)
        {
            var existingStokKarti = await _context.StokKartlari.FindAsync(stokKarti.Id);
            if (existingStokKarti == null)
            {
                throw new KeyNotFoundException("Stok kartı bulunamadı.");
            }

            if (await _context.StokKartlari.AnyAsync(s => s.StokKodu == stokKarti.StokKodu && s.Id != stokKarti.Id))
            {
                throw new InvalidOperationException("Bu stok kodu zaten kayıtlı.");
            }

            _context.Entry(existingStokKarti).CurrentValues.SetValues(stokKarti);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteStokKartiAsync(int id)
        {
            var stokKarti = await _context.StokKartlari.FindAsync(id);
            if (stokKarti == null)
            {
                throw new KeyNotFoundException("Stok kartı bulunamadı.");
            }

            var kayitliIslemVar = await _context.SatisFaturaSatirlari.AnyAsync(s => s.StokId == id)
                || await _context.AlisFaturaSatirlari.AnyAsync(s => s.StokId == id)
                || await _context.StokHareketler.AnyAsync(h => h.StokId == id);

            if (kayitliIslemVar)
            {
                stokKarti.Aktif = false;
            }
            else
            {
                _context.StokKartlari.Remove(stokKarti);
            }

            await _context.SaveChangesAsync();
            return !kayitliIslemVar;
        }
    }
}
