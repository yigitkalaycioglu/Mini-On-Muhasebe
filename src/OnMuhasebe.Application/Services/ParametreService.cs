using System.Globalization;
using Microsoft.EntityFrameworkCore;
using OnMuhasebe.Application.Abstractions;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Domain.Rules;

namespace OnMuhasebe.Application.Services
{
    public class ParametreService : IParametreService
    {
        private readonly IApplicationDbContext _context;
        private readonly TimeProvider _zaman;

        // Servis istek başına oluşturulduğu için (Scoped) parametreler bir istekte bir kez okunur.
        private Dictionary<string, string>? _degerler;

        public ParametreService(IApplicationDbContext context, TimeProvider zaman)
        {
            _context = context;
            _zaman = zaman;
        }

        // Belge numaralarındaki yıl yerel saate göre belirlenir.
        private DateTime Simdi => _zaman.GetLocalNow().DateTime;

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

        // Tutar kolonları decimal(18,2); daha fazla basamak saklanamaz.
        private const int EnFazlaOndalikBasamak = 2;

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
                    throw new KayitBulunamadiException("Parametre bulunamadı.");
                }

                // Kod formdan gizli alan olarak gelse de değiştirilmez; doğrulama kayıttaki koda göre yapılır.
                kayit.ParametreDegeri = DegeriDogrula(kayit.ParametreKodu, gelen.ParametreDegeri, Simdi);
            }

            await FormatlarFarkliMiAsync();

            await _context.SaveChangesAsync();
            _degerler = null;
        }

        /// <summary>
        /// İki belge türü aynı numarayı üretirse numaralar karışır; bu yüzden formatların
        /// sabit kısımları (yıl yerleştirildikten sonra) birbirinden farklı olmalıdır.
        /// </summary>
        private async Task FormatlarFarkliMiAsync()
        {
            // Takip edilen kayıtlar sorguda aynı nesne olarak döner; henüz kaydedilmemiş yeni değerler de görülür.
            var formatParametreleri = await _context.Parametreler
                .Where(p => FormatParametreleri.Contains(p.ParametreKodu))
                .ToListAsync();
            var formatlar = formatParametreleri
                .Select(p => p.ParametreDegeri)
                .Concat(new[] { Sabitler.TahsilatNoFormati, Sabitler.OdemeNoFormati });

            var cakisanVar = formatlar
                .Select(f => FormatCoz(f, Simdi))
                .GroupBy(f => (f.OnEk.ToUpperInvariant(), f.SonEk.ToUpperInvariant()))
                .Any(g => g.Count() > 1);

            if (cakisanVar)
            {
                throw new IsKuraliException("Belge numarası formatları birbirinden farklı olmalıdır (ör. SAT-, ALS-, SF-, SE-; TAH- ve ODE- tahsilat/ödeme için ayrılmıştır).");
            }
        }

        private static string DegeriDogrula(string kod, string? deger, DateTime simdi)
        {
            deger = deger?.Trim() ?? "";
            if (deger.Length == 0)
            {
                throw new IsKuraliException($"{kod} parametresi boş bırakılamaz.");
            }

            if (AnahtarParametreler.Contains(kod))
            {
                if (deger != Sabitler.DegerAcik && deger != Sabitler.DegerKapali)
                {
                    throw new IsKuraliException($"{kod} parametresi Acik veya Kapali olmalıdır.");
                }
            }
            else if (kod == Sabitler.ParamVarsayilanKdvOrani)
            {
                if (!SayiyaCevir(deger, out var kdv) || kdv < 0 || kdv > 100)
                {
                    throw new IsKuraliException("Varsayılan KDV oranı 0 ile 100 arasında bir sayı olmalıdır.");
                }
                deger = kdv.ToString(CultureInfo.InvariantCulture);
            }
            else if (kod == Sabitler.ParamOndalikBasamak)
            {
                if (!int.TryParse(deger, out var basamak) || basamak < 0 || basamak > EnFazlaOndalikBasamak)
                {
                    throw new IsKuraliException($"Ondalık basamak 0 ile {EnFazlaOndalikBasamak} arasında bir tam sayı olmalıdır (tutarlar 2 basamakla saklanır).");
                }
            }
            else if (FormatParametreleri.Contains(kod))
            {
                // Format çözülemiyorsa FormatCoz hata fırlatır.
                if (FormatCoz(deger, simdi).NumaraUzunlugu > BelgeNoUzunlugu)
                {
                    throw new IsKuraliException($"{kod} formatı en fazla {BelgeNoUzunlugu} karakterlik numara üretmelidir.");
                }
            }

            if (deger.Length > 250)
            {
                throw new IsKuraliException($"{kod} parametresi en fazla 250 karakter olabilir.");
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

        public async Task<int> GetOndalikBasamakAsync()
        {
            // Veritabanında elle geçersiz bir değer yazılmış olsa da 0-2 aralığında kalır.
            return Math.Clamp((int)await GetSayiAsync(Sabitler.ParamOndalikBasamak), 0, EnFazlaOndalikBasamak);
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
            var bicim = FormatCoz(format, Simdi);

            // Yalnızca aynı ön ekle başlayan numaralar veritabanından getirilir; en büyük sıra burada bulunur.
            var onEk = bicim.OnEk;
            var nolar = await mevcutNolar
                .Where(n => n != null && n.StartsWith(onEk))
                .ToListAsync();

            return bicim.Sonraki(nolar);
        }

        /// <summary>Format çözülemiyorsa (ör. {0000} alanı yok) kullanıcıya gösterilecek iş kuralı hatası verir.</summary>
        private static BelgeNoFormati FormatCoz(string format, DateTime tarih)
        {
            try
            {
                return BelgeNoFormati.Coz(format, tarih);
            }
            catch (FormatException ex)
            {
                throw new IsKuraliException(ex.Message);
            }
        }
    }
}
