using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Application.Abstractions
{
    /// <summary>
    /// Servislerin veritabanına eriştiği arayüz. Infrastructure katmanındaki ApplicationDbContext bunu uygular;
    /// böylece uygulama katmanı somut veritabanı sınıfına ve SQL Server'a bağımlı olmaz (bağımlılığın tersine çevrilmesi).
    /// </summary>
    public interface IApplicationDbContext
    {
        DbSet<Kullanici> Kullanicilar { get; }
        DbSet<SatisElemani> SatisElemanlari { get; }
        DbSet<Cari> Cariler { get; }
        DbSet<StokKarti> StokKartlari { get; }
        DbSet<SatisFaturasi> SatisFaturalari { get; }
        DbSet<SatisFaturaSatiri> SatisFaturaSatirlari { get; }
        DbSet<AlisFaturasi> AlisFaturalari { get; }
        DbSet<AlisFaturaSatiri> AlisFaturaSatirlari { get; }
        DbSet<StokHareket> StokHareketler { get; }
        DbSet<CariHareket> CariHareketler { get; }
        DbSet<Parametre> Parametreler { get; }

        /// <summary>Bekleyen bütün değişiklikleri tek bir veritabanı işleminde (transaction) kaydeder.</summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
