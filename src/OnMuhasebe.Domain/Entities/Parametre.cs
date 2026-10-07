namespace OnMuhasebe.Domain.Entities
{
    public class Parametre
    {
        public int Id { get; set; }
        public string ParametreKodu { get; set; } = null!; // Benzersiz (Sabitler.Param...)
        public string ParametreDegeri { get; set; } = null!;
        public string? Aciklama { get; set; }
    }
}
