namespace OnMuhasebe.Domain.Entities
{
    public class AlisFaturasi
    {
        public int Id { get; set; }
        public string FaturaNo { get; set; } = null!; // Benzersiz; "Alış Fatura No Formatı" parametresine göre üretilir
        public DateTime Tarih { get; set; }
        public int CariId { get; set; } // Tedarikçi

        // Kalemlerden hesaplanır.
        public decimal AraToplam { get; set; } // KDV hariç
        public decimal KdvToplam { get; set; }
        public decimal GenelToplam { get; set; }

        public string? Aciklama { get; set; }
        public int KullaniciId { get; set; } // Faturayı giren kullanıcı
        public DateTime OlusturmaTarihi { get; set; }

        // Navigation properties
        public Cari Cari { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
        public ICollection<AlisFaturaSatiri> AlisFaturaSatirlari { get; set; } = new List<AlisFaturaSatiri>();
    }
}
