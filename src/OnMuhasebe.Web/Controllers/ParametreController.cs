using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Web.Extensions;

namespace OnMuhasebe.Web.Controllers
{
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
            return View(parametreler.Select(ParametreDegeriDto.FromEntity).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(List<ParametreDegeriDto> parametreler)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _parametreService.UpdateParametrelerAsync(parametreler);
                    this.BasariMesaji("Parametreler kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            return View("Index", parametreler);
        }
    }
}
