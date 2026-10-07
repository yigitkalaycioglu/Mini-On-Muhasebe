using System.Security.Cryptography;
using OnMuhasebe.Application.Abstractions;

namespace OnMuhasebe.Infrastructure.Security
{
    /// <summary>
    /// PBKDF2-SHA256, 16 bayt rastgele salt, 600.000 iterasyon, 32 bayt çıktı.
    /// Saklama biçimi: "iterasyon.saltBase64.hashBase64" (seed_data.sql'deki özetler de bu biçimde).
    /// </summary>
    public sealed class Pbkdf2SifreHashleyici : ISifreHashleyici
    {
        private const int IterasyonSayisi = 600000;

        // Kayıtlı olmayan kullanıcı adıyla girişte karşılaştırılan geçerli biçimli bir özet (uygulama başına bir kez üretilir).
        private readonly Lazy<string> _sahteHash;

        public Pbkdf2SifreHashleyici()
        {
            _sahteHash = new Lazy<string>(() => HashOlustur(Guid.NewGuid().ToString()));
        }

        public string HashOlustur(string sifre)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, IterasyonSayisi, HashAlgorithmName.SHA256, 32);

            return $"{IterasyonSayisi}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Dogrula(string sifre, string? kayitliHash)
        {
            if (kayitliHash == null)
            {
                // Kullanıcı yoksa da aynı hesap yapılır; yanıt süresi kullanıcı adının varlığını ele vermez.
                HashlerEslesiyor(sifre, _sahteHash.Value);
                return false;
            }

            return HashlerEslesiyor(sifre, kayitliHash);
        }

        private static bool HashlerEslesiyor(string sifre, string kayitliHash)
        {
            var parcalar = kayitliHash.Split('.');
            if (parcalar.Length != 3)
                return false;

            if (!int.TryParse(parcalar[0], out var iterasyonSayisi))
                return false;

            byte[] salt, hash;
            try
            {
                salt = Convert.FromBase64String(parcalar[1]);
                hash = Convert.FromBase64String(parcalar[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var hashDogrulama = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, iterasyonSayisi, HashAlgorithmName.SHA256, hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, hashDogrulama);
        }
    }
}
