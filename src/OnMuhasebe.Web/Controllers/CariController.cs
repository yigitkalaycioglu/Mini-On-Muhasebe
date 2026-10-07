using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class CariController : Controller
    {
        private readonly ICariService _cariService;
        public CariController(ICariService cariService)
        {
            _cariService = cariService;
        }

        public async Task<IActionResult> Index()
        {
            // Bakiyeler hareketler belleğe alınmadan veritabanında hesaplanır.
            var bakiyeler = await _cariService.GetCariBakiyeleriAsync();

            var model = new CariListesiViewModel
            {
                Satirlar = bakiyeler.Select(b => new CariSatiriViewModel
                {
                    Cari = b.Cari,
                    TipAdi = _cariService.CariTipiAdi(b.Cari.CariTipi),
                    Bakiye = b.Bakiye
                }).ToList()
            };

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Ekstre(int? id, DateTime? baslangic, DateTime? bitis)
        {
            var model = new CariEkstreViewModel
            {
                Cariler = await _cariService.GetAllCarilerAsync()
            };

            if (!id.HasValue)
            {
                return View(model);
            }

            var cari = await _cariService.GetCariEkstresiAsync(id.Value, baslangic, bitis);
            if (cari == null)
            {
                return NotFound();
            }

            model.Cari = cari;
            model.DevirBakiye = await _cariService.GetDevirBakiyeAsync(id.Value, baslangic);

            // Yürüyen bakiye: devirden başlayıp her hareketin etkisi eklenerek ilerler.
            var yuruyen = model.DevirBakiye;
            foreach (var hareket in cari.CariHareketleri.OrderBy(h => h.Tarih).ThenBy(h => h.Id))
            {
                yuruyen += _cariService.HareketEtkisi(hareket);
                model.Satirlar.Add(new EkstreSatiriViewModel
                {
                    Hareket = hareket,
                    IslemAdi = _cariService.IslemTipiAdi(hareket.IslemTipi),
                    YuruyenBakiye = yuruyen
                });
            }

            model.ToplamBorc = model.Satirlar.Sum(x => x.Hareket.Borc);
            model.ToplamAlacak = model.Satirlar.Sum(x => x.Hareket.Alacak);
            model.SonBakiye = yuruyen;

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cari = await _cariService.GetCariByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }

            return View(DuzenlemeModeli(id, CariDto.FromEntity(cari), cari));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(CariDto cari)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _cariService.CreateCariAsync(cari);
                    this.BasariMesaji("Cari kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }
            return View("Create", cari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost([FromRoute] int id, CariDto cari)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _cariService.UpdateCariAsync(id, cari);
                    this.BasariMesaji("Cari güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            // Form girilen değerlerle yeniden gösterilir; hesap özeti kayıtlı cariden hesaplanır.
            var kayitli = await _cariService.GetCariByIdAsync(id);
            if (kayitli == null)
            {
                return NotFound();
            }
            return View("Edit", DuzenlemeModeli(id, cari, kayitli));
        }

        private CariDuzenleViewModel DuzenlemeModeli(int id, CariDto form, Cari kayitli) => new()
        {
            Id = id,
            Form = form,
            Ozet = new CariOzetiViewModel
            {
                ToplamBorc = _cariService.ToplamBorc(kayitli),
                ToplamAlacak = _cariService.ToplamAlacak(kayitli),
                Bakiye = _cariService.Bakiye(kayitli)
            },
            HareketSayisi = kayitli.CariHareketleri.Count
        };

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            var silindi = await _cariService.DeleteCariAsync(id);
            if (silindi)
            {
                this.BasariMesaji("Cari silindi.");
            }
            else
            {
                this.UyariMesaji("Bu carinin fatura veya hareket kayıtları olduğu için silinemedi; bunun yerine pasife alındı.");
            }

            return RedirectToAction("Index");
        }
    }
}
