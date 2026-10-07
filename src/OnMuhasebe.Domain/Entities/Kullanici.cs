namespace OnMuhasebe.Domain.Entities
{
    public class Kullanici
    {
        public int Id { get; set; }
        public string KullaniciAdi { get; set; } = null!; // Benzersiz
        public string SifreHash { get; set; } = null!; // PBKDF2 özeti; düz şifre saklanmaz
        public string AdSoyad { get; set; } = null!;
        public string Rol { get; set; } = null!; // "Yönetici" veya "Standart"
        public bool Aktif { get; set; } = true; // Pasif kullanıcı giriş yapamaz

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
        public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
        public ICollection<StokHareket> StokHareketleri { get; set; } = new List<StokHareket>();
        public ICollection<CariHareket> CariHareketleri { get; set; } = new List<CariHareket>();
    }
}
