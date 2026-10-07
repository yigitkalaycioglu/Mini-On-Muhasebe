using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class SatisFaturasiConfiguration : IEntityTypeConfiguration<SatisFaturasi>
    {
        public void Configure(EntityTypeBuilder<SatisFaturasi> builder)
        {
            // Silme davranışı: cari, satış elemanı ve kullanıcı belgesi varken veritabanı seviyesinde de silinemez
            // (Restrict). Uygulama bu durumda kaydı pasife alır; böylece bir ana kayıt silinerek faturalar ve
            // hareketler, yani muhasebe geçmişi, kaybolamaz.
            builder.HasOne(f => f.Cari)
                .WithMany(c => c.SatisFaturalari)
                .HasForeignKey(f => f.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.SatisElemani)
                .WithMany(e => e.SatisFaturalari)
                .HasForeignKey(f => f.SatisElemaniId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Kullanici)
                .WithMany(k => k.SatisFaturalari)
                .HasForeignKey(f => f.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Numara serviste kontrol edilse de eşzamanlı iki kayıt için veritabanında da benzersiz.
            builder.HasIndex(f => f.FaturaNo).IsUnique();

            // Kolon tipleri: dökümanın 7. bölümündeki tabloyla birebir.
            builder.Property(f => f.FaturaNo).HasMaxLength(20);
            builder.Property(f => f.Tarih).HasColumnType("date");
            builder.Property(f => f.OlusturmaTarihi).HasColumnType("datetime");
        }
    }
}
