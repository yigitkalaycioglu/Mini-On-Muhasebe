using Microsoft.AspNetCore.Mvc.Rendering;

namespace OnMuhasebe.Web.Extensions
{
    public static class TutarBicimi
    {
        /// <summary>
        /// Tutarı "Ondalık Basamak" parametresindeki basamak sayısıyla biçimlendirir (ör. 1.250,50).
        /// Biçim GorunumParametreleriFilter tarafından ViewData'ya konur; view'de: @Html.Tutar(fatura.GenelToplam)
        /// Miktarlar ve girilen birim fiyatlar yuvarlanmadığı için bu yardımcı yalnızca tutarlarda kullanılır.
        /// </summary>
        public static string Tutar(this IHtmlHelper html, decimal tutar) =>
            tutar.ToString(html.ViewData["TutarBicimi"] as string ?? "N2");
    }
}
