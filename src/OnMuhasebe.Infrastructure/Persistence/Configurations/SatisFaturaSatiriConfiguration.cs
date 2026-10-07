using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnMuhasebe.Domain.Entities;

namespace OnMuhasebe.Infrastructure.Persistence.Configurations
{
    public class SatisFaturaSatiriConfiguration : IEntityTypeConfiguration<SatisFaturaSatiri>
    {
        public void Configure(EntityTypeBuilder<SatisFaturaSatiri> builder)
        {
            // Navigation adı ile FK kolon adı convention'a uymadığı için ("StokKarti" için EF "StokKartiId" bekler,
            // kolonumuz "StokId") ilişkiler mevcut kolonlara açıkça bağlanır; yoksa EF ayrı bir gölge FK kolonu üretir.

            // Fatura silinince kalemleri de silinir (Cascade).
            builder.HasOne(s => s.SatisFaturasi)
                .WithMany(sf => sf.SatisFaturaSatirlari)
                .HasForeignKey(s => s.SatisFaturaId);

            builder.HasOne(s => s.StokKarti)
                .WithMany(st => st.SatisFaturaSatirlari)
                .HasForeignKey(s => s.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.KdvOrani).HasPrecision(5, 2);
        }
    }
}
