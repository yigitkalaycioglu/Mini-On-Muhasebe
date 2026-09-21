using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class CariService : ICariService
    {
        private readonly ApplicationDbContext _context;
        public CariService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // Cari hesaplamaları
        // Bakiye hiçbir yerde saklanmaz, her zaman CariHareketler'den hesaplanır.
        // Hesabın tek tanımı burasıdır; controller'lar ViewModel kurarken buradan alır.
        // ---------------------------------------------------------------

        /// <summary>Tek hareketin bakiyeye etkisi: borç artı, alacak eksi. EF Core bunu SQL'e çevirir.</summary>
        public static readonly Expression<Func<CariHareket, decimal>> Etki =
            h => h.Borc - h.Alacak;

        private static readonly Func<CariHareket, decimal> EtkiFonksiyonu = Etki.Compile();

        /// <summary>Tek hareketin bakiyeye etkisi (ekstredeki yürüyen bakiye için).</summary>
        public decimal HareketEtkisi(CariHareket hareket) => EtkiFonksiyonu(hareket);

        /// <summary>
        /// Carinin bakiyesi. Pozitif: cari bize borçlu, negatif: biz cariye borçluyuz.
        /// CariHareketleri yüklenmemişse 0 döner.
        /// </summary>
        public decimal Bakiye(Cari cari) => cari.CariHareketleri.Sum(EtkiFonksiyonu);

        public decimal ToplamBorc(Cari cari) => cari.CariHareketleri.Sum(h => h.Borc);

        public decimal ToplamAlacak(Cari cari) => cari.CariHareketleri.Sum(h => h.Alacak);

        // Satış faturası müşteriye, alış faturası tedarikçiye kesilir.
        public bool MusteriMi(Cari cari) =>
            cari.CariTipi == Sabitler.CariTipiMusteri || cari.CariTipi == Sabitler.CariTipiHerIkisi;

        public bool TedarikciMi(Cari cari) =>
            cari.CariTipi == Sabitler.CariTipiTedarikci || cari.CariTipi == Sabitler.CariTipiHerIkisi;

        public string CariTipiAdi(byte cariTipi) => cariTipi switch
        {
            Sabitler.CariTipiMusteri => "Müşteri",
            Sabitler.CariTipiTedarikci => "Tedarikçi",
            Sabitler.CariTipiHerIkisi => "Müşteri + Tedarikçi",
            _ => "-"
        };

        public string IslemTipiAdi(string islemTipi) => islemTipi switch
        {
            Sabitler.IslemSatis => "Satış Faturası",
            Sabitler.IslemAlis => "Alış Faturası",
            Sabitler.IslemTahsilat => "Tahsilat",
            Sabitler.IslemOdeme => "Ödeme",
            _ => islemTipi
        };

        public async Task<List<Cari>> GetAllCarilerAsync()
        {
            return await _context.Cariler
                .Include(c => c.CariHareketleri)
                .ToListAsync();
        }

        public async Task<Cari?> GetCariByIdAsync(int id)
        {
            return await _context.Cariler
                .Include(c => c.CariHareketleri)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cari?> GetCariEkstresiAsync(int id, DateTime? baslangic, DateTime? bitis)
        {
            var bitisSonu = bitis?.Date.AddDays(1);

            return await _context.Cariler
                .Include(c => c.CariHareketleri
                    .Where(h => (!baslangic.HasValue || h.Tarih >= baslangic.Value)
                             && (!bitisSonu.HasValue || h.Tarih < bitisSonu.Value)))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<decimal> GetDevirBakiyeAsync(int cariId, DateTime? baslangic)
        {
            if (!baslangic.HasValue)
            {
                return 0;
            }

            return await _context.CariHareketler
                .Where(h => h.CariId == cariId && h.Tarih < baslangic.Value)
                .SumAsync(Etki);
        }

        public async Task<Cari> CreateCariAsync(Cari cari)
        {
            if (await _context.Cariler.AnyAsync(c => c.CariKodu == cari.CariKodu))
            {
                throw new InvalidOperationException("Bu cari kodu zaten kayıtlı.");
            }

            _context.Cariler.Add(cari);
            await _context.SaveChangesAsync();
            return cari;
        }

        public async Task DeleteCariAsync(int id)
        {
            var cari = await _context.Cariler.FindAsync(id);
            if (cari == null)
            {
                throw new KeyNotFoundException("Cari bulunamadı.");
            }

            var kayitliIslemVar = await _context.SatisFaturalari.AnyAsync(f => f.CariId == id)
                || await _context.AlisFaturalari.AnyAsync(f => f.CariId == id)
                || await _context.CariHareketler.AnyAsync(h => h.CariId == id);

            if (kayitliIslemVar)
            {
                cari.Aktif = false;
            }
            else
            {
                _context.Cariler.Remove(cari);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateCariAsync(Cari cari)
        {
            var existingCari = await _context.Cariler.FindAsync(cari.Id);
            if (existingCari == null)
            {
                throw new KeyNotFoundException("Cari bulunamadı.");
            }

            if (await _context.Cariler.AnyAsync(c => c.CariKodu == cari.CariKodu && c.Id != cari.Id))
            {
                throw new InvalidOperationException("Bu cari kodu zaten kayıtlı.");
            }

            _context.Entry(existingCari).CurrentValues.SetValues(cari);
            await _context.SaveChangesAsync();
        }
    }
}
