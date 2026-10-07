using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Extensions;
using OnMuhasebe.Application.Models;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Domain.Rules;

namespace OnMuhasebe.Application.Services
{
    public class SatisFaturasiService : ISatisFaturasiService
    {
        private readonly IApplicationDbContext _context;
        private readonly IParametreService _parametreService;
        private readonly IStokKartiService _stokKartiService;
        private readonly ICariService _cariService;
        private readonly TimeProvider _zaman;
        public SatisFaturasiService(IApplicationDbContext context, IParametreService parametreService, IStokKartiService stokKartiService, ICariService cariService, TimeProvider zaman)
        {
            _context = context;
            _parametreService = parametreService;
            _stokKartiService = stokKartiService;
            _cariService = cariService;
            _zaman = zaman;
        }

        public async Task<SatisFaturasi> CreateSatisFaturasiAsync(SatisFaturasiDto fatura, int kullaniciId)
        {
            ArgumentNullException.ThrowIfNull(fatura);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(kullaniciId);

            var satisFaturasi = fatura.ToEntity();
            if (satisFaturasi.SatisFaturaSatirlari.Count == 0)
            {
                throw new IsKuraliException("Faturaya en az bir kalem ekleyin.");
            }

            // Form ekranda süzülse de istek elle değiştirilebilir; seçimler sunucuda da doğrulanır.
            var cari = await _context.Cariler.FindAsync(satisFaturasi.CariId);
            if (cari == null || !cari.Aktif || !_cariService.MusteriMi(cari))
            {
                throw new IsKuraliException("Geçerli ve aktif bir müşteri seçilmelidir.");
            }

            var satisElemani = await _context.SatisElemanlari.FindAsync(satisFaturasi.SatisElemaniId);
            if (satisElemani == null || !satisElemani.Aktif)
            {
                throw new IsKuraliException("Geçerli ve aktif bir satış elemanı seçilmelidir.");
            }

            var stokIdler = satisFaturasi.SatisFaturaSatirlari.Select(k => k.StokId).Distinct().ToList();
            var stokAdlari = await _context.StokKartlari
                .Where(s => stokIdler.Contains(s.Id) && s.Aktif)
                .ToDictionaryAsync(s => s.Id, s => s.StokAdi);
            if (stokAdlari.Count != stokIdler.Count)
            {
                throw new IsKuraliException("Faturadaki ürünlerden biri bulunamadı ya da pasif.");
            }

            var faturaNo = await GetYeniFaturaNoAsync();
            satisFaturasi.FaturaNo = faturaNo;
            satisFaturasi.KullaniciId = kullaniciId;
            satisFaturasi.OlusturmaTarihi = _zaman.GetLocalNow().DateTime;
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
                        throw new IsKuraliException($"{stokAdlari[grup.Key]} için yeterli stok yok. Mevcut: {mevcutMiktar:N2}, istenen: {istenenToplam:N2}");
                    }
                }
            }

            // Tutarlar "Ondalık Basamak" parametresine göre ticari usulde yuvarlanır (TutarHesabi).
            var basamak = await _parametreService.GetOndalikBasamakAsync();

            foreach (var kalem in satisFaturasi.SatisFaturaSatirlari)
            {
                kalem.SatirTutari = TutarHesabi.SatirTutari(kalem.Miktar, kalem.BirimFiyat, basamak);
                satisFaturasi.AraToplam += kalem.SatirTutari;
                satisFaturasi.KdvToplam += TutarHesabi.Kdv(kalem.SatirTutari, kalem.KdvOrani, basamak);

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
                    throw new IsKuraliException($"{faturaNo} numarası bu sırada başka bir faturaya verildi. Faturayı tekrar kaydedin.");
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
                throw new KayitBulunamadiException("Satış faturası bulunamadı.");
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

            query = query.TarihAraliginda(f => f.Tarih, baslangic, bitis);

            return await query.OrderByDescending(f => f.Tarih).ThenByDescending(f => f.Id).ToListAsync();
        }

        /// <summary>Satış elemanı bazında fatura sayısı ve toplamlar; gruplama veritabanında yapılır.</summary>
        public async Task<List<SatisElemaniCirosu>> GetSatisElemaniCirolariAsync(DateTime? baslangic, DateTime? bitis)
        {
            var query = _context.SatisFaturalari.AsQueryable();
            query = query.TarihAraliginda(f => f.Tarih, baslangic, bitis);

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
