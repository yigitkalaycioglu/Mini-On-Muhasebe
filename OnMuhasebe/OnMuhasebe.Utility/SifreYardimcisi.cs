using System.Security.Cryptography;

namespace OnMuhasebe.Utility
{
    public static class SifreYardimcisi
    {
        private const int IterasyonSayisi = 600000;

        public static string HashOlustur(string sifre)
        {
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(sifre, salt, IterasyonSayisi, HashAlgorithmName.SHA256, 32);

            return $"{IterasyonSayisi}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Dogrula(string sifre, string kayitliHash)
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
