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

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // Dökümandaki tutar ve miktar alanlarının hepsi decimal(18,2).
            // KDV oranları decimal(5,2) olduğu için aşağıda ayrıca belirtilir.
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }

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
                .HasForeignKey(s => s.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlisFaturaSatiri>()
                .HasOne(a => a.AlisFaturasi)
                .WithMany(af => af.AlisFaturaSatirlari)
                .HasForeignKey(a => a.AlisFaturaId);

            modelBuilder.Entity<AlisFaturaSatiri>()
                .HasOne(a => a.StokKarti)
                .WithMany(st => st.AlisFaturaSatirlari)
                .HasForeignKey(a => a.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StokHareket>()
                .HasOne(sh => sh.StokKarti)
                .WithMany(st => st.StokHareketleri)
                .HasForeignKey(sh => sh.StokId)
                .OnDelete(DeleteBehavior.Restrict);

            // Silme davranışı: fatura silinince kalemleri de silinir (yukarıdaki Cascade ilişkiler).
            // Cari, stok kartı, satış elemanı ve kullanıcı ise belgesi/hareketi varken veritabanı
            // seviyesinde de silinemez (Restrict). Uygulama bu durumda kaydı pasife alır; böylece
            // bir ana kayıt silinerek faturalar ve hareketler, yani muhasebe geçmişi, kaybolamaz.
            modelBuilder.Entity<SatisFaturasi>()
                .HasOne(f => f.Cari)
                .WithMany(c => c.SatisFaturalari)
                .HasForeignKey(f => f.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SatisFaturasi>()
                .HasOne(f => f.SatisElemani)
                .WithMany(e => e.SatisFaturalari)
                .HasForeignKey(f => f.SatisElemaniId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SatisFaturasi>()
                .HasOne(f => f.Kullanici)
                .WithMany(k => k.SatisFaturalari)
                .HasForeignKey(f => f.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlisFaturasi>()
                .HasOne(f => f.Cari)
                .WithMany(c => c.AlisFaturalari)
                .HasForeignKey(f => f.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AlisFaturasi>()
                .HasOne(f => f.Kullanici)
                .WithMany(k => k.AlisFaturalari)
                .HasForeignKey(f => f.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StokHareket>()
                .HasOne(h => h.Kullanici)
                .WithMany(k => k.StokHareketleri)
                .HasForeignKey(h => h.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CariHareket>()
                .HasOne(h => h.Cari)
                .WithMany(c => c.CariHareketleri)
                .HasForeignKey(h => h.CariId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CariHareket>()
                .HasOne(h => h.Kullanici)
                .WithMany(k => k.CariHareketleri)
                .HasForeignKey(h => h.KullaniciId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // Kolon tipleri: dökümanın 7. bölümündeki tabloyla birebir.
            // Formdan girilen metinlerin uzunluğu modeldeki [StringLength] ile belirlenir;
            // burada sunucunun atadığı alanlar (belge no, hareket tipi, şifre özeti...) ile
            // tarih ve KDV oranı tipleri tanımlanır. Tanımlanmasa EF Core nvarchar(max),
            // datetime2 ve decimal(18,2) kullanıyordu.
            modelBuilder.Entity<Kullanici>(e =>
            {
                e.Property(k => k.SifreHash).HasMaxLength(256);
                e.Property(k => k.Rol).HasMaxLength(20);
            });

            modelBuilder.Entity<StokKarti>()
                .Property(s => s.KdvOrani).HasPrecision(5, 2);

            modelBuilder.Entity<SatisFaturasi>(e =>
            {
                e.Property(f => f.FaturaNo).HasMaxLength(20);
                e.Property(f => f.Tarih).HasColumnType("date");
                e.Property(f => f.OlusturmaTarihi).HasColumnType("datetime");
            });

            modelBuilder.Entity<SatisFaturaSatiri>()
                .Property(s => s.KdvOrani).HasPrecision(5, 2);

            modelBuilder.Entity<AlisFaturasi>(e =>
            {
                e.Property(f => f.FaturaNo).HasMaxLength(20);
                e.Property(f => f.Tarih).HasColumnType("date");
                e.Property(f => f.OlusturmaTarihi).HasColumnType("datetime");
            });

            modelBuilder.Entity<AlisFaturaSatiri>()
                .Property(s => s.KdvOrani).HasPrecision(5, 2);

            modelBuilder.Entity<StokHareket>(e =>
            {
                e.Property(h => h.Tarih).HasColumnType("date");
                e.Property(h => h.HareketTipi).HasMaxLength(20);
                e.Property(h => h.Yon).HasMaxLength(10);
                e.Property(h => h.BelgeNo).HasMaxLength(20);
            });

            modelBuilder.Entity<CariHareket>(e =>
            {
                e.Property(h => h.Tarih).HasColumnType("date");
                e.Property(h => h.IslemTipi).HasMaxLength(20);
                e.Property(h => h.BelgeNo).HasMaxLength(20);
            });
        }
    }
}
