using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class AlisFaturaSatiriConfiguration : IEntityTypeConfiguration<AlisFaturaSatiri>
    {
        public void Configure(EntityTypeBuilder<AlisFaturaSatiri> builder)
        {
            // Fatura silinince kalemleri de silinir (Cascade); stok kartına ise StokId kolonuyla bağlanır.
            builder.HasOne(a => a.AlisFaturasi)
                .WithMany(af => af.AlisFaturaSatirlari)
                .HasForeignKey(a => a.AlisFaturaId);

            builder.HasOne(a => a.StokKarti)
                .WithMany(st => st.AlisFaturaSatirlari)
                .HasForeignKey(a => a.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.KdvOrani).HasPrecision(5, 2);
        }
    }
}
