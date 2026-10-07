using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebeWeb.Guvenlik;
using System.Security.Claims;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IKullaniciService _kullaniciService;
        private readonly GirisDenemeTakibi _denemeTakibi;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IKullaniciService kullaniciService, GirisDenemeTakibi denemeTakibi, ILogger<AccountController> logger)
        {
            _kullaniciService = kullaniciService;
            _denemeTakibi = denemeTakibi;
            _logger = logger;
        }

        public IActionResult Login(string? returnUrl)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting(GirisDenemeTakibi.HizSiniriPolitikasi)]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı ve şifre zorunludur.");
                return View();
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var gunlukAdi = GunlukIcinTemizle(kullaniciAdi);

            // Kilitliyken şifre hiç denenmez; doğru şifre de kabul edilmez.
            var kalanKilit = _denemeTakibi.KalanKilitSuresi(kullaniciAdi);
            if (kalanKilit != null)
            {
                _logger.LogWarning("Kilitli kullanıcı adıyla giriş denemesi: {KullaniciAdi}, IP {Ip}", gunlukAdi, ip);
                ModelState.AddModelError(string.Empty, KilitMesaji(kalanKilit.Value));
                return View();
            }

            var kullanici = await _kullaniciService.DogrulaAsync(kullaniciAdi, sifre);

            if (kullanici == null)
            {
                var kilitlendi = _denemeTakibi.HataliDenemeKaydet(kullaniciAdi);
                _logger.LogWarning("Başarısız giriş denemesi: {KullaniciAdi}, IP {Ip}", gunlukAdi, ip);

                if (kilitlendi)
                {
                    _logger.LogWarning("{KullaniciAdi} için {Deneme} hatalı denemeden sonra giriş {Dakika} dakika kilitlendi, IP {Ip}",
                        gunlukAdi, GirisDenemeTakibi.EnFazlaHataliDeneme, GirisDenemeTakibi.KilitSuresi.TotalMinutes, ip);
                    ModelState.AddModelError(string.Empty, KilitMesaji(GirisDenemeTakibi.KilitSuresi));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                }
                return View();
            }

            _denemeTakibi.Sifirla(kullaniciAdi);
            _logger.LogInformation("Giriş yapıldı: {KullaniciAdi}, IP {Ip}", kullanici.KullaniciAdi, ip);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                new Claim(ClaimTypes.Name, kullanici.KullaniciAdi),
                new Claim(ClaimTypes.Role, kullanici.Rol),
            };

            var kimlik = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(kimlik));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private static string KilitMesaji(TimeSpan kalan) =>
            $"Çok fazla hatalı deneme yapıldı. {Math.Max(1, (int)Math.Ceiling(kalan.TotalMinutes))} dakika sonra tekrar deneyin.";

        // Formdan gelen metin günlüğe yazılmadan önce satır sonu gibi kontrol karakterlerinden arındırılır
        // (sahte günlük satırı eklenemesin) ve kısaltılır.
        private static string GunlukIcinTemizle(string metin) =>
            new string(metin.Where(c => !char.IsControl(c)).Take(50).ToArray());
    }
}
