using System.Text.RegularExpressions;

namespace OnMuhasebe.Domain.Rules
{
    /// <summary>
    /// Belge numarası formatı. "SAT-{yyyy}-{0000}" 2026 yılı için → ön ek "SAT-2026-", son ek "", sıra no basamağı 4.
    /// {yyyy}: dört haneli yıl, {yy}: iki haneli yıl, {0000}: sıfır sayısı kadar basamaklı sıra no.
    /// </summary>
    public readonly record struct BelgeNoFormati(string OnEk, string SonEk, int Basamak)
    {
        private static readonly Regex SiraDeseni = new(@"\{(0+)\}");

        /// <summary>Üretilecek numaranın uzunluğu (sıra no basamak sayısını aşmadıkça).</summary>
        public int NumaraUzunlugu => OnEk.Length + Basamak + SonEk.Length;

        /// <exception cref="FormatException">Formatta sıra numarası için tek bir {0000} alanı yoksa.</exception>
        public static BelgeNoFormati Coz(string format, DateTime tarih)
        {
            var yilli = format
                .Replace("{yyyy}", tarih.Year.ToString())
                .Replace("{yy}", (tarih.Year % 100).ToString("D2"));

            var eslesmeler = SiraDeseni.Matches(yilli);
            if (eslesmeler.Count != 1)
            {
                throw new FormatException($"\"{format}\" formatında sıra numarası için tek bir {{0000}} alanı olmalıdır.");
            }

            var m = eslesmeler[0];
            return new BelgeNoFormati(yilli[..m.Index], yilli[(m.Index + m.Length)..], m.Groups[1].Length);
        }

        /// <summary>
        /// Bu formattaki mevcut numaraların en büyük sıra numarasının bir fazlası (ör. SAT-2026-0004 → SAT-2026-0005).
        /// Formata uymayan numaralar yok sayılır; hiç numara yoksa sıra 1'den başlar.
        /// </summary>
        public string Sonraki(IEnumerable<string?> mevcutNolar)
        {
            var sonSira = 0;
            foreach (var no in mevcutNolar)
            {
                if (no == null || !no.StartsWith(OnEk) || !no.EndsWith(SonEk) || no.Length < OnEk.Length + SonEk.Length)
                {
                    continue;
                }

                var sira = no.Substring(OnEk.Length, no.Length - OnEk.Length - SonEk.Length);
                if (int.TryParse(sira, out var sayi) && sayi > sonSira)
                {
                    sonSira = sayi;
                }
            }

            return OnEk + (sonSira + 1).ToString().PadLeft(Basamak, '0') + SonEk;
        }
    }
}
