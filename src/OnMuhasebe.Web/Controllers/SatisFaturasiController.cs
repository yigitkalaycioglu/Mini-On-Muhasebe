using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Dtos;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class SatisFaturasiController : Controller
    {
        private readonly ISatisFaturasiService _satisFaturasiService;
        private readonly ISatisElemaniService _satisElemaniService;
        private readonly ICariService _cariService;
        private readonly IStokKartiService _stokKartiService;
        private readonly IParametreService _parametreService;
        public SatisFaturasiController(ISatisFaturasiService satisFaturasiService, ICariService cariService, IStokKartiService stokKartiService, ISatisElemaniService satisElemaniService, IParametreService parametreService)
        {
            _satisFaturasiService = satisFaturasiService;
            _cariService = cariService;
            _satisElemaniService = satisElemaniService;
            _stokKartiService = stokKartiService;
            _parametreService = parametreService;
        }
        
        public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis)
        {
            var satisFaturalari = await _satisFaturasiService.GetAllSatisFaturalariAsync(baslangic, bitis);
            return View(satisFaturalari);
        }

        public async Task<IActionResult> Create()
        {
            await DropdownListeleriniDoldurAsync();
            ViewData["YeniFaturaNo"] = await _satisFaturasiService.GetYeniFaturaNoAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(SatisFaturasiDto fatura)
        {
            if (fatura.Kalemler.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Faturaya en az bir kalem ekleyin.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kayit = await _satisFaturasiService.CreateSatisFaturasiAsync(fatura, User.KullaniciId());
                    this.BasariMesaji($"{kayit.FaturaNo} numaralı fatura kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            await DropdownListeleriniDoldurAsync();
            return View("Create", fatura);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var satisFaturasi = await _satisFaturasiService.GetSatisFaturasiByIdAsync(id);
            if (satisFaturasi == null)
            {
                return NotFound();
            }
            return View(satisFaturasi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost([FromRoute] int id)
        {
            try
            {
                await _satisFaturasiService.DeleteSatisFaturasiAsync(id);
                this.BasariMesaji("Fatura silindi; stok ve cari hareketleri geri alındı.");
            }
            catch (IsKuraliException ex)
            {
                this.UyariMesaji(ex.Message);
                return RedirectToAction("Detay", new { id });
            }

            return RedirectToAction("Index");
        }

        private async Task DropdownListeleriniDoldurAsync()
        {
            var tumCariler = await _cariService.GetAllCarilerAsync();
            ViewData["Cariler"] = tumCariler.Where(c => c.Aktif && _cariService.MusteriMi(c));

            var tumSatisElemanlari = await _satisElemaniService.GetAllSatisElemanlariAsync();
            ViewData["SatisElemanlari"] = tumSatisElemanlari.Where(e => e.Aktif);

            var tumStoklar = await _stokKartiService.GetAllStokKartlariAsync();
            ViewData["Stoklar"] = tumStoklar.Where(s => s.Aktif);

            ViewData["VarsayilanKdv"] = await _parametreService.GetSayiAsync(Sabitler.ParamVarsayilanKdvOrani);
        }
    }
}
