using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnMuhasebe.Application.Exceptions;

namespace OnMuhasebe.Web.Filters
{
    /// <summary>
    /// Serviste bulunamayan kayıt (KayitBulunamadiException) her action'da ayrı ayrı yakalanmak yerine
    /// burada 404 yanıtına çevrilir; kullanıcı uygulamanın "sayfa bulunamadı" ekranını görür.
    /// </summary>
    public class KayitBulunamadiFiltresi : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is KayitBulunamadiException)
            {
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
            }
        }
    }
}
