using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnMuhasebe.Web.Filters;
using OnMuhasebe.Web.Security;

namespace OnMuhasebe.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Sunum katmanının servisleri: MVC ve filtreleri, cookie kimlik doğrulama, giriş denemesi sınırları
        /// (Program.cs: builder.Services.AddWeb()).
        /// </summary>
        public static IServiceCollection AddWeb(this IServiceCollection services)
        {
            // Firma bilgisi ve para birimi her sayfada gerektiği için tek bir filtreyle ViewData'ya konur.
            services.AddControllersWithViews(options =>
            {
                options.Filters.Add<GorunumParametreleriFilter>();

                // Serviste bulunamayan kayıt her action'da ayrı ayrı yakalanmaz; burada 404'e çevrilir.
                options.Filters.Add<KayitBulunamadiFiltresi>();

                // Model bağlama hataları (ör. tutar alanına harf girilmesi) varsayılan olarak İngilizce gelir.
                var mesajlar = options.ModelBindingMessageProvider;
                mesajlar.SetValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
                mesajlar.SetAttemptedValueIsInvalidAccessor((deger, alan) => $"'{deger}' değeri {alan} için geçerli değil.");
                mesajlar.SetNonPropertyAttemptedValueIsInvalidAccessor(deger => $"'{deger}' geçerli bir değer değil.");
                mesajlar.SetUnknownValueIsInvalidAccessor(alan => $"{alan} için girilen değer geçerli değil.");
                mesajlar.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen değer geçerli değil.");
                mesajlar.SetValueMustNotBeNullAccessor(alan => $"{alan} alanı boş bırakılamaz.");
                mesajlar.SetValueMustBeANumberAccessor(alan => $"{alan} alanına sayı girilmelidir.");
                mesajlar.SetNonPropertyValueMustBeANumberAccessor(() => "Bu alana sayı girilmelidir.");
                mesajlar.SetMissingBindRequiredValueAccessor(alan => $"{alan} alanı zorunludur.");
                mesajlar.SetMissingKeyOrValueAccessor(() => "Değer zorunludur.");
                mesajlar.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;

                    // Pasife alınan ya da rolü değişen kullanıcının açık oturumu bir sonraki istekte kapanır.
                    options.Events.OnValidatePrincipal = OturumDogrulama.DogrulaAsync;
                });

            // Kaba kuvvet saldırısına karşı: kullanıcı adı başına hatalı deneme sayacı (bellekte)...
            services.AddMemoryCache();
            services.AddSingleton<GirisDenemeTakibi>();

            // ...ve IP başına giriş denemesi hız sınırı. Aşılırsa 429 döner, hata sayfası gösterilir.
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(GirisDenemeTakibi.HizSiniriPolitikasi, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "bilinmiyor",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = GirisDenemeTakibi.IpBasinaDakikadaDeneme,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
                options.OnRejected = (context, _) =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("OnMuhasebe.Web.Security");
                    logger.LogWarning("Giriş hız sınırı aşıldı: IP {Ip}", context.HttpContext.Connection.RemoteIpAddress);
                    return ValueTask.CompletedTask;
                };
            });

            return services;
        }
    }
}
