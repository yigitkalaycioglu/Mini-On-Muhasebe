using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class StokHareketConfiguration : IEntityTypeConfiguration<StokHareket>
    {
        public void Configure(EntityTypeBuilder<StokHareket> builder)
        {
            builder.HasOne(h => h.StokKarti)
                .WithMany(st => st.StokHareketleri)
                .HasForeignKey(h => h.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Kullanici)
                .WithMany(k => k.StokHareketleri)
                .HasForeignKey(h => h.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fatura silinirken ilgili hareketler BelgeNo üzerinden bulunuyor.
            builder.HasIndex(h => h.BelgeNo);

            builder.Property(h => h.Tarih).HasColumnType("date");
            builder.Property(h => h.HareketTipi).HasMaxLength(20);
            builder.Property(h => h.Yon).HasMaxLength(10);
            builder.Property(h => h.BelgeNo).HasMaxLength(20);
        }
    }
}
