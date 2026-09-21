using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;
using OnMuhasebeWeb.ViewModels;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class StokKartiController : Controller
    {
        private readonly IStokKartiService _stokKartiService;
        public StokKartiController(IStokKartiService stokKartiService)
        {
            _stokKartiService = stokKartiService;
        }

        public async Task<IActionResult> Index()
        {
            var stokKartlari = await _stokKartiService.GetAllStokKartlariAsync();

            var model = new StokListesiViewModel
            {
                Satirlar = stokKartlari.Select(s => new StokSatiriViewModel
                {
                    Stok = s,
                    Mevcut = _stokKartiService.MevcutMiktar(s),
                    Kritik = _stokKartiService.KritikSeviyede(s)
                }).ToList()
            };
            model.KritikSayisi = model.Satirlar.Count(x => x.Stok.Aktif && x.Kritik);

            return View(model);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var stokKarti = await _stokKartiService.GetStokKartiByIdAsync(id);
            if (stokKarti == null)
            {
                return NotFound();
            }

            ViewData["Ozet"] = StokOzetiHazirla(stokKarti);
            return View(stokKarti);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(StokKarti stokKarti)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _stokKartiService.CreateStokKartiAsync(stokKarti);
                    TempData["Mesaj"] = "Stok kartı kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("stok kodu"))
                    {
                        ModelState.AddModelError("StokKodu", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }
            return View("Create", stokKarti);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, StokKarti stokKarti)
        {
            if (ModelState.IsValid)
            {
                stokKarti.Id = id;

                try
                {
                    await _stokKartiService.UpdateStokKartiAsync(stokKarti);
                    TempData["Mesaj"] = "Stok kartı güncellendi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("stok kodu"))
                    {
                        ModelState.AddModelError("StokKodu", ex.Message);
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

            // Formdan gelen nesnede hareket listesi boştur; stok özeti kayıtlı karttan hesaplanır.
            var kayitli = await _stokKartiService.GetStokKartiByIdAsync(id);
            ViewData["Ozet"] = kayitli == null ? new StokOzetiViewModel() : StokOzetiHazirla(kayitli);
            return View("Edit", stokKarti);
        }

        private StokOzetiViewModel StokOzetiHazirla(StokKarti stokKarti) => new()
        {
            ToplamGiris = _stokKartiService.ToplamGiris(stokKarti),
            ToplamCikis = _stokKartiService.ToplamCikis(stokKarti),
            Mevcut = _stokKartiService.MevcutMiktar(stokKarti),
            Kritik = _stokKartiService.KritikSeviyede(stokKarti)
        };

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var stokKarti = await _stokKartiService.GetStokKartiByIdAsync(id);
            if (stokKarti == null)
            {
                return NotFound();
            }

            await _stokKartiService.DeleteStokKartiAsync(id);

            if (!stokKarti.Aktif)
            {
                TempData["Mesaj"] = "Bu stok kartının fatura veya hareket kayıtları olduğu için silinemedi; bunun yerine pasife alındı.";
                TempData["MesajTipi"] = "warning";
            }
            else
            {
                TempData["Mesaj"] = "Stok kartı silindi.";
                TempData["MesajTipi"] = "success";
            }

            return RedirectToAction("Index");
        }
    }
}
