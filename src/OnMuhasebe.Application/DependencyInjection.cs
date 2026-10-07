using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnMuhasebe.Application.Services;

namespace OnMuhasebe.Application
{
    public static class DependencyInjection
    {
        /// <summary>Uygulama katmanının servislerini kaydeder (Program.cs: builder.Services.AddApplication()).</summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Servisler istek başına oluşturulur (Scoped); aynı istekteki bütün servisler aynı DbContext'i paylaşır.
            services.AddScoped<ISatisElemaniService, SatisElemaniService>();
            services.AddScoped<ICariService, CariService>();
            services.AddScoped<IStokKartiService, StokKartiService>();
            services.AddScoped<IKullaniciService, KullaniciService>();
            services.AddScoped<ISatisFaturasiService, SatisFaturasiService>();
            services.AddScoped<IAlisFaturasiService, AlisFaturasiService>();
            services.AddScoped<IParametreService, ParametreService>();
            services.AddScoped<IStokHareketService, StokHareketService>();
            services.AddScoped<ITahsilatOdemeService, TahsilatOdemeService>();

            // Servisler "şimdi"yi doğrudan DateTime.Now'dan değil TimeProvider'dan alır; testlerde zaman sabitlenebilir.
            services.TryAddSingleton(TimeProvider.System);

            return services;
        }
    }
}
