using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;

namespace OnMuhasebe.Web.Filters
{
    /// <summary>
    /// Her sayfada gereken görüntü parametrelerini (firma bilgisi, para birimi, tutar biçimi) ViewData'ya koyar.
    /// Böylece her controller aynı değerleri ayrı ayrı okumak zorunda kalmaz.
    /// </summary>
    public class GorunumParametreleriFilter : IAsyncResultFilter
    {
        private readonly IParametreService _parametreService;
        private readonly ILogger<GorunumParametreleriFilter> _logger;
        public GorunumParametreleriFilter(IParametreService parametreService, ILogger<GorunumParametreleriFilter> logger)
        {
            _parametreService = parametreService;
            _logger = logger;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ViewResult viewResult)
            {
                try
                {
                    var viewData = viewResult.ViewData;
                    viewData["FirmaUnvani"] = await _parametreService.GetDegerAsync(Sabitler.ParamFirmaUnvani);
                    viewData["VergiDairesiNo"] = await _parametreService.GetDegerAsync(Sabitler.ParamVergiDairesiNo);
                    viewData["ParaBirimi"] = await _parametreService.GetDegerAsync(Sabitler.ParamParaBirimi);

                    // Tutarlar "Ondalık Basamak" parametresine göre gösterilir (Html.Tutar yardımcısı).
                    var basamak = await _parametreService.GetOndalikBasamakAsync();
                    viewData["OndalikBasamak"] = basamak;
                    viewData["TutarBicimi"] = "N" + basamak;
                }
                catch (Exception ex)
                {
                    // Parametreler okunamasa da sayfa açılsın (ör. veritabanı hatasında hata sayfasının kendisi).
                    // View'ler bu değerler yoksa varsayılanları kullanır.
                    _logger.LogWarning(ex, "Görünüm parametreleri okunamadı.");
                }
            }

            await next();
        }
    }
}
