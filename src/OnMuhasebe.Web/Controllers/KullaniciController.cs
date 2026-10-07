using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;
using OnMuhasebe.Web.ViewModels;

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
            return View(new KullaniciDuzenleViewModel { Id = id, Form = KullaniciDto.FromEntity(kullanici) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreSifirla(int id, string yeniSifre, string yeniSifreTekrar)
        {
            var sifreHatasi = SifreHatasi(yeniSifre, yeniSifreTekrar);
            if (sifreHatasi != null)
            {
                this.UyariMesaji(sifreHatasi);
                return RedirectToAction("Edit", new { id });
            }

            await _kullaniciService.SifreSifirlaAsync(id, yeniSifre);
            this.BasariMesaji("Şifre sıfırlandı.");

            return RedirectToAction("Edit", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(KullaniciOlusturDto kullanici)
        {
            var sifreHatasi = SifreHatasi(kullanici.Sifre, kullanici.SifreTekrar);
            if (sifreHatasi != null)
            {
                ModelState.AddModelError(string.Empty, sifreHatasi);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _kullaniciService.CreateKullaniciAsync(kullanici, kullanici.Sifre!);
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
        public async Task<IActionResult> EditPost(int id, KullaniciDto kullanici)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _kullaniciService.UpdateKullaniciAsync(id, kullanici, User.KullaniciId());
                    this.BasariMesaji("Kullanıcı güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            return View("Edit", new KullaniciDuzenleViewModel { Id = id, Form = kullanici });
        }

        // Yeni kullanıcıda ve şifre sıfırlamada aynı kural: en az 6 karakter, tekrarıyla aynı.
        private static string? SifreHatasi(string? sifre, string? tekrar)
        {
            if (string.IsNullOrEmpty(sifre) || sifre.Length < 6)
            {
                return "Şifre en az 6 karakter olmalıdır.";
            }
            return sifre != tekrar ? "Şifreler eşleşmiyor." : null;
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
