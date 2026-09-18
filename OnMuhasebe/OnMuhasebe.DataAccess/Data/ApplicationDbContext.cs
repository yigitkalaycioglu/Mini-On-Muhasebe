using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Models;

namespace OnMuhasebe.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<SatisElemani> SatisElemanlari { get; set; }
        public DbSet<Cari> Cariler { get; set; }
        public DbSet<StokKarti> StokKartlari { get; set; }
        public DbSet<SatisFaturasi> SatisFaturalari { get; set; }
        public DbSet<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; }
        public DbSet<AlisFaturasi> AlisFaturalari { get; set; }
        public DbSet<AlisFaturaSatiri> AlisFaturaSatirlari { get; set; }
        public DbSet<StokHareket> StokHareketler { get; set; }
        public DbSet<CariHareket> CariHareketler { get; set; }
        public DbSet<Parametre> Parametreler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Navigation property adı ile FK kolon adı convention'a uymadığı için
            // (örn. "StokKarti" navigation'ı için EF "StokKartiId" bekler, oysa
            // kolonumuz "StokId") EF Core bunları otomatik eşleştiremeyip ayrı
            // birer gölge FK kolonu üretiyordu. Aşağıda ilişkiler mevcut kolonlara
            // açıkça bağlanıyor ki gölge kolon oluşmasın.

            modelBuilder.Entity<SatisFaturaSatiri>()
                .HasOne(s => s.SatisFaturasi)
                .WithMany(sf => sf.SatisFaturaSatirlari)
                .HasForeignKey(s => s.SatisFaturaId);

            modelBuilder.Entity<SatisFaturaSatiri>()
                .HasOne(s => s.StokKarti)
                .WithMany(st => st.SatisFaturaSatirlari)
                .HasForeignKey(s => s.StokId);

            modelBuilder.Entity<AlisFaturaSatiri>()
                .HasOne(a => a.AlisFaturasi)
                .WithMany(af => af.AlisFaturaSatirlari)
                .HasForeignKey(a => a.AlisFaturaId);

            modelBuilder.Entity<AlisFaturaSatiri>()
                .HasOne(a => a.StokKarti)
                .WithMany(st => st.AlisFaturaSatirlari)
                .HasForeignKey(a => a.StokId);

            modelBuilder.Entity<StokHareket>()
                .HasOne(sh => sh.StokKarti)
                .WithMany(st => st.StokHareketleri)
                .HasForeignKey(sh => sh.StokId);

            // Benzersizlik yalnızca servis katmanında kontrol ediliyordu; eşzamanlı iki kayıt
            // aynı kodu yazabildiği için veritabanı seviyesinde de garanti altına alınıyor.
            modelBuilder.Entity<Cari>()
                .HasIndex(c => c.CariKodu)
                .IsUnique();

            modelBuilder.Entity<StokKarti>()
                .HasIndex(s => s.StokKodu)
                .IsUnique();

            modelBuilder.Entity<Kullanici>()
                .HasIndex(k => k.KullaniciAdi)
                .IsUnique();

            modelBuilder.Entity<Parametre>()
                .HasIndex(p => p.ParametreKodu)
                .IsUnique();

            modelBuilder.Entity<SatisFaturasi>()
                .HasIndex(f => f.FaturaNo)
                .IsUnique();

            modelBuilder.Entity<AlisFaturasi>()
                .HasIndex(f => f.FaturaNo)
                .IsUnique();

            // Fatura silinirken ilgili hareketler BelgeNo üzerinden bulunuyor.
            modelBuilder.Entity<StokHareket>()
                .HasIndex(h => h.BelgeNo);

            modelBuilder.Entity<CariHareket>()
                .HasIndex(h => h.BelgeNo);
        }
    }
}
