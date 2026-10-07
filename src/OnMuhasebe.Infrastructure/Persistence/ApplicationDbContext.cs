using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<SatisElemani> SatisElemanlari { get; set; }
        public DbSet<Cari> Cariler { get; set; }
        public DbSet<StokKarti> StokKartlari { get; set; }
        public DbSet<SatisFaturasi> SatisFaturalari { get; set; }
        public DbSet<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; }
        public DbSet<AlisFaturasi> AlisFaturalari { get; set; }
        public DbSet<AlisFaturaSatiri> AlisFaturaSatirlari { get; set; }
        public DbSet<StokHareket> StokHareketler { get; set; }
        public DbSet<CariHareket> CariHareketler { get; set; }
        public DbSet<Parametre> Parametreler { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Dökümandaki tutar ve miktar alanlarının hepsi decimal(18,2).
            // KDV oranları decimal(5,2) olduğu için ilgili yapılandırma sınıflarında ayrıca belirtilir.
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Her tablonun ilişkileri, indeksleri ve kolon tipleri Configurations klasöründeki
            // kendi IEntityTypeConfiguration sınıfında tanımlıdır.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
