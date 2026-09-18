using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class AlisFaturasiController : Controller
    {
        private readonly IAlisFaturasiService _alisFaturasiService;
        private readonly ICariService _cariService;
        private readonly IStokKartiService _stokKartiService;
        public AlisFaturasiController(IAlisFaturasiService alisFaturasiService, ICariService cariService, IStokKartiService stokKartiService)
        {
            _alisFaturasiService = alisFaturasiService;
            _cariService = cariService;
            _stokKartiService = stokKartiService;
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
            if (ModelState.IsValid)
            {
                try
                {
                    // TODO: Login sistemi kurulunca oturumdaki kullanıcıdan alınacak.
                    int kullaniciId = 1;

                    await _alisFaturasiService.CreateAlisFaturasiAsync(alisFaturasi, kullaniciId);
                    TempData["Mesaj"] = $"{alisFaturasi.FaturaNo} numaralı fatura kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
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
                TempData["Mesaj"] = "Fatura silindi; stok ve cari hareketleri geri alındı.";
                TempData["MesajTipi"] = "success";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction("Index");
        }

        private async Task DropdownListeleriniDoldurAsync()
        {
            var tumCariler = await _cariService.GetAllCarilerAsync();
            ViewData["Cariler"] = tumCariler.Where(c => c.Aktif && (c.CariTipi == 2 || c.CariTipi == 3));

            var tumStoklar = await _stokKartiService.GetAllStokKartlariAsync();
            ViewData["Stoklar"] = tumStoklar.Where(s => s.Aktif);
        }
    }
}
