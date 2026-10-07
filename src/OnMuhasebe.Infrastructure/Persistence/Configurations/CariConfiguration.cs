using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class CariConfiguration : IEntityTypeConfiguration<Cari>
    {
        public void Configure(EntityTypeBuilder<Cari> builder)
        {
            // Benzersizlik serviste kontrol edilse de eşzamanlı iki kayıt için veritabanında da garanti altında.
            builder.HasIndex(c => c.CariKodu).IsUnique();
        }
    }
}
