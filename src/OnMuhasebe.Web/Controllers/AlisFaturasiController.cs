using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class AlisFaturasiController : Controller
    {
        private readonly IAlisFaturasiService _alisFaturasiService;
        private readonly ICariService _cariService;
        private readonly IStokKartiService _stokKartiService;
        private readonly IParametreService _parametreService;
        public AlisFaturasiController(IAlisFaturasiService alisFaturasiService, ICariService cariService, IStokKartiService stokKartiService, IParametreService parametreService)
        {
            _alisFaturasiService = alisFaturasiService;
            _cariService = cariService;
            _stokKartiService = stokKartiService;
            _parametreService = parametreService;
        }

        public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis)
        {
            var alisFaturalari = await _alisFaturasiService.GetAllAlisFaturalariAsync(baslangic, bitis);
            return View(alisFaturalari);
        }

        public async Task<IActionResult> Create()
        {
            await DropdownListeleriniDoldurAsync();
            ViewData["YeniFaturaNo"] = await _alisFaturasiService.GetYeniFaturaNoAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(AlisFaturasi alisFaturasi)
        {
            // Formdan gelmeyen alanlar nullable yüzünden örtük [Required] sayılır; doğrulamadan çıkar.
            ModelState.Remove(nameof(AlisFaturasi.FaturaNo));
            ModelState.Remove(nameof(AlisFaturasi.Cari));
            ModelState.Remove(nameof(AlisFaturasi.Kullanici));
            for (var i = 0; i < alisFaturasi.AlisFaturaSatirlari.Count; i++)
            {
                ModelState.Remove($"AlisFaturaSatirlari[{i}].AlisFaturasi");
                ModelState.Remove($"AlisFaturaSatirlari[{i}].StokKarti");
            }

            if (alisFaturasi.AlisFaturaSatirlari.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Faturaya en az bir kalem ekleyin.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _alisFaturasiService.CreateAlisFaturasiAsync(alisFaturasi, User.KullaniciId());
                    this.BasariMesaji($"{alisFaturasi.FaturaNo} numaralı fatura kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            await DropdownListeleriniDoldurAsync();
            return View("Create", alisFaturasi);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var alisFaturasi = await _alisFaturasiService.GetAlisFaturasiByIdAsync(id);
            if (alisFaturasi == null)
            {
                return NotFound();
            }
            return View(alisFaturasi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                await _alisFaturasiService.DeleteAlisFaturasiAsync(id);
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
            ViewData["Cariler"] = tumCariler.Where(c => c.Aktif && _cariService.TedarikciMi(c));

            var tumStoklar = await _stokKartiService.GetAllStokKartlariAsync();
            ViewData["Stoklar"] = tumStoklar.Where(s => s.Aktif);

            ViewData["VarsayilanKdv"] = await _parametreService.GetSayiAsync(Sabitler.ParamVarsayilanKdvOrani);
        }
    }
}
