using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class StokKartiController : Controller
    {
        private readonly IStokKartiService _stokKartiService;
        private readonly IParametreService _parametreService;
        public StokKartiController(IStokKartiService stokKartiService, IParametreService parametreService)
        {
            _stokKartiService = stokKartiService;
            _parametreService = parametreService;
        }

        public async Task<IActionResult> Index()
        {
            // Mevcut miktarlar hareketler belleğe alınmadan veritabanında hesaplanır.
            var bakiyeler = await _stokKartiService.GetStokBakiyeleriAsync();

            // "Kritik Stok Uyarısı" parametresi kapalıysa uyarı ve etiketler gösterilmez.
            var uyariAcik = await _parametreService.AcikMiAsync(Sabitler.ParamKritikStokUyarisi);

            var model = new StokListesiViewModel
            {
                Satirlar = bakiyeler.Select(b => new StokSatiriViewModel
                {
                    Stok = b.Stok,
                    Mevcut = b.Mevcut,
                    Kritik = uyariAcik && _stokKartiService.KritikSeviyede(b)
                }).ToList()
            };
            model.KritikSayisi = model.Satirlar.Count(x => x.Stok.Aktif && x.Kritik);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            // Yeni kartta KDV oranı "Varsayılan KDV Oranı" parametresinden gelir.
            var kdv = await _parametreService.GetSayiAsync(Sabitler.ParamVarsayilanKdvOrani);
            ViewData["VarsayilanKdv"] = kdv.ToString("0.##");
            return View();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var stokKarti = await _stokKartiService.GetStokKartiByIdAsync(id);
            if (stokKarti == null)
            {
                return NotFound();
            }

            return View(await DuzenlemeModeliAsync(id, StokKartiDto.FromEntity(stokKarti), stokKarti));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(StokKartiDto stokKarti)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _stokKartiService.CreateStokKartiAsync(stokKarti);
                    this.BasariMesaji("Stok kartı kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }
            return View("Create", stokKarti);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int id, StokKartiDto stokKarti)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _stokKartiService.UpdateStokKartiAsync(id, stokKarti);
                    this.BasariMesaji("Stok kartı güncellendi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            // Form girilen değerlerle yeniden gösterilir; stok özeti kayıtlı karttan hesaplanır.
            var kayitli = await _stokKartiService.GetStokKartiByIdAsync(id);
            if (kayitli == null)
            {
                return NotFound();
            }
            return View("Edit", await DuzenlemeModeliAsync(id, stokKarti, kayitli));
        }

        private async Task<StokKartiDuzenleViewModel> DuzenlemeModeliAsync(int id, StokKartiDto form, StokKarti kayitli) => new()
        {
            Id = id,
            Form = form,
            Ozet = new StokOzetiViewModel
            {
                ToplamGiris = _stokKartiService.ToplamGiris(kayitli),
                ToplamCikis = _stokKartiService.ToplamCikis(kayitli),
                Mevcut = _stokKartiService.MevcutMiktar(kayitli),
                Kritik = await _parametreService.AcikMiAsync(Sabitler.ParamKritikStokUyarisi)
                    && _stokKartiService.KritikSeviyede(kayitli)
            }
        };

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var silindi = await _stokKartiService.DeleteStokKartiAsync(id);
            if (silindi)
            {
                this.BasariMesaji("Stok kartı silindi.");
            }
            else
            {
                this.UyariMesaji("Bu stok kartının fatura veya hareket kayıtları olduğu için silinemedi; bunun yerine pasife alındı.");
            }

            return RedirectToAction("Index");
        }
    }
}
