using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            // Trong thực tế sẽ lấy cart items từ session/database
            var model = new CheckoutViewModel
            {
                CartItems = new List<CartItem>
                {
                    new CartItem { ProductId = 1, ProductName = "Espresso Arabica", UnitPrice = 185000, Quantity = 2, ImageUrl = "/images/products/espresso.jpg" },
                    new CartItem { ProductId = 3, ProductName = "Cà Phê Phin Truyền Thống", UnitPrice = 95000, Quantity = 1, ImageUrl = "/images/products/phin.jpg" }
                }
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Trong thực tế sẽ lưu đơn hàng vào database
            TempData["OrderSuccess"] = true;
            TempData["OrderId"] = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.OrderId = TempData["OrderId"];
            return View();
        }
    }
}
