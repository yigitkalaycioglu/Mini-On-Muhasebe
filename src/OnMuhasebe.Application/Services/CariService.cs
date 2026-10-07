using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Extensions;
using OnMuhasebe.Application.Models;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Services
{
    public class CariService : ICariService
    {
        private readonly IApplicationDbContext _context;
        public CariService(IApplicationDbContext context)
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

        /// <summary>
        /// Her carinin toplam borç, alacak ve bakiyesi. Hareketler belleğe alınmaz;
        /// toplamlar veritabanında hesaplanıp cariyle birlikte gelir (liste ve rapor ekranları için).
        /// </summary>
        public async Task<List<CariBakiyesi>> GetCariBakiyeleriAsync()
        {
            return await _context.Cariler
                .AsNoTracking()
                .OrderBy(c => c.CariKodu)
                .Select(c => new CariBakiyesi
                {
                    Cari = c,
                    Borc = c.CariHareketleri.Sum(h => h.Borc),
                    Alacak = c.CariHareketleri.Sum(h => h.Alacak),
                    Bakiye = c.CariHareketleri.AsQueryable().Sum(Etki)
                })
                .ToListAsync();
        }

        /// <summary>Cariler hareketleri olmadan (açılır listeler için). Bakiye gerekiyorsa GetCariBakiyeleriAsync kullanılır.</summary>
        public async Task<List<Cari>> GetAllCarilerAsync()
        {
            return await _context.Cariler
                .AsNoTracking()
                .OrderBy(c => c.CariKodu)
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
            var cari = await _context.Cariler.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (cari == null)
            {
                return null;
            }

            // Yalnızca seçilen dönemin hareketleri yüklenir; öncekiler devir bakiyesinde toplanır.
            cari.CariHareketleri = await _context.CariHareketler
                .AsNoTracking()
                .Where(h => h.CariId == id)
                .TarihAraliginda(h => h.Tarih, baslangic, bitis)
                .ToListAsync();
            return cari;
        }

        public async Task<decimal> GetDevirBakiyeAsync(int cariId, DateTime? baslangic)
        {
            if (!baslangic.HasValue)
            {
                return 0;
            }

            return await _context.CariHareketler
                .Where(h => h.CariId == cariId && h.Tarih < baslangic.Value.Date)
                .SumAsync(Etki);
        }

        public async Task<Cari> CreateCariAsync(CariDto cari)
        {
            if (await _context.Cariler.AnyAsync(c => c.CariKodu == cari.CariKodu))
            {
                throw new IsKuraliException(nameof(CariDto.CariKodu), "Bu cari kodu zaten kayıtlı.");
            }

            var yeni = new Cari();
            cari.ApplyTo(yeni);
            _context.Cariler.Add(yeni);
            await _context.SaveChangesAsync();
            return yeni;
        }

        public async Task<bool> DeleteCariAsync(int id)
        {
            var cari = await _context.Cariler.FindAsync(id);
            if (cari == null)
            {
                throw new KayitBulunamadiException("Cari bulunamadı.");
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
            return !kayitliIslemVar;
        }

        public async Task UpdateCariAsync(int id, CariDto cari)
        {
            var existingCari = await _context.Cariler.FindAsync(id);
            if (existingCari == null)
            {
                throw new KayitBulunamadiException("Cari bulunamadı.");
            }

            if (await _context.Cariler.AnyAsync(c => c.CariKodu == cari.CariKodu && c.Id != id))
            {
                throw new IsKuraliException(nameof(CariDto.CariKodu), "Bu cari kodu zaten kayıtlı.");
            }

            cari.ApplyTo(existingCari);
            await _context.SaveChangesAsync();
        }
    }
}
