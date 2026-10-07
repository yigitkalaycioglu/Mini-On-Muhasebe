using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class ParametreConfiguration : IEntityTypeConfiguration<Parametre>
    {
        public void Configure(EntityTypeBuilder<Parametre> builder)
        {
            builder.HasIndex(p => p.ParametreKodu).IsUnique();
        }
    }
}
