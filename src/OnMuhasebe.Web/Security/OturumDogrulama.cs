using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnMuhasebe.Application.Services;

namespace OnMuhasebe.Web.Security
{
    public static class OturumDogrulama
    {
        /// <summary>
        /// Her istekte oturum çerezindeki kullanıcıyı veritabanıyla karşılaştırır.
        /// Kullanıcı silinmiş, pasife alınmış ya da adı/rolü değişmişse oturum sonlandırılır;
        /// aksi halde eski çerez 8 saatlik kayan süre boyunca eski yetkilerle çalışmaya devam ederdi.
        /// </summary>
        public static async Task DogrulaAsync(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;
            if (principal == null || !int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var kullaniciId))
            {
                await OturumuKapatAsync(context);
                return;
            }

            var kullaniciService = context.HttpContext.RequestServices.GetRequiredService<IKullaniciService>();
            var kullanici = await kullaniciService.GetKullaniciByIdAsync(kullaniciId);

            var gecerli = kullanici != null
                && kullanici.Aktif
                && kullanici.KullaniciAdi == principal.FindFirstValue(ClaimTypes.Name)
                && kullanici.Rol == principal.FindFirstValue(ClaimTypes.Role);

            if (!gecerli)
            {
                await OturumuKapatAsync(context);
            }
        }

        private static async Task OturumuKapatAsync(CookieValidatePrincipalContext context)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
