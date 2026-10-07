using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    // Tüm raporlar hareket tablolarından hesaplanır; hiçbir rapor ayrı bir toplam tablosu tutmaz.
    // Tahsilat/ödeme, stok hareketleri ve fatura listeleri kendi ekranlarında filtrelenerek raporlanır.
    [Authorize]
    public class RaporController : Controller
    {
        private readonly ICariService _cariService;
        private readonly IStokKartiService _stokKartiService;
        private readonly ISatisFaturasiService _satisFaturasiService;
        public RaporController(ICariService cariService, IStokKartiService stokKartiService, ISatisFaturasiService satisFaturasiService)
        {
            _cariService = cariService;
            _stokKartiService = stokKartiService;
            _satisFaturasiService = satisFaturasiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CariBakiye()
        {
            var bakiyeler = await _cariService.GetCariBakiyeleriAsync();

            var model = new CariBakiyeRaporViewModel
            {
                Satirlar = bakiyeler.Select(b => new CariBakiyeSatiriViewModel
                {
                    Cari = b.Cari,
                    TipAdi = _cariService.CariTipiAdi(b.Cari.CariTipi),
                    Borc = b.Borc,
                    Alacak = b.Alacak,
                    Bakiye = b.Bakiye
                }).ToList()
            };

            // Pozitif bakiye: cari bize borçlu (alacağımız). Negatif bakiye: biz cariye borçluyuz.
            model.ToplamBorc = model.Satirlar.Sum(x => x.Borc);
            model.ToplamAlacak = model.Satirlar.Sum(x => x.Alacak);
            model.Alacagimiz = model.Satirlar.Where(x => x.Bakiye > 0).Sum(x => x.Bakiye);
            model.Borcumuz = model.Satirlar.Where(x => x.Bakiye < 0).Sum(x => -x.Bakiye);
            model.Net = model.Alacagimiz - model.Borcumuz;

            return View(model);
        }

        public async Task<IActionResult> StokDurum()
        {
            var bakiyeler = await _stokKartiService.GetStokBakiyeleriAsync();

            var model = new StokDurumRaporViewModel
            {
                Satirlar = bakiyeler.Select(b => new StokDurumSatiriViewModel
                {
                    Stok = b.Stok,
                    Giris = b.Giris,
                    Cikis = b.Cikis,
                    Mevcut = b.Mevcut,
                    // Stok değeri alış fiyatı üzerinden; eksi stok değer taşımaz.
                    Deger = Math.Max(b.Mevcut, 0) * b.Stok.AlisFiyati,
                    Seviye = b.Mevcut <= 0 ? "tukendi"
                        : _stokKartiService.KritikSeviyede(b) ? "kritik"
                        : "yeterli"
                }).ToList()
            };
            model.KritikSayisi = model.Satirlar.Count(x => x.Seviye == "kritik");
            model.TukenenSayisi = model.Satirlar.Count(x => x.Seviye == "tukendi");
            model.ToplamDeger = model.Satirlar.Sum(x => x.Deger);

            return View(model);
        }

        public async Task<IActionResult> KritikStok()
        {
            var bakiyeler = await _stokKartiService.GetStokBakiyeleriAsync(yalnizcaAktif: true);

            var model = new KritikStokRaporViewModel
            {
                Satirlar = bakiyeler
                    .Where(_stokKartiService.KritikSeviyede)
                    .Select(b =>
                    {
                        // Eksik: kritik seviyeye ulaşmak için gereken miktar; maliyet alış fiyatıyla hesaplanır.
                        var eksik = Math.Max(b.Stok.KritikStok - b.Mevcut, 0);
                        return new KritikStokSatiriViewModel
                        {
                            Stok = b.Stok,
                            Mevcut = b.Mevcut,
                            Eksik = eksik,
                            Maliyet = eksik * b.Stok.AlisFiyati
                        };
                    })
                    .OrderBy(x => x.Mevcut > 0)
                    .ThenByDescending(x => x.Eksik)
                    .ToList()
            };
            model.TukenenSayisi = model.Satirlar.Count(x => x.Mevcut <= 0);
            model.ToplamMaliyet = model.Satirlar.Sum(x => x.Maliyet);

            return View(model);
        }

        public async Task<IActionResult> SatisElemaniSatis(DateTime? baslangic, DateTime? bitis)
        {
            var cirolar = await _satisFaturasiService.GetSatisElemaniCirolariAsync(baslangic, bitis);
            var toplamGenel = cirolar.Sum(c => c.GenelToplam);

            var model = new SatisElemaniSatisRaporViewModel
            {
                Satirlar = cirolar
                    .Select(c => new SatisElemaniSatisSatiriViewModel
                    {
                        Eleman = c.Eleman,
                        FaturaSayisi = c.FaturaSayisi,
                        AraToplam = c.AraToplam,
                        KdvToplam = c.KdvToplam,
                        GenelToplam = c.GenelToplam,
                        // Pay: elemanın cirosunun toplam ciro içindeki yüzdesi
                        Pay = toplamGenel > 0 ? c.GenelToplam / toplamGenel * 100 : 0
                    })
                    .OrderByDescending(x => x.GenelToplam)
                    .ToList(),
                ToplamFaturaSayisi = cirolar.Sum(c => c.FaturaSayisi),
                ToplamAra = cirolar.Sum(c => c.AraToplam),
                ToplamKdv = cirolar.Sum(c => c.KdvToplam),
                ToplamGenel = toplamGenel
            };

            return View(model);
        }
    }
}
