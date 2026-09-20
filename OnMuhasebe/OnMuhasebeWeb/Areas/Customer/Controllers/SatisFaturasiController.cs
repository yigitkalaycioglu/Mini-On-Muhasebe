using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Business.Services.IServices;
using OnMuhasebe.Models;
using System.Security.Claims;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class SatisFaturasiController : Controller
    {
        private readonly ISatisFaturasiService _satisFaturasiService;
        private readonly ISatisElemaniService _satisElemaniService;
        private readonly ICariService _cariService;
        private readonly IStokKartiService _stokKartiService;
        public SatisFaturasiController(ISatisFaturasiService satisFaturasiService, ICariService cariService, IStokKartiService stokKartiService, ISatisElemaniService satisElemaniService)
        {
            _satisFaturasiService = satisFaturasiService;
            _cariService = cariService;
            _satisElemaniService = satisElemaniService;
            _stokKartiService = stokKartiService;
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
        public async Task<IActionResult> CreatePost(SatisFaturasi satisFaturasi)
        {
            // Formdan gelmeyen alanlar nullable yüzünden örtük [Required] sayılır; doğrulamadan çıkar.
            ModelState.Remove(nameof(SatisFaturasi.FaturaNo));
            ModelState.Remove(nameof(SatisFaturasi.Cari));
            ModelState.Remove(nameof(SatisFaturasi.SatisElemani));
            ModelState.Remove(nameof(SatisFaturasi.Kullanici));
            for (var i = 0; i < satisFaturasi.SatisFaturaSatirlari.Count; i++)
            {
                ModelState.Remove($"SatisFaturaSatirlari[{i}].SatisFaturasi");
                ModelState.Remove($"SatisFaturaSatirlari[{i}].StokKarti");
            }

            if (satisFaturasi.SatisFaturaSatirlari.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Faturaya en az bir kalem ekleyin.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int kullaniciId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                    await _satisFaturasiService.CreateSatisFaturasiAsync(satisFaturasi, kullaniciId);
                    TempData["Mesaj"] = $"{satisFaturasi.FaturaNo} numaralı fatura kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await DropdownListeleriniDoldurAsync();
            return View("Create", satisFaturasi);
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
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                await _satisFaturasiService.DeleteSatisFaturasiAsync(id);
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
            ViewData["Cariler"] = tumCariler.Where(c => c.Aktif && (c.CariTipi == 1 || c.CariTipi == 3));

            var tumSatisElemanlari = await _satisElemaniService.GetAllSatisElemanlariAsync();
            ViewData["SatisElemanlari"] = tumSatisElemanlari.Where(e => e.Aktif);

            var tumStoklar = await _stokKartiService.GetAllStokKartlariAsync();
            ViewData["Stoklar"] = tumStoklar.Where(s => s.Aktif);
        }
    }
}
