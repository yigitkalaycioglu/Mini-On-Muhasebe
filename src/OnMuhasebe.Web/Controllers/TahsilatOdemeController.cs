using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnMuhasebe.Application.Exceptions;
using OnMuhasebe.Application.Services;
using OnMuhasebe.Domain;
using OnMuhasebe.Domain.Entities;
using OnMuhasebe.Web.Extensions;
using OnMuhasebe.Web.Security;
using OnMuhasebe.Web.ViewModels;

namespace OnMuhasebe.Web.Controllers
{
    [Authorize]
    public class TahsilatOdemeController : Controller
    {
        private readonly ITahsilatOdemeService _tahsilatOdemeService;
        private readonly ICariService _cariService;
        public TahsilatOdemeController(ITahsilatOdemeService tahsilatOdemeService, ICariService cariService)
        {
            _tahsilatOdemeService = tahsilatOdemeService;
            _cariService = cariService;
        }

        public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis)
        {
            var hareketler = await _tahsilatOdemeService.GetAllTahsilatOdemelerAsync(baslangic, bitis);

            var model = new TahsilatOdemeListesiViewModel
            {
                Satirlar = hareketler.Select(h => new TahsilatOdemeSatiriViewModel
                {
                    Hareket = h,
                    IslemAdi = _cariService.IslemTipiAdi(h.IslemTipi),
                    Tahsilat = h.IslemTipi == Sabitler.IslemTahsilat,
                    // Tahsilat alacak, ödeme borç olarak işlenir; tutar hangisi doluysa odur.
                    Tutar = h.IslemTipi == Sabitler.IslemTahsilat ? h.Alacak : h.Borc
                }).ToList()
            };
            model.ToplamTahsilat = model.Satirlar.Where(x => x.Tahsilat).Sum(x => x.Tutar);
            model.ToplamOdeme = model.Satirlar.Where(x => !x.Tahsilat).Sum(x => x.Tutar);
            model.Net = model.ToplamTahsilat - model.ToplamOdeme;

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            await CarileriDoldurAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreatePost(CariHareket cariHareket, decimal? tutar)
        {
            // Borc/Alacak tutardan, BelgeNo ve KullaniciId serviste atanır; navigation'lar formdan gelmez.
            ModelState.Remove(nameof(CariHareket.Cari));
            ModelState.Remove(nameof(CariHareket.Kullanici));

            // Boş tutar hata üretmeden null gelir; geçersiz metin ise zaten bağlama hatası olarak işaretlidir.
            if (tutar == null && ModelState.GetFieldValidationState("tutar") != ModelValidationState.Invalid)
            {
                ModelState.AddModelError("tutar", "Tutar zorunludur.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _tahsilatOdemeService.CreateTahsilatOdemeAsync(cariHareket, tutar!.Value, User.KullaniciId());
                    this.BasariMesaji($"{cariHareket.BelgeNo} numaralı {_cariService.IslemTipiAdi(cariHareket.IslemTipi).ToLower()} kaydedildi.");
                    return RedirectToAction("Index");
                }
                catch (IsKuraliException ex)
                {
                    ModelState.HataEkle(ex);
                }
            }

            await CarileriDoldurAsync();
            return View("Create", cariHareket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                await _tahsilatOdemeService.DeleteTahsilatOdemeAsync(id);
                this.BasariMesaji("Kayıt silindi; cari bakiye güncellendi.");
            }
            catch (IsKuraliException ex)
            {
                this.UyariMesaji(ex.Message);
            }

            return RedirectToAction("Index");
        }

        private async Task CarileriDoldurAsync()
        {
            // Ekran seçilen işleme göre listeyi süzer: tahsilat müşteriden, ödeme tedarikçiye.
            var cariler = await _cariService.GetAllCarilerAsync();
            ViewData["Cariler"] = cariler
                .Where(c => c.Aktif)
                .OrderBy(c => c.CariKodu)
                .Select(c => new TahsilatOdemeCariViewModel
                {
                    Cari = c,
                    UygunIslemler = string.Join(' ', new[]
                    {
                        _cariService.MusteriMi(c) ? Sabitler.IslemTahsilat : null,
                        _cariService.TedarikciMi(c) ? Sabitler.IslemOdeme : null
                    }.Where(x => x != null))
                })
                .ToList();
        }
    }
}
