using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business;
using OnMuhasebe.Business.Services.IServices;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IStokKartiService _stokKartiService;
        private readonly IParametreService _parametreService;
        public HomeController(IStokKartiService stokKartiService, IParametreService parametreService)
        {
            _stokKartiService = stokKartiService;
            _parametreService = parametreService;
        }

        public async Task<IActionResult> Index()
        {
            // "Kritik Stok Uyarısı" parametresi açıksa panelde uyarı gösterilir.
            if (await _parametreService.AcikMiAsync(Sabitler.ParamKritikStokUyarisi))
            {
                ViewData["KritikStokSayisi"] = await _stokKartiService.GetKritikStokSayisiAsync();
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
