namespace OnMuhasebe.Domain.Entities
{
    public class SatisFaturaSatiri
    {
        public int Id { get; set; }
        public int SatisFaturaId { get; set; }
        public int StokId { get; set; }
        public decimal Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal KdvOrani { get; set; }
        public decimal SatirTutari { get; set; } // Miktar × BirimFiyat (KDV hariç)

        // Navigation properties
        public SatisFaturasi SatisFaturasi { get; set; } = null!;
        public StokKarti StokKarti { get; set; } = null!;
    }
}
