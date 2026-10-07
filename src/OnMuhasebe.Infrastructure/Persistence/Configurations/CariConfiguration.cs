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

            // Kolon uzunlukları: dökümanın 7. bölümündeki tabloyla birebir.
            builder.Property(c => c.CariKodu).HasMaxLength(20);
            builder.Property(c => c.Unvan).HasMaxLength(150);
            builder.Property(c => c.VergiDairesi).HasMaxLength(50);
            builder.Property(c => c.VergiNo).HasMaxLength(20);
            builder.Property(c => c.Telefon).HasMaxLength(20);
            builder.Property(c => c.Adres).HasMaxLength(250);
        }
    }
}
