using System.Globalization;
using Microsoft.AspNetCore.Localization;
using OnMuhasebe.Application;
using OnMuhasebe.Infrastructure;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;

var builder = WebApplication.CreateBuilder(args);

// Yanıtlarda sunucu yazılımının adı ("Server: Kestrel") gönderilmez.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Katmanlar: Application (iş kuralları) → Infrastructure (veritabanı, parola özeti) → Web (MVC, kimlik doğrulama)
builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddWeb();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Hata");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 404 gibi durumlarda boş sayfa yerine uygulamanın hata sayfası gösterilir.
app.UseStatusCodePagesWithReExecute("/Home/Hata", "?kod={0}");

// Content-Security-Policy, X-Frame-Options, X-Content-Type-Options, Referrer-Policy...
app.UseGuvenlikBasliklari(app.Environment.IsDevelopment());

app.UseHttpsRedirection();

// Tutarlar virgüllü girilir ("12,50"). Kültür sunucunun diline bırakılırsa İngilizce bir sunucuda
// "12,50" → 1250 olarak kaydedilir; bu yüzden uygulama kültürü tr-TR olarak sabitlenir.
var turkce = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(turkce),
    SupportedCultures = new[] { turkce },
    SupportedUICultures = new[] { turkce }
});

app.UseRouting();

// Hız sınırı politikası action'a [EnableRateLimiting] ile bağlandığı için UseRouting'den sonra gelir.
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

// Entegrasyon testlerindeki WebApplicationFactory<Program> için
public partial class Program;
