using Microsoft.AspNetCore.Mvc;
using OnMuhasebe.Models;
using System.Diagnostics;

namespace OnMuhasebeWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

    }
}
