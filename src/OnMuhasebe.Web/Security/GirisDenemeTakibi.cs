using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace OnMuhasebe.Web.Security
{
    /// <summary>
    /// Kaba kuvvet (şifre deneme) saldırısına karşı iki katman (sınırlar GirisGuvenligiAyarlari'nda):
    /// 1) IP başına hız sınırı: aynı adresten dakikada en fazla 10 giriş denemesi ("giris" politikası, AddWeb).
    /// 2) Kullanıcı adı başına kilit: üst üste 5 hatalı denemeden sonra o kullanıcı adıyla 5 dakika giriş yapılamaz.
    ///    Farklı IP'lerden dağıtılmış denemeleri de durdurur.
    ///
    /// Sayaçlar bellekte tutulur; dökümandaki tablo yapısını bozmamak için Kullanicilar tablosuna kolon eklenmedi.
    /// Uygulama yeniden başlayınca sıfırlanır. Kayıtlı olmayan kullanıcı adları da aynı şekilde sayılır ve kilitlenir;
    /// böylece kilit mesajından bir kullanıcı adının kayıtlı olup olmadığı anlaşılamaz.
    /// </summary>
    public class GirisDenemeTakibi
    {
        public const string HizSiniriPolitikasi = "giris";

        private sealed class Kayit
        {
            public int HataliDeneme;
            public DateTimeOffset? KilitBitisi;
        }

        private readonly IMemoryCache _cache;
        private readonly TimeProvider _zaman;
        private readonly GirisGuvenligiAyarlari _ayarlar;
        public GirisDenemeTakibi(IMemoryCache cache, TimeProvider zaman, IOptions<GirisGuvenligiAyarlari> ayarlar)
        {
            _cache = cache;
            _zaman = zaman;
            _ayarlar = ayarlar.Value;
        }

        public int EnFazlaHataliDeneme => _ayarlar.EnFazlaHataliDeneme;
        public TimeSpan KilitSuresi => _ayarlar.KilitSuresi;

        private static string Anahtar(string kullaniciAdi) => "giris-denemesi:" + kullaniciAdi.Trim().ToLowerInvariant();

        /// <summary>Kullanıcı adı kilitliyse kalan süre, değilse null.</summary>
        public TimeSpan? KalanKilitSuresi(string kullaniciAdi)
        {
            if (_cache.TryGetValue(Anahtar(kullaniciAdi), out Kayit? kayit) && kayit != null)
            {
                lock (kayit)
                {
                    var kalan = kayit.KilitBitisi - _zaman.GetUtcNow();
                    if (kalan > TimeSpan.Zero)
                    {
                        return kalan;
                    }
                }
            }
            return null;
        }

        /// <summary>Hatalı denemeyi sayar. Bu denemeyle kullanıcı adı kilitlendiyse true döner.</summary>
        public bool HataliDenemeKaydet(string kullaniciAdi)
        {
            var kayit = _cache.GetOrCreate(Anahtar(kullaniciAdi), giris =>
            {
                // Son hatalı denemeden 15 dakika (kilit daha uzunsa kilit süresi kadar) sonra sayaç kendiliğinden silinir.
                giris.SlidingExpiration = KilitSuresi > TimeSpan.FromMinutes(15) ? KilitSuresi : TimeSpan.FromMinutes(15);
                return new Kayit();
            })!;

            lock (kayit)
            {
                // Süresi dolmuş bir kilitten sonra sayım baştan başlar.
                if (kayit.KilitBitisi <= _zaman.GetUtcNow())
                {
                    kayit.KilitBitisi = null;
                    kayit.HataliDeneme = 0;
                }

                kayit.HataliDeneme++;
                if (kayit.HataliDeneme < EnFazlaHataliDeneme)
                {
                    return false;
                }

                kayit.HataliDeneme = 0;
                kayit.KilitBitisi = _zaman.GetUtcNow() + KilitSuresi;
                return true;
            }
        }

        /// <summary>Başarılı girişten sonra sayaç sıfırlanır.</summary>
        public void Sifirla(string kullaniciAdi) => _cache.Remove(Anahtar(kullaniciAdi));
    }
}
