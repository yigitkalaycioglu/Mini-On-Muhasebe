using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            return View(stokKartlari);
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
            return View("Edit", stokKarti);
        }

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
