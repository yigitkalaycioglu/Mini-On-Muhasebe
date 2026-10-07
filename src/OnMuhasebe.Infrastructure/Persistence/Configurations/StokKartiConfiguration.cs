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

            builder.Property(s => s.KdvOrani).HasPrecision(5, 2);
        }
    }
}
