using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Infrastructure.Persistence;
using OnMuhasebe.Infrastructure.Security;

namespace OnMuhasebe.Infrastructure
{
    public static class DependencyInjection
    {
        public const string BaglantiCumlesiAdi = "DefaultConnection";

        /// <summary>
        /// Veritabanı ve parola özeti uygulamalarını kaydeder (Program.cs: builder.Services.AddInfrastructure()).
        /// Bağlantı cümlesi appsettings.json'daki ConnectionStrings:DefaultConnection'dan ya da
        /// ConnectionStrings__DefaultConnection ortam değişkeninden okunur.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>((servisler, options) =>
            {
                var baglanti = servisler.GetRequiredService<IConfiguration>().GetConnectionString(BaglantiCumlesiAdi)
                    ?? throw new InvalidOperationException($"'{BaglantiCumlesiAdi}' bağlantı cümlesi tanımlı değil (appsettings.json → ConnectionStrings).");
                options.UseSqlServer(baglanti);
            });
            services.AddScoped<IApplicationDbContext>(servisler => servisler.GetRequiredService<ApplicationDbContext>());

            services.AddSingleton<ISifreHashleyici, Pbkdf2SifreHashleyici>();

            return services;
        }
    }
}
