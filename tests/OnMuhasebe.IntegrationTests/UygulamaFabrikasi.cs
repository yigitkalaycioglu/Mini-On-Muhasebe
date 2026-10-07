using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnMuhasebe.Infrastructure.Persistence;

namespace OnMuhasebe.IntegrationTests
{
    /// <summary>
    /// Uygulamayı bellekte çalıştırır. Her test çalıştırması için ayrı bir veritabanı (OnMuhasebeTest_...) kurulur,
    /// migration'lar ve database/seed_data.sql uygulanır, testler bitince veritabanı silinir.
    /// </summary>
    public sealed class UygulamaFabrikasi : WebApplicationFactory<Program>, IAsyncLifetime
    {
        /// <summary>
        /// Veritabanı sunucusunun bağlantı cümlesi, Database= kısmı olmadan.
        /// Örn: Server=localhost\MSSQLSERVER01;Trusted_Connection=True;TrustServerCertificate=True
        /// </summary>
        public const string OrtamDegiskeni = "ONMUHASEBE_TEST_SQL";

        public static string? SunucuBaglantisi => Environment.GetEnvironmentVariable(OrtamDegiskeni);

        /// <summary>Ortam değişkeni yoksa çağıran testi atlar.</summary>
        public static void VeritabaniGerekli() =>
            Assert.SkipWhen(string.IsNullOrWhiteSpace(SunucuBaglantisi), $"{OrtamDegiskeni} tanımlı değil; entegrasyon testi atlandı.");

        private readonly string _veritabani = $"OnMuhasebeTest_{Guid.NewGuid():N}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection", $"{SunucuBaglantisi};Database={_veritabani}");

            // Testler art arda çok sayıda giriş yapar; IP başına hız sınırı burada engel olmasın.
            builder.UseSetting("GirisGuvenligi:IpBasinaDakikadaDeneme", "1000");

            // Çalıştırılan her SQL komutu yazılmasın; test çıktısında yalnızca uyarı ve hatalar kalsın.
            builder.UseSetting("Logging:LogLevel:Default", "Warning");
        }

        public async ValueTask InitializeAsync()
        {
            if (string.IsNullOrWhiteSpace(SunucuBaglantisi))
            {
                return;
            }

            using var scope = Services.CreateScope();
            var db = TestVeritabani(scope);
            await db.Database.MigrateAsync();
            await OrnekVeriyiYukleAsync(db);
        }

        public override async ValueTask DisposeAsync()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SunucuBaglantisi))
                {
                    using var scope = Services.CreateScope();
                    await TestVeritabani(scope).Database.EnsureDeletedAsync();
                }
            }
            finally
            {
                await base.DisposeAsync();
            }
        }

        // Testler veri yükler ve sonunda veritabanını siler. Uygulama bir sebeple bu çalıştırmanın veritabanı yerine
        // başka birine (ör. appsettings.json'daki asıl veritabanına) bağlanıyorsa hiçbir işlem yapmadan durulur.
        private ApplicationDbContext TestVeritabani(IServiceScope scope)
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var baglanilan = db.Database.GetDbConnection().Database;
            if (baglanilan != _veritabani)
            {
                throw new InvalidOperationException($"Testler {_veritabani} yerine {baglanilan} veritabanına bağlanıyor; durduruldu.");
            }
            return db;
        }

        // seed_data.sql'deki GO ile ayrılmış parçalar sırayla çalıştırılır (USE satırı bu veritabanı için atlanır).
        private static async Task OrnekVeriyiYukleAsync(ApplicationDbContext db)
        {
            var betik = await File.ReadAllTextAsync(Path.Combine(DepoKoku(), "database", "seed_data.sql"));
            var baglanti = db.Database.GetDbConnection();
            await baglanti.OpenAsync();
            try
            {
                foreach (var parca in Regex.Split(betik, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
                {
                    var sql = Regex.Replace(parca, @"^\s*USE\s+\S+\s*$", "", RegexOptions.Multiline | RegexOptions.IgnoreCase).Trim();
                    if (sql.Length == 0)
                    {
                        continue;
                    }

                    await using var komut = baglanti.CreateCommand();
                    komut.CommandText = sql;
                    await komut.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                await baglanti.CloseAsync();
            }
        }

        private static string DepoKoku()
        {
            var klasor = new DirectoryInfo(AppContext.BaseDirectory);
            while (klasor != null && !File.Exists(Path.Combine(klasor.FullName, "OnMuhasebe.slnx")))
            {
                klasor = klasor.Parent;
            }
            return klasor?.FullName ?? throw new InvalidOperationException("Depo kökü (OnMuhasebe.slnx) bulunamadı.");
        }
    }

    /// <summary>Veritabanı kullanan bütün test sınıfları aynı uygulamayı paylaşır ve sırayla çalışır.</summary>
    [CollectionDefinition(Ad)]
    public class VeritabaniKoleksiyonu : ICollectionFixture<UygulamaFabrikasi>
    {
        public const string Ad = "Veritabani";
    }
}
