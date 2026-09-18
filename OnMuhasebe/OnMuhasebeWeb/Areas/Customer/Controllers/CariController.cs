using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
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
            return View(cariler);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Ekstre(int? id, DateTime? baslangic, DateTime? bitis)
        {
            ViewData["Cariler"] = await _cariService.GetAllCarilerAsync();

            if (!id.HasValue)
            {
                return View((Cari?)null);
            }

            var cari = await _cariService.GetCariEkstresiAsync(id.Value, baslangic, bitis);
            if (cari == null)
            {
                return NotFound();
            }

            ViewData["DevirBakiye"] = await _cariService.GetDevirBakiyeAsync(id.Value, baslangic);
            return View(cari);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cari = await _cariService.GetCariByIdAsync(id);
            if (cari == null)
            {
                return NotFound();
            }
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
            return View("Edit", cari);
        }

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
