using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Security;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class StokHareketController : Controller
    {
        private readonly IStokHareketService _stokHareketService;
        private readonly IStokKartiService _stokKartiService;
        public StokHareketController(IStokHareketService stokHareketService, IStokKartiService stokKartiService)
        {
            _stokHareketService = stokHareketService;
            _stokKartiService = stokKartiService;
        }

        public async Task<IActionResult> Index(int? stokId, DateTime? baslangic, DateTime? bitis)
        {
            var hareketler = await _stokHareketService.GetAllStokHareketleriAsync(stokId, baslangic, bitis);

            var model = new StokHareketListesiViewModel
            {
                Satirlar = hareketler.Select(h => new StokHareketSatiriViewModel
                {
                    Hareket = h,
                    TipAdi = _stokKartiService.HareketTipiAdi(h.HareketTipi),
                    TipSinifi = TipSinifi(h.HareketTipi),
                    GirisMiktari = h.Yon == Sabitler.YonGiris ? h.Miktar : null,
                    CikisMiktari = h.Yon == Sabitler.YonCikis ? h.Miktar : null,
                    SayimFisi = h.HareketTipi == Sabitler.HareketSayimFazlasi || h.HareketTipi == Sabitler.HareketSayimEksigi
                }).ToList()
            };
            model.ToplamGiris = model.Satirlar.Sum(x => x.GirisMiktari ?? 0);
            model.ToplamCikis = model.Satirlar.Sum(x => x.CikisMiktari ?? 0);

            if (stokId.HasValue)
            {
                model.FiltreliStok = await _stokKartiService.GetStokKartiByIdAsync(stokId.Value);
            }

            return View(model);
        }

        // Rozet rengi: giriş hareketleri yeşil/sarı, çıkış hareketleri mavi/kırmızı.
        private static string TipSinifi(string hareketTipi) => hareketTipi switch
        {
            Sabitler.HareketAlis => "badge-success",
            Sabitler.HareketSatis => "badge-brand",
            Sabitler.HareketSayimFazlasi => "badge-warning",
            Sabitler.HareketSayimEksigi => "badge-danger",
            _ => ""
        };

        public async Task<IActionResult> SayimFisi()
        {
            await StoklariDoldurAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("SayimFisi")]
        public async Task<IActionResult> SayimFisiPost(StokHareket sayimFisi)
        {
            // Yon, BelgeNo ve KullaniciId sunucuda atanır; navigation'lar formdan gelmez.
            ModelState.Remove(nameof(StokHareket.Yon));
            ModelState.Remove(nameof(StokHareket.StokKarti));
            ModelState.Remove(nameof(StokHareket.Kullanici));

            if (ModelState.IsValid)
            {
                try
                {
                    await _stokHareketService.CreateSayimFisiAsync(sayimFisi, User.KullaniciId());
                    TempData["Mesaj"] = $"{sayimFisi.BelgeNo} numaralı {_stokKartiService.HareketTipiAdi(sayimFisi.HareketTipi).ToLower()} fişi kaydedildi.";
                    TempData["MesajTipi"] = "success";
                    return RedirectToAction("Index");
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            await StoklariDoldurAsync();
            return View("SayimFisi", sayimFisi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                await _stokHareketService.DeleteSayimFisiAsync(id);
                TempData["Mesaj"] = "Sayım fişi silindi; stok miktarı geri alındı.";
                TempData["MesajTipi"] = "success";
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                TempData["Mesaj"] = ex.Message;
                TempData["MesajTipi"] = "warning";
            }

            return RedirectToAction("Index");
        }

        private async Task StoklariDoldurAsync()
        {
            // Fiş ekranında seçilen ürünün mevcut miktarı ve sayım sonrası miktar gösterilir.
            var bakiyeler = await _stokKartiService.GetStokBakiyeleriAsync(yalnizcaAktif: true);
            ViewData["Stoklar"] = bakiyeler
                .Select(b => new SayimFisiStokViewModel
                {
                    Stok = b.Stok,
                    Mevcut = b.Mevcut
                })
                .ToList();
        }
    }
}
