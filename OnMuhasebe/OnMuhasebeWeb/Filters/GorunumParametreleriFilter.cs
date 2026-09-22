using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnMuhasebe.Business;
using OnMuhasebe.Business.Services.IServices;

namespace OnMuhasebeWeb.Filters
{
    /// <summary>
    /// Her sayfada gereken görüntü parametrelerini (firma bilgisi, para birimi) ViewData'ya koyar.
    /// Böylece her controller aynı değerleri ayrı ayrı okumak zorunda kalmaz.
    /// </summary>
    public class GorunumParametreleriFilter : IAsyncResultFilter
    {
        private readonly IParametreService _parametreService;
        public GorunumParametreleriFilter(IParametreService parametreService)
        {
            _parametreService = parametreService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ViewResult viewResult)
            {
                var viewData = viewResult.ViewData;
                viewData["FirmaUnvani"] = await _parametreService.GetDegerAsync(Sabitler.ParamFirmaUnvani);
                viewData["VergiDairesiNo"] = await _parametreService.GetDegerAsync(Sabitler.ParamVergiDairesiNo);
                viewData["ParaBirimi"] = await _parametreService.GetDegerAsync(Sabitler.ParamParaBirimi);
            }

            await next();
        }
    }
}
