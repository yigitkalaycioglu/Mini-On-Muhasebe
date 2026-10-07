using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class KullaniciConfiguration : IEntityTypeConfiguration<Kullanici>
    {
        public void Configure(EntityTypeBuilder<Kullanici> builder)
        {
            builder.HasIndex(k => k.KullaniciAdi).IsUnique();

            builder.Property(k => k.SifreHash).HasMaxLength(256);
            builder.Property(k => k.Rol).HasMaxLength(20);
        }
    }
}
