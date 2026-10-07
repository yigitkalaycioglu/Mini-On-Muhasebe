using System.Security.Claims;

namespace OnMuhasebe.Web.Security
{
    public static class KullaniciBilgisi
    {
        /// <summary>
        /// Oturum açan kullanıcının Id'si (girişte NameIdentifier claim'ine yazılır).
        /// Belgelerde "işlemi yapan kullanıcı" olarak kaydedilir.
        /// </summary>
        public static int KullaniciId(this ClaimsPrincipal kullanici) =>
            int.Parse(kullanici.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
