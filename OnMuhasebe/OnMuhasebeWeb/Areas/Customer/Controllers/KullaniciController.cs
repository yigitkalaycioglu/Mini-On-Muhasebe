using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class KullaniciController : Controller
    {
        private readonly IKullaniciService _kullaniciService;
        public KullaniciController(IKullaniciService kullaniciService)
        {
            _kullaniciService = kullaniciService;
        }

        public async Task<IActionResult> Index()
        {
            var kullanicilar = await _kullaniciService.GetAllKullanicilarAsync();
            return View(kullanicilar);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var kullanici = await _kullaniciService.GetKullaniciByIdAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }
            return View(kullanici);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var kullanici = await _kullaniciService.GetKullaniciByIdAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            await _kullaniciService.DeleteKullaniciAsync(id);

            if (!kullanici.Aktif)
            {
                TempData["Mesaj"] = "Bu kullanıcının işlem kayıtları olduğu için silinemedi; bunun yerine pasife alındı.";
                TempData["MesajTipi"] = "warning";
            }
            else
            {
                TempData["Mesaj"] = "Kullanıcı silindi.";
                TempData["MesajTipi"] = "success";
            }

            return RedirectToAction("Index");
        }
    }
}
