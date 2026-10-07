using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = Sabitler.RolYonetici)]
    public class ParametreController : Controller
    {
        private readonly IParametreService _parametreService;
        public ParametreController(IParametreService parametreService)
        {
            _parametreService = parametreService;
        }

        public async Task<IActionResult> Index()
        {
            var parametreler = await _parametreService.GetAllParametrelerAsync();
            return View(parametreler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(List<Parametre> parametreler)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _parametreService.UpdateParametrelerAsync(parametreler);
                    TempData["Mesaj"] = "Parametreler kaydedildi.";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
            }

            return View("Index", parametreler);
        }
    }
}
