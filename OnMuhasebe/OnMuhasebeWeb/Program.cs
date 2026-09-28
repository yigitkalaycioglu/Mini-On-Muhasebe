using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Business.Services;
using OnMuhasebeWeb.Filters;
using OnMuhasebeWeb.Guvenlik;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Firma bilgisi ve para birimi her sayfada gerektiği için tek bir filtreyle ViewData'ya konur.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GorunumParametreleriFilter>();

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


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Area kullanıldığı için yollar "/Customer/..." ile başlamalı.
        options.LoginPath = "/Customer/Account/Login";
        options.LogoutPath = "/Customer/Account/Logout";
        options.AccessDeniedPath = "/Customer/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Pasife alınan ya da rolü değişen kullanıcının açık oturumu bir sonraki istekte kapanır.
        options.Events.OnValidatePrincipal = OturumDogrulama.DogrulaAsync;
    });

builder.Services.AddScoped<ISatisElemaniService, SatisElemaniService>();
builder.Services.AddScoped<ICariService, CariService>();
builder.Services.AddScoped<IStokKartiService, StokKartiService>();
builder.Services.AddScoped<IKullaniciService, KullaniciService>();
builder.Services.AddScoped<ISatisFaturasiService, SatisFaturasiService>();
builder.Services.AddScoped<IAlisFaturasiService, AlisFaturasiService>();
builder.Services.AddScoped<IParametreService, ParametreService>();
builder.Services.AddScoped<IStokHareketService, StokHareketService>();
builder.Services.AddScoped<ITahsilatOdemeService, TahsilatOdemeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Area kullanıldığı için hata sayfasının yolu "/Customer/..." ile başlar.
    app.UseExceptionHandler("/Customer/Home/Hata");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// 404 gibi durumlarda boş sayfa yerine uygulamanın hata sayfası gösterilir.
app.UseStatusCodePagesWithReExecute("/Customer/Home/Hata", "?kod={0}");

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

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
