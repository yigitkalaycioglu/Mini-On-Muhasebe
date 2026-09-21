using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = Sabitler.RolYonetici)]
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
        public async Task<IActionResult> SifreSifirla(int id, string yeniSifre, string yeniSifreTekrar)
        {
            if (string.IsNullOrEmpty(yeniSifre) || yeniSifre.Length < 6)
            {
                TempData["Mesaj"] = "Şifre en az 6 karakter olmalıdır.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Edit", new { id });
            }

            if (yeniSifre != yeniSifreTekrar)
            {
                TempData["Mesaj"] = "Şifreler eşleşmiyor.";
                TempData["MesajTipi"] = "warning";
                return RedirectToAction("Edit", new { id });
            }

            try
            {
                await _kullaniciService.SifreSifirlaAsync(id, yeniSifre);
                TempData["Mesaj"] = "Şifre sıfırlandı.";
                TempData["MesajTipi"] = "success";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(Kullanici kullanici, string sifre, string sifreTekrar)
        {
            ModelState.Remove("SifreHash");

            if (string.IsNullOrEmpty(sifre) || sifre.Length < 6)
            {
                ModelState.AddModelError(string.Empty, "Şifre en az 6 karakter olmalıdır.");
            }
            else if (sifre != sifreTekrar)
            {
                ModelState.AddModelError(string.Empty, "Şifreler eşleşmiyor.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _kullaniciService.CreateKullaniciAsync(kullanici, sifre);
                    TempData["Mesaj"] = "Kullanıcı kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("kullanıcı adı"))
                    {
                        ModelState.AddModelError("KullaniciAdi", ex.Message);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                    }
                }
            }

            return View("Create", kullanici);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, Kullanici kullanici)
        {
            ModelState.Remove("SifreHash");
            kullanici.Id = id;

            if (ModelState.IsValid)
            {
                try
                {
                    await _kullaniciService.UpdateKullaniciAsync(kullanici);
                    TempData["Mesaj"] = "Kullanıcı güncellendi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("kullanıcı adı"))
                    {
                        ModelState.AddModelError("KullaniciAdi", ex.Message);
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

            return View("Edit", kullanici);
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
