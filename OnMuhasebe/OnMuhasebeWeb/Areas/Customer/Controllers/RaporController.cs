using Microsoft.AspNetCore.Mvc;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class RaporController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
