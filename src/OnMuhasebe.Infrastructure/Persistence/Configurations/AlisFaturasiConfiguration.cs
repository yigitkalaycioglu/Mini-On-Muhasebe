using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class AlisFaturasiConfiguration : IEntityTypeConfiguration<AlisFaturasi>
    {
        public void Configure(EntityTypeBuilder<AlisFaturasi> builder)
        {
            // Cari ve kullanıcı belgesi varken silinemez (Restrict); muhasebe geçmişi korunur.
            builder.HasOne(f => f.Cari)
                .WithMany(c => c.AlisFaturalari)
                .HasForeignKey(f => f.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Kullanici)
                .WithMany(k => k.AlisFaturalari)
                .HasForeignKey(f => f.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(f => f.FaturaNo).IsUnique();

            builder.Property(f => f.FaturaNo).HasMaxLength(20);
            builder.Property(f => f.Tarih).HasColumnType("date");
            builder.Property(f => f.OlusturmaTarihi).HasColumnType("datetime");
            builder.Property(f => f.Aciklama).HasMaxLength(250);
        }
    }
}
