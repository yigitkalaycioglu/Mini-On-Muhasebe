namespace OnMuhasebe.Domain.Entities
{
    public class StokHareket
    {
        public int Id { get; set; }
        public int StokId { get; set; }
        public DateTime Tarih { get; set; }
        public string HareketTipi { get; set; } = null!; // "Satis", "Alis", "SayimFazlasi", "SayimEksigi"
        public string Yon { get; set; } = null!; // "Giris" veya "Cikis"; hareket tipine göre serviste atanır
        public decimal Miktar { get; set; }
        public string? BelgeNo { get; set; } // İlgili fatura ya da fiş numarası; serviste üretilir
        public string? Aciklama { get; set; }
        public int KullaniciId { get; set; } // İşlemi yapan kullanıcı

        // Navigation properties
        public StokKarti StokKarti { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
    }
}
