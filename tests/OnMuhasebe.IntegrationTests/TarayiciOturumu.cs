using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OnMuhasebe.IntegrationTests
{
    /// <summary>Çerezleri saklayan, formları anti-forgery token'ıyla gönderen basit bir tarayıcı.</summary>
    public sealed class TarayiciOturumu
    {
        private readonly HttpClient _istemci;

        public TarayiciOturumu(UygulamaFabrikasi fabrika)
        {
            _istemci = fabrika.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        public Task<HttpResponseMessage> GetAsync(string yol) => _istemci.GetAsync(yol, TestContext.Current.CancellationToken);

        public async Task<string> SayfaAsync(string yol)
        {
            var yanit = await GetAsync(yol);
            Assert.Equal(HttpStatusCode.OK, yanit.StatusCode);
            return await MetinAsync(yanit);
        }

        /// <summary>Yanıtın HTML'i; Razor'un kodladığı Türkçe karakterler (ör. &amp;#x131;) çözülmüş olarak.</summary>
        public static async Task<string> MetinAsync(HttpResponseMessage yanit) =>
            WebUtility.HtmlDecode(await yanit.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        /// <summary>Form sayfasını açıp token'ı alır, verilen alanlarla formu gönderir.</summary>
        public async Task<HttpResponseMessage> FormGonderAsync(string formSayfasi, string hedef, params (string Ad, string Deger)[] alanlar)
        {
            var sayfa = await SayfaAsync(formSayfasi);
            var token = Regex.Match(sayfa, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
            Assert.False(string.IsNullOrEmpty(token), $"{formSayfasi} sayfasında anti-forgery token bulunamadı.");

            var icerik = new FormUrlEncodedContent(alanlar
                .Select(a => new KeyValuePair<string, string>(a.Ad, a.Deger))
                .Append(new KeyValuePair<string, string>("__RequestVerificationToken", token)));
            return await _istemci.PostAsync(hedef, icerik, TestContext.Current.CancellationToken);
        }

        public async Task GirisYapAsync(string kullaniciAdi, string sifre)
        {
            var yanit = await FormGonderAsync("/Account/Login", "/Account/Login", ("kullaniciAdi", kullaniciAdi), ("sifre", sifre));
            Assert.Equal(HttpStatusCode.Redirect, yanit.StatusCode);
        }

        /// <summary>Yönlendirme adresinin yol ve sorgu kısmı (adres mutlak ya da göreli olabilir).</summary>
        public static string YonlendirmeYolu(HttpResponseMessage yanit)
        {
            var adres = yanit.Headers.Location ?? throw new InvalidOperationException("Yanıt bir yönlendirme değil.");
            return adres.IsAbsoluteUri ? adres.PathAndQuery : adres.OriginalString;
        }
    }
}
