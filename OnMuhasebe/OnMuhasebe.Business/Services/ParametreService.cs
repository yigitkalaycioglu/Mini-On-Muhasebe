using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.DataAccess;
using OnMuhasebe.Models;

namespace OnMuhasebe.Business.Services
{
    public class ParametreService : IParametreService
    {
        private readonly ApplicationDbContext _context;

        // Servis istek başına oluşturulduğu için (Scoped) parametreler bir istekte bir kez okunur.
        private Dictionary<string, string>? _degerler;

        public ParametreService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dökümanın 9. bölümündeki örnek değerler. Tabloda kayıt yoksa bunlar kullanılır.
        // Negatif stok kontrolü güvenli taraf olsun diye varsayılan olarak açıktır.
        private static readonly Dictionary<string, string> VarsayilanDegerler = new()
        {
            [Sabitler.ParamFirmaUnvani] = "",
            [Sabitler.ParamVergiDairesiNo] = "",
            [Sabitler.ParamVarsayilanKdvOrani] = "20",
            [Sabitler.ParamParaBirimi] = "TL",
            [Sabitler.ParamOndalikBasamak] = "2",
            [Sabitler.ParamNegatifStokKontrolu] = Sabitler.DegerAcik,
            [Sabitler.ParamSatisFaturaNoFormati] = "SAT-{yyyy}-{0000}",
            [Sabitler.ParamAlisFaturaNoFormati] = "ALS-{yyyy}-{0000}",
            [Sabitler.ParamSayimFazlasiFisNoFormati] = "SF-{yyyy}-{0000}",
            [Sabitler.ParamSayimEksigiFisNoFormati] = "SE-{yyyy}-{0000}",
            [Sabitler.ParamKritikStokUyarisi] = Sabitler.DegerAcik
        };

        private static readonly string[] AnahtarParametreler =
        {
            Sabitler.ParamNegatifStokKontrolu,
            Sabitler.ParamKritikStokUyarisi
        };

        private static readonly string[] FormatParametreleri =
        {
            Sabitler.ParamSatisFaturaNoFormati,
            Sabitler.ParamAlisFaturaNoFormati,
            Sabitler.ParamSayimFazlasiFisNoFormati,
            Sabitler.ParamSayimEksigiFisNoFormati
        };

        // Belge numarası kolonları nvarchar(20)
        private const int BelgeNoUzunlugu = 20;

        private static readonly Regex SiraDeseni = new(@"\{(0+)\}");

        public async Task<List<Parametre>> GetAllParametrelerAsync()
        {
            return await _context.Parametreler.OrderBy(p => p.Id).ToListAsync();
        }

        public async Task UpdateParametrelerAsync(List<Parametre> parametreler)
        {
            var idler = parametreler.Select(p => p.Id).ToList();
            var kayitlar = await _context.Parametreler
                .Where(p => idler.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var gelen in parametreler)
            {
                if (!kayitlar.TryGetValue(gelen.Id, out var kayit))
                {
                    throw new KeyNotFoundException("Parametre bulunamadı.");
                }

                // Kod formdan gizli alan olarak gelse de değiştirilmez; doğrulama kayıttaki koda göre yapılır.
                kayit.ParametreDegeri = DegeriDogrula(kayit.ParametreKodu, gelen.ParametreDegeri);
            }

            await _context.SaveChangesAsync();
            _degerler = null;
        }

        private static string DegeriDogrula(string kod, string? deger)
        {
            deger = deger?.Trim() ?? "";
            if (deger.Length == 0)
            {
                throw new InvalidOperationException($"{kod} parametresi boş bırakılamaz.");
            }

            if (AnahtarParametreler.Contains(kod))
            {
                if (deger != Sabitler.DegerAcik && deger != Sabitler.DegerKapali)
                {
                    throw new InvalidOperationException($"{kod} parametresi Acik veya Kapali olmalıdır.");
                }
            }
            else if (kod == Sabitler.ParamVarsayilanKdvOrani)
            {
                if (!SayiyaCevir(deger, out var kdv) || kdv < 0 || kdv > 100)
                {
                    throw new InvalidOperationException("Varsayılan KDV oranı 0 ile 100 arasında bir sayı olmalıdır.");
                }
                deger = kdv.ToString(CultureInfo.InvariantCulture);
            }
            else if (kod == Sabitler.ParamOndalikBasamak)
            {
                if (!int.TryParse(deger, out var basamak) || basamak < 0 || basamak > 4)
                {
                    throw new InvalidOperationException("Ondalık basamak 0 ile 4 arasında bir tam sayı olmalıdır.");
                }
            }
            else if (FormatParametreleri.Contains(kod))
            {
                // Format çözülemiyorsa FormatCoz hata fırlatır.
                var (onEk, sonEk, basamak) = FormatCoz(deger, DateTime.Now);
                if (onEk.Length + basamak + sonEk.Length > BelgeNoUzunlugu)
                {
                    throw new InvalidOperationException($"{kod} formatı en fazla {BelgeNoUzunlugu} karakterlik numara üretmelidir.");
                }
            }

            if (deger.Length > 250)
            {
                throw new InvalidOperationException($"{kod} parametresi en fazla 250 karakter olabilir.");
            }

            return deger;
        }

        private async Task<Dictionary<string, string>> DegerleriGetirAsync()
        {
            return _degerler ??= await _context.Parametreler
                .AsNoTracking()
                .ToDictionaryAsync(p => p.ParametreKodu, p => p.ParametreDegeri);
        }

        private static string Varsayilan(string parametreKodu) =>
            VarsayilanDegerler.TryGetValue(parametreKodu, out var deger) ? deger : "";

        public async Task<string> GetDegerAsync(string parametreKodu)
        {
            var degerler = await DegerleriGetirAsync();
            return degerler.TryGetValue(parametreKodu, out var deger) && !string.IsNullOrWhiteSpace(deger)
                ? deger
                : Varsayilan(parametreKodu);
        }

        public async Task<bool> AcikMiAsync(string parametreKodu)
        {
            var deger = await GetDegerAsync(parametreKodu);
            if (deger == Sabitler.DegerAcik) return true;
            if (deger == Sabitler.DegerKapali) return false;
            return Varsayilan(parametreKodu) == Sabitler.DegerAcik;
        }

        public async Task<decimal> GetSayiAsync(string parametreKodu)
        {
            var deger = await GetDegerAsync(parametreKodu);
            if (SayiyaCevir(deger, out var sayi)) return sayi;
            return SayiyaCevir(Varsayilan(parametreKodu), out var varsayilan) ? varsayilan : 0;
        }

        // Hem "20.5" hem "20,5" kabul edilir.
        private static bool SayiyaCevir(string deger, out decimal sayi)
        {
            return decimal.TryParse(deger.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out sayi);
        }

        public async Task<string> YeniBelgeNoAsync(string formatParametreKodu, IQueryable<string?> mevcutNolar)
        {
            var format = await GetDegerAsync(formatParametreKodu);
            return await YeniBelgeNoFormattanAsync(format, mevcutNolar);
        }

        public async Task<string> YeniBelgeNoFormattanAsync(string format, IQueryable<string?> mevcutNolar)
        {
            var (onEk, sonEk, basamak) = FormatCoz(format, DateTime.Now);

            var nolar = await mevcutNolar
                .Where(n => n != null && n.StartsWith(onEk))
                .ToListAsync();

            var sonSira = 0;
            foreach (var no in nolar)
            {
                if (no == null || !no.EndsWith(sonEk) || no.Length < onEk.Length + sonEk.Length)
                {
                    continue;
                }

                var sira = no.Substring(onEk.Length, no.Length - onEk.Length - sonEk.Length);
                if (int.TryParse(sira, out var sayi) && sayi > sonSira)
                {
                    sonSira = sayi;
                }
            }

            return onEk + (sonSira + 1).ToString().PadLeft(basamak, '0') + sonEk;
        }

        /// <summary>
        /// "SAT-{yyyy}-{0000}" → ön ek "SAT-2026-", son ek "", sıra no basamağı 4.
        /// {yyyy}: dört haneli yıl, {yy}: iki haneli yıl, {0000}: sıfır sayısı kadar basamaklı sıra no.
        /// </summary>
        private static (string OnEk, string SonEk, int Basamak) FormatCoz(string format, DateTime tarih)
        {
            var yilli = format
                .Replace("{yyyy}", tarih.Year.ToString())
                .Replace("{yy}", (tarih.Year % 100).ToString("D2"));

            var eslesmeler = SiraDeseni.Matches(yilli);
            if (eslesmeler.Count != 1)
            {
                throw new InvalidOperationException($"\"{format}\" formatında sıra numarası için tek bir {{0000}} alanı olmalıdır.");
            }

            var m = eslesmeler[0];
            return (yilli[..m.Index], yilli[(m.Index + m.Length)..], m.Groups[1].Length);
        }
    }
}
