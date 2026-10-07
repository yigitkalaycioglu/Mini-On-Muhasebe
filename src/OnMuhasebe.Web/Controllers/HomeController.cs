using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;

namespace OnMuhasebe.Web.Controllers
{
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

        // Beklenmeyen hatalar ve 404 gibi durum kodları buraya yönlendirilir (Program.cs).
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Hata(int? kod)
        {
            ViewData["Kod"] = kod ?? 500;
            return View();
        }
    }
}
