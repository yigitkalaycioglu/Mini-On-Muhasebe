using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class SatisElemaniController : Controller
    {
        private readonly ISatisElemaniService _satisElemaniService;
        public SatisElemaniController(ISatisElemaniService satisElemaniService)
        {
            _satisElemaniService = satisElemaniService;
        }

        public async Task<IActionResult> Index()
        {
            var satisElemanlari = await _satisElemaniService.GetAllSatisElemanlariAsync();
            return View(satisElemanlari);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var satisElemani = await _satisElemaniService.GetSatisElemaniByIdAsync(id);
            if (satisElemani == null)
            {
                return NotFound();
            }
            return View(satisElemani);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(SatisElemani satisElemani)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _satisElemaniService.CreateSatisElemaniAsync(satisElemani);
                    TempData["Mesaj"] = "Satış elemanı kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("ad ve soyad"))
                    {
                        ModelState.AddModelError("AdSoyad", ex.Message);
                    }
                    else if (ex.Message.Contains("telefon"))
                    {
                        ModelState.AddModelError("Telefon", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }

            return View("Create", satisElemani);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var satisElemani = await _satisElemaniService.GetSatisElemaniByIdAsync(id);
            if (satisElemani == null)
            {
                return NotFound();
            }

            await _satisElemaniService.DeleteSatisElemaniAsync(id);

            if (!satisElemani.Aktif)
            {
                TempData["Mesaj"] = "Bu satış elemanının faturaları olduğu için silinemedi; bunun yerine pasife alındı.";
                TempData["MesajTipi"] = "warning";
            }
            else
            {
                TempData["Mesaj"] = "Satış elemanı silindi.";
                TempData["MesajTipi"] = "success";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, SatisElemani satisElemani)
        {
            if (ModelState.IsValid)
            {
                satisElemani.Id = id;

                try
                {
                    await _satisElemaniService.UpdateSatisElemaniAsync(satisElemani);
                    TempData["Mesaj"] = "Satış elemanı güncellendi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("ad ve soyad"))
                    {
                        ModelState.AddModelError("AdSoyad", ex.Message);
                    }
                    else if (ex.Message.Contains("telefon"))
                    {
                        ModelState.AddModelError("Telefon", ex.Message);
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

            return View("Edit", satisElemani);
        }
    }
}
