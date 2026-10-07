namespace OnMuhasebe.Business
{
    /// <summary>
    /// Belirli bir form alanına ait iş kuralı hatası (ör. "Bu stok kodu zaten kayıtlı.").
    /// Controller mesajı ModelState'e bu alanın adıyla ekler; böylece hata ilgili kutunun altında görünür.
    /// InvalidOperationException'dan türediği için genel iş kuralı hatası gibi de yakalanabilir.
    /// </summary>
    public class AlanHatasiException : InvalidOperationException
    {
        public string Alan { get; }

        public AlanHatasiException(string alan, string mesaj) : base(mesaj)
        {
            Alan = alan;
        }
    }
}
