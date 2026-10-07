using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class StokKartiConfiguration : IEntityTypeConfiguration<StokKarti>
    {
        public void Configure(EntityTypeBuilder<StokKarti> builder)
        {
            builder.HasIndex(s => s.StokKodu).IsUnique();

            builder.Property(s => s.StokKodu).HasMaxLength(20);
            builder.Property(s => s.StokAdi).HasMaxLength(150);
            builder.Property(s => s.Birim).HasMaxLength(10);
            builder.Property(s => s.KdvOrani).HasPrecision(5, 2);
        }
    }
}
