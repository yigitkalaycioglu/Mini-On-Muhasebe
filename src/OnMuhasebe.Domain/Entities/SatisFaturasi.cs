namespace OnMuhasebe.Domain.Entities
{
    public class SatisFaturasi
    {
        public int Id { get; set; }
        public string FaturaNo { get; set; } = null!; // Benzersiz; "Satış Fatura No Formatı" parametresine göre üretilir
        public DateTime Tarih { get; set; }
        public int CariId { get; set; } // Müşteri
        public int SatisElemaniId { get; set; }

        // Kalemlerden hesaplanır.
        public decimal AraToplam { get; set; } // KDV hariç
        public decimal KdvToplam { get; set; }
        public decimal GenelToplam { get; set; }

        public string? Aciklama { get; set; }
        public int KullaniciId { get; set; } // Faturayı kesen kullanıcı
        public DateTime OlusturmaTarihi { get; set; }

        // Navigation properties
        public Cari Cari { get; set; } = null!;
        public SatisElemani SatisElemani { get; set; } = null!;
        public Kullanici Kullanici { get; set; } = null!;
        public ICollection<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; } = new List<SatisFaturaSatiri>();
    }
}
