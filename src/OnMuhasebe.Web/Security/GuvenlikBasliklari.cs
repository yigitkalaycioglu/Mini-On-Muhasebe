using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnMuhasebe.Web.Security
{
    /// <summary>
    /// Tarayıcıya yönelik güvenlik başlıkları.
    /// Content-Security-Policy her istekte rastgele bir "nonce" üretir: yalnızca bu değeri taşıyan satır içi
    /// script'ler çalışır. Sayfaya bir açık yoluyla sonradan eklenen bir script nonce'u bilemeyeceği için çalışmaz.
    /// </summary>
    public static class GuvenlikBasliklari
    {
        private const string NonceAnahtari = "CspNonce";

        public static IApplicationBuilder UseGuvenlikBasliklari(this IApplicationBuilder app, bool gelistirmeOrtami)
        {
            // dotnet watch'un sayfayı yenileme bağlantısı yalnızca geliştirme ortamında izinli
            var baglanti = gelistirmeOrtami ? "'self' ws://localhost:* wss://localhost:*" : "'self'";

            return app.Use(async (context, next) =>
            {
                // Onaltılık (hex) biçim: Base64'teki '+' gibi karakterler HTML'de kodlanmak zorunda kalmaz.
                var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
                context.Items[NonceAnahtari] = nonce;

                var basliklar = context.Response.Headers;
                basliklar["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    $"script-src 'self' 'nonce-{nonce}'; " +
                    "style-src 'self' 'unsafe-inline'; " +   // view'lerdeki style="..." nitelikleri için
                    "img-src 'self' data:; " +                 // Bootstrap'in CSS içine gömülü SVG simgeleri
                    "font-src 'self'; " +
                    $"connect-src {baglanti}; " +
                    "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
                basliklar["X-Frame-Options"] = "DENY";              // sayfa başka bir siteye gömülemez (tıklama kandırmacası)
                basliklar["X-Content-Type-Options"] = "nosniff";    // tarayıcı dosya türünü tahmin etmez
                basliklar["Referrer-Policy"] = "strict-origin-when-cross-origin";
                basliklar["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

                await next();
            });
        }

        /// <summary>View'deki satır içi script'ler için: &lt;script nonce="@Html.CspNonce()"&gt;</summary>
        public static string CspNonce(this IHtmlHelper html) =>
            html.ViewContext.HttpContext.Items[NonceAnahtari] as string ?? "";
    }
}
