namespace OnMuhasebe.Application.Abstractions
{
    /// <summary>
    /// Parola özeti (hash) üretme ve doğrulama. Uygulaması Infrastructure katmanında (PBKDF2);
    /// servisler algoritmayı bilmez, testlerde hızlı bir sahte uygulama kullanılabilir.
    /// </summary>
    public interface ISifreHashleyici
    {
        string HashOlustur(string sifre);

        /// <summary>
        /// Şifre kayıtlı özetle eşleşiyorsa true döner. Kullanıcı bulunamadığında kayitliHash null verilir;
        /// doğrulama yine aynı sürede çalışıp false döner, böylece yanıt süresinden kullanıcı adının
        /// kayıtlı olup olmadığı anlaşılamaz.
        /// </summary>
        bool Dogrula(string sifre, string? kayitliHash);
    }
}
