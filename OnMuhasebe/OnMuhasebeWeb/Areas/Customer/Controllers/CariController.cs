using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;
using OnMuhasebeWeb.ViewModels;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            var cariler = await _cariService.GetAllCarilerAsync();

            var model = new CariListesiViewModel
            {
                Satirlar = cariler.Select(c => new CariSatiriViewModel
                {
                    Cari = c,
                    TipAdi = _cariService.CariTipiAdi(c.CariTipi),
                    Bakiye = _cariService.Bakiye(c)
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

            ViewData["Ozet"] = CariOzetiHazirla(cari);
            return View(cari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(Cari cari)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _cariService.CreateCariAsync(cari);
                    TempData["Mesaj"] = "Cari kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("cari kodu"))
                    {
                        ModelState.AddModelError("CariKodu", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }
            return View("Create", cari);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, Cari cari)
        {
            if (ModelState.IsValid)
            {
                cari.Id = id;

                try
                {
                    await _cariService.UpdateCariAsync(cari);
                    TempData["Mesaj"] = "Cari güncellendi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("cari kodu"))
                    {
                        ModelState.AddModelError("CariKodu", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }

            // Formdan gelen nesnede hareket listesi boştur; hesap özeti kayıtlı cariden hesaplanır.
            var kayitli = await _cariService.GetCariByIdAsync(id);
            ViewData["Ozet"] = kayitli == null ? new CariOzetiViewModel() : CariOzetiHazirla(kayitli);
            return View("Edit", cari);
        }

        private CariOzetiViewModel CariOzetiHazirla(Cari cari) => new()
        {
            ToplamBorc = _cariService.ToplamBorc(cari),
            ToplamAlacak = _cariService.ToplamAlacak(cari),
            Bakiye = _cariService.Bakiye(cari)
        };

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var cari = await _cariService.GetCariByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }

            await _cariService.DeleteCariAsync(id);

            if (!cari.Aktif)
            {
                TempData["Mesaj"] = "Bu carinin fatura veya hareket kayıtları olduğu için silinemedi; bunun yerine pasife alındı.";
                TempData["MesajTipi"] = "warning";
            }
            else
            {
                TempData["Mesaj"] = "Cari silindi.";
                TempData["MesajTipi"] = "success";
            }

            return RedirectToAction("Index");
        }
    }
}
