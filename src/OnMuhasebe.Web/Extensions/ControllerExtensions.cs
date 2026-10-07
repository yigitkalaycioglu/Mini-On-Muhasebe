using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnMuhasebe.Application.Exceptions;

namespace OnMuhasebe.Web.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>Bir sonraki sayfada yeşil başarı kutusunda gösterilecek mesaj.</summary>
        public static void BasariMesaji(this Controller controller, string mesaj) =>
            MesajKoy(controller, mesaj, IslemMesaji.Basari);

        /// <summary>Bir sonraki sayfada sarı uyarı kutusunda gösterilecek mesaj.</summary>
        public static void UyariMesaji(this Controller controller, string mesaj) =>
            MesajKoy(controller, mesaj, IslemMesaji.Uyari);

        /// <summary>İş kuralı hatasını forma ekler: alanı belliyse o alanın altında, değilse formun üstündeki özette.</summary>
        public static void HataEkle(this ModelStateDictionary modelState, IsKuraliException hata) =>
            modelState.AddModelError(hata.Alan ?? string.Empty, hata.Message);

        private static void MesajKoy(Controller controller, string mesaj, string tip)
        {
            controller.TempData[IslemMesaji.Anahtar] = mesaj;
            controller.TempData[IslemMesaji.TipAnahtari] = tip;
        }
    }
}
