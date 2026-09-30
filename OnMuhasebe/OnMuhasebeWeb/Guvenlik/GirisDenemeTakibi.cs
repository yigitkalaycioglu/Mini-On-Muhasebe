using Microsoft.Extensions.Caching.Memory;

namespace OnMuhasebeWeb.Guvenlik
{
    /// <summary>
    /// Kaba kuvvet (şifre deneme) saldırısına karşı iki katman:
    /// 1) IP başına hız sınırı: aynı adresten dakikada en fazla 10 giriş denemesi (Program.cs, "giris" politikası).
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
        public const int IpBasinaDakikadaDeneme = 10;
        public const int EnFazlaHataliDeneme = 5;
        public static readonly TimeSpan KilitSuresi = TimeSpan.FromMinutes(5);

        private sealed class Kayit
        {
            public int HataliDeneme;
            public DateTimeOffset? KilitBitisi;
        }

        private readonly IMemoryCache _cache;
        public GirisDenemeTakibi(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string Anahtar(string kullaniciAdi) => "giris-denemesi:" + kullaniciAdi.Trim().ToLowerInvariant();

        /// <summary>Kullanıcı adı kilitliyse kalan süre, değilse null.</summary>
        public TimeSpan? KalanKilitSuresi(string kullaniciAdi)
        {
            if (_cache.TryGetValue(Anahtar(kullaniciAdi), out Kayit? kayit) && kayit != null)
            {
                lock (kayit)
                {
                    var kalan = kayit.KilitBitisi - DateTimeOffset.UtcNow;
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
                // Son hatalı denemeden 15 dakika sonra sayaç kendiliğinden silinir.
                giris.SlidingExpiration = TimeSpan.FromMinutes(15);
                return new Kayit();
            })!;

            lock (kayit)
            {
                // Süresi dolmuş bir kilitten sonra sayım baştan başlar.
                if (kayit.KilitBitisi <= DateTimeOffset.UtcNow)
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
                kayit.KilitBitisi = DateTimeOffset.UtcNow + KilitSuresi;
                return true;
            }
        }

        /// <summary>Başarılı girişten sonra sayaç sıfırlanır.</summary>
        public void Sifirla(string kullaniciAdi) => _cache.Remove(Anahtar(kullaniciAdi));
    }
}
