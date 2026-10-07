using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class CariHareketConfiguration : IEntityTypeConfiguration<CariHareket>
    {
        public void Configure(EntityTypeBuilder<CariHareket> builder)
        {
            builder.HasOne(h => h.Cari)
                .WithMany(c => c.CariHareketleri)
                .HasForeignKey(h => h.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(h => h.Kullanici)
                .WithMany(k => k.CariHareketleri)
                .HasForeignKey(h => h.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fatura silinirken ilgili hareketler BelgeNo üzerinden bulunuyor.
            builder.HasIndex(h => h.BelgeNo);

            builder.Property(h => h.Tarih).HasColumnType("date");
            builder.Property(h => h.IslemTipi).HasMaxLength(20);
            builder.Property(h => h.BelgeNo).HasMaxLength(20);
        }
    }
}
