namespace OnMuhasebe.Application.Exceptions
{
    /// <summary>İstenen kayıt (cari, fatura, kullanıcı...) yok. Web katmanı bunu 404 yanıtına çevirir.</summary>
    public class KayitBulunamadiException : Exception
    {
        public KayitBulunamadiException(string mesaj) : base(mesaj)
        {
        }
    }
}
