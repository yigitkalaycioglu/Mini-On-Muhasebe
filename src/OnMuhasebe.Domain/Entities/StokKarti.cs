namespace OnMuhasebe.Domain.Entities
{
    public class StokKarti
    {
        public int Id { get; set; }
        public string StokKodu { get; set; } = null!; // Benzersiz
        public string StokAdi { get; set; } = null!;
        public string Birim { get; set; } = null!; // Adet, Kg, Lt, vb.
        public decimal KdvOrani { get; set; } // Yeni kartta "Varsayılan KDV Oranı" parametresinden gelir
        public decimal AlisFiyati { get; set; }
        public decimal SatisFiyati { get; set; }
        public decimal KritikStok { get; set; } // Uyarı seviyesi
        public bool Aktif { get; set; } = true;

        // Mevcut miktar saklanmaz; StokHareketleri'nden hesaplanır (Σ Giriş − Σ Çıkış).

        // Navigation properties
        public ICollection<SatisFaturaSatiri> SatisFaturaSatirlari { get; set; } = new List<SatisFaturaSatiri>();
        public ICollection<AlisFaturaSatiri> AlisFaturaSatirlari { get; set; } = new List<AlisFaturaSatiri>();
        public ICollection<StokHareket> StokHareketleri { get; set; } = new List<StokHareket>();
    }
}
