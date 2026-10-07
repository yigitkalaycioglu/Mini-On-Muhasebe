namespace OnMuhasebe.Domain.Entities
{
    public class Cari
    {
        public int Id { get; set; }
        public string CariKodu { get; set; } = null!; // Benzersiz
        public string Unvan { get; set; } = null!;
        public byte CariTipi { get; set; } // 1=Müşteri, 2=Tedarikçi, 3=Her ikisi (Sabitler.CariTipi...)
        public string? VergiDairesi { get; set; }
        public string? VergiNo { get; set; }
        public string? Telefon { get; set; }
        public string? Adres { get; set; }
        public bool Aktif { get; set; } = true;

        // Bakiye saklanmaz; CariHareketleri'nden hesaplanır (Σ Borç − Σ Alacak).

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
        public ICollection<AlisFaturasi> AlisFaturalari { get; set; } = new List<AlisFaturasi>();
        public ICollection<CariHareket> CariHareketleri { get; set; } = new List<CariHareket>();
    }
}
