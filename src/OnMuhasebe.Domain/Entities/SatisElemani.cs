namespace OnMuhasebe.Domain.Entities
{
    public class SatisElemani
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; } = null!;
        public string Telefon { get; set; } = null!;
        public bool Aktif { get; set; } = true;

        // Navigation properties
        public ICollection<SatisFaturasi> SatisFaturalari { get; set; } = new List<SatisFaturasi>();
    }
}
