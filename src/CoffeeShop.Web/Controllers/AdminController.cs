using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            // Demo statistics
            ViewBag.TotalOrders = 156;
            ViewBag.TodayRevenue = 12500000;
            ViewBag.TotalProducts = 48;
            ViewBag.TotalCustomers = 234;
            ViewBag.PendingOrders = 12;
            ViewBag.ProcessingOrders = 8;

            return View();
        }

        public IActionResult Products()
        {
            return View();
        }

        public IActionResult Orders()
        {
            return View();
        }

        public IActionResult Customers()
        {
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }
    }
}
