using System.ComponentModel.DataAnnotations;

namespace OnMuhasebe.Web.Security
{
    /// <summary>
    /// Giriş güvenliği ayarları: appsettings.json'daki "GirisGuvenligi" bölümünden ya da
    /// GirisGuvenligi__KilitSuresi gibi ortam değişkenlerinden okunur. Geçersiz değerle uygulama başlamaz.
    /// </summary>
    public class GirisGuvenligiAyarlari
    {
        public const string Bolum = "GirisGuvenligi";

        /// <summary>Aynı IP adresinden dakikada en fazla kaç giriş denemesi yapılabilir.</summary>
        [Range(1, 1000)]
        public int IpBasinaDakikadaDeneme { get; set; } = 10;

        /// <summary>Bir kullanıcı adıyla üst üste kaç hatalı denemeden sonra giriş kilitlenir.</summary>
        [Range(1, 100)]
        public int EnFazlaHataliDeneme { get; set; } = 5;

        /// <summary>Kilitlenen kullanıcı adıyla ne kadar süre giriş yapılamaz.</summary>
        [Range(typeof(TimeSpan), "00:00:01", "1.00:00:00")]
        public TimeSpan KilitSuresi { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>İşlem yapılmadığında oturumun açık kalacağı süre (her istekte yenilenir).</summary>
        [Range(typeof(TimeSpan), "00:01:00", "7.00:00:00")]
        public TimeSpan OturumSuresi { get; set; } = TimeSpan.FromHours(8);
    }
}
