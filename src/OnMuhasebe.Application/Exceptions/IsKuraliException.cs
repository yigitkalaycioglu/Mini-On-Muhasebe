namespace OnMuhasebe.Application.Exceptions
{
    /// <summary>
    /// İş kuralı ihlali (ör. "Bu stok kodu zaten kayıtlı.", "... için yeterli stok yok."). Mesaj kullanıcıya gösterilir.
    /// Alan verilmişse hata ilgili form alanının altında, verilmemişse formun üstündeki özette görünür.
    /// Kendi türü olduğu için controller'lar yalnızca bunu yakalar; çerçevenin (EF Core vb.) kendi hataları
    /// kullanıcıya mesaj olarak sızmaz, hata sayfasına düşer.
    /// </summary>
    public class IsKuraliException : Exception
    {
        public string? Alan { get; }

        public IsKuraliException(string mesaj) : base(mesaj)
        {
        }

        public IsKuraliException(string alan, string mesaj) : base(mesaj)
        {
            Alan = alan;
        }
    }
}
