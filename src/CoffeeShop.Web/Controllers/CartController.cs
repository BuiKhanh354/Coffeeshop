using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Controllers
{
    public class CartController : Controller
    {
        // Sử dụng static để giả lập session storage (trong thực tế sẽ dùng Session hoặc Database)
        private static List<CartItem> _cartItems = new();

        public IActionResult Index()
        {
            ViewBag.SubTotal = _cartItems.Sum(x => x.TotalPrice);
            ViewBag.ShippingFee = _cartItems.Count > 0 ? 30000m : 0m;
            ViewBag.Total = ViewBag.SubTotal + ViewBag.ShippingFee;
            return View(_cartItems);
        }

        [HttpPost]
        public IActionResult Add(int productId, string productName, string imageUrl, decimal unitPrice, int quantity = 1)
        {
            var existingItem = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                _cartItems.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    ImageUrl = imageUrl,
                    UnitPrice = unitPrice,
                    Quantity = quantity
                });
            }

            return Json(new { success = true, cartCount = _cartItems.Sum(x => x.Quantity) });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _cartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            return Json(new { 
                success = true, 
                cartCount = _cartItems.Sum(x => x.Quantity),
                subTotal = _cartItems.Sum(x => x.TotalPrice),
                total = _cartItems.Sum(x => x.TotalPrice) + (_cartItems.Count > 0 ? 30000 : 0)
            });
        }

        [HttpPost]
        public IActionResult Remove(int productId)
        {
            var item = _cartItems.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
            }

            return Json(new { 
                success = true, 
                cartCount = _cartItems.Sum(x => x.Quantity),
                subTotal = _cartItems.Sum(x => x.TotalPrice),
                total = _cartItems.Sum(x => x.TotalPrice) + (_cartItems.Count > 0 ? 30000 : 0)
            });
        }

        [HttpGet]
        public IActionResult GetCount()
        {
            return Json(new { count = _cartItems.Sum(x => x.Quantity) });
        }

        [HttpGet]
        public IActionResult GetItems()
        {
            return Json(_cartItems);
        }
    }
}
