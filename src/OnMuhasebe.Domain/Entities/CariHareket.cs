namespace OnMuhasebe.Domain.Entities
{
    public class CariHareket
    {
        public int Id { get; set; }
        public int CariId { get; set; }
        public DateTime Tarih { get; set; }
        public string IslemTipi { get; set; } = null!; // "Satis", "Alis", "Tahsilat", "Odeme"

        // Bakiyeye etkisi: Borç − Alacak. Tahsilat alacak, ödeme borç olarak işlenir.
        public decimal Borc { get; set; }
        public decimal Alacak { get; set; }

        public string? OdemeTuru { get; set; } // "Nakit", "Havale", "Çek" (yalnızca tahsilat/ödemede)
        public string? BelgeNo { get; set; } // İlgili fatura ya da tahsilat/ödeme numarası; serviste üretilir
        public string? Aciklama { get; set; }
        public int KullaniciId { get; set; } // İşlemi yapan kullanıcı

        // Navigation properties
        public Cari Cari { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
    }
}
