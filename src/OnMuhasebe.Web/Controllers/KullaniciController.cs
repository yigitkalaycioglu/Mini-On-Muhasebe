using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;

namespace OnMuhasebe.Web.Controllers
{
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
                this.UyariMesaji("Şifre en az 6 karakter olmalıdır.");
                return RedirectToAction("Edit", new { id });
            }

            if (yeniSifre != yeniSifreTekrar)
            {
                this.UyariMesaji("Şifreler eşleşmiyor.");
                return RedirectToAction("Edit", new { id });
            }

            await _kullaniciService.SifreSifirlaAsync(id, yeniSifre);
            this.BasariMesaji("Şifre sıfırlandı.");

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
                    this.BasariMesaji("Kullanıcı kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
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
                    await _kullaniciService.UpdateKullaniciAsync(kullanici, User.KullaniciId());
                    this.BasariMesaji("Kullanıcı güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            return View("Edit", kullanici);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var silindi = await _kullaniciService.DeleteKullaniciAsync(id, User.KullaniciId());
                if (silindi)
                {
                    this.BasariMesaji("Kullanıcı silindi.");
                }
                else
                {
                    this.UyariMesaji("Bu kullanıcının işlem kayıtları olduğu için silinemedi; bunun yerine pasife alındı.");
                }
            }
            catch (IsKuraliException ex)
            {
                // Kendini ya da son yöneticiyi silme girişimi
                this.UyariMesaji(ex.Message);
            }

            return RedirectToAction("Index");
        }
    }
}
