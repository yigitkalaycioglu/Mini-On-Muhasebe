using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Business.Services;
using OnMuhasebeWeb.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Firma bilgisi ve para birimi her sayfada gerektiği için tek bir filtreyle ViewData'ya konur.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GorunumParametreleriFilter>();
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
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
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
