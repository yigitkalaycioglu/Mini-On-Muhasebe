using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business;
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
        private readonly IParametreService _parametreService;
        public StokKartiController(IStokKartiService stokKartiService, IParametreService parametreService)
        {
            _stokKartiService = stokKartiService;
            _parametreService = parametreService;
        }

        public async Task<IActionResult> Index()
        {
            var stokKartlari = await _stokKartiService.GetAllStokKartlariAsync();

            // "Kritik Stok Uyarısı" parametresi kapalıysa uyarı ve etiketler gösterilmez.
            var uyariAcik = await _parametreService.AcikMiAsync(Sabitler.ParamKritikStokUyarisi);

            var model = new StokListesiViewModel
            {
                Satirlar = stokKartlari.Select(s => new StokSatiriViewModel
                {
                    Stok = s,
                    Mevcut = _stokKartiService.MevcutMiktar(s),
                    Kritik = uyariAcik && _stokKartiService.KritikSeviyede(s)
                }).ToList()
            };
            model.KritikSayisi = model.Satirlar.Count(x => x.Stok.Aktif && x.Kritik);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            // Yeni kartta KDV oranı "Varsayılan KDV Oranı" parametresinden gelir.
            var kdv = await _parametreService.GetSayiAsync(Sabitler.ParamVarsayilanKdvOrani);
            ViewData["VarsayilanKdv"] = kdv.ToString("0.##");
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var stokKarti = await _stokKartiService.GetStokKartiByIdAsync(id);
            if (stokKarti == null)
            {
                return NotFound();
            }

            ViewData["Ozet"] = await StokOzetiHazirlaAsync(stokKarti);
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
            ViewData["Ozet"] = kayitli == null ? new StokOzetiViewModel() : await StokOzetiHazirlaAsync(kayitli);
            return View("Edit", stokKarti);
        }

        private async Task<StokOzetiViewModel> StokOzetiHazirlaAsync(StokKarti stokKarti) => new()
        {
            ToplamGiris = _stokKartiService.ToplamGiris(stokKarti),
            ToplamCikis = _stokKartiService.ToplamCikis(stokKarti),
            Mevcut = _stokKartiService.MevcutMiktar(stokKarti),
            Kritik = await _parametreService.AcikMiAsync(Sabitler.ParamKritikStokUyarisi)
                && _stokKartiService.KritikSeviyede(stokKarti)
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
