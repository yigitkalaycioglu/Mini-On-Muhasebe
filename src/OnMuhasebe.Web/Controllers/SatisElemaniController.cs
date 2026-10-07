using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class SatisElemaniController : Controller
    {
        private readonly ISatisElemaniService _satisElemaniService;
        public SatisElemaniController(ISatisElemaniService satisElemaniService)
        {
            _satisElemaniService = satisElemaniService;
        }

        public async Task<IActionResult> Index()
        {
            var satisElemanlari = await _satisElemaniService.GetAllSatisElemanlariAsync();
            return View(satisElemanlari);
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var satisElemani = await _satisElemaniService.GetSatisElemaniByIdAsync(id);
            if (satisElemani == null)
            {
                return NotFound();
            }
            return View(new SatisElemaniDuzenleViewModel { Id = id, Form = SatisElemaniDto.FromEntity(satisElemani) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(SatisElemaniDto satisElemani)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _satisElemaniService.CreateSatisElemaniAsync(satisElemani);
                    this.BasariMesaji("Satış elemanı kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            return View("Create", satisElemani);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var silindi = await _satisElemaniService.DeleteSatisElemaniAsync(id);
            if (silindi)
            {
                this.BasariMesaji("Satış elemanı silindi.");
            }
            else
            {
                this.UyariMesaji("Bu satış elemanının faturaları olduğu için silinemedi; bunun yerine pasife alındı.");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, SatisElemaniDto satisElemani)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _satisElemaniService.UpdateSatisElemaniAsync(id, satisElemani);
                    this.BasariMesaji("Satış elemanı güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            return View("Edit", new SatisElemaniDuzenleViewModel { Id = id, Form = satisElemani });
        }
    }
}
