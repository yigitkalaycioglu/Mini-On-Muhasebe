using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class SatisElemaniConfiguration : IEntityTypeConfiguration<SatisElemani>
    {
        public void Configure(EntityTypeBuilder<SatisElemani> builder)
        {
            builder.Property(e => e.AdSoyad).HasMaxLength(100);
            builder.Property(e => e.Telefon).HasMaxLength(20);
        }
    }
}
