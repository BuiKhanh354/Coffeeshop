using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;

namespace CoffeeShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        private int? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }
            return null;
        }

        private string GetOrCreateSessionId()
        {
            var sessionId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", sessionId);
            }
            return sessionId;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            var cartItems = cart.CartItems?.ToList() ?? new List<CartItem>();

            var subTotal = cartItems.Sum(x => x.TotalPrice);
            var shippingFee = cartItems.Count > 0 ? 30000m : 0m;

            ViewBag.SubTotal = subTotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subTotal + shippingFee;

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            await _cartService.AddToCartAsync(cart.Id, productId, quantity);

            var cartCount = await _cartService.GetCartItemCountAsync(userId, sessionId);

            return Json(new { success = true, cartCount });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            await _cartService.UpdateQuantityAsync(cartItemId, quantity);

            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            var cartItems = cart.CartItems?.ToList() ?? new List<CartItem>();

            var subTotal = cartItems.Sum(x => x.TotalPrice);
            var cartCount = cartItems.Sum(x => x.Quantity);

            return Json(new
            {
                success = true,
                cartCount,
                subTotal,
                total = subTotal + (cartItems.Count > 0 ? 30000 : 0)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            await _cartService.RemoveFromCartAsync(cartItemId);

            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            var cartItems = cart.CartItems?.ToList() ?? new List<CartItem>();

            var subTotal = cartItems.Sum(x => x.TotalPrice);
            var cartCount = cartItems.Sum(x => x.Quantity);

            return Json(new
            {
                success = true,
                cartCount,
                subTotal,
                total = subTotal + (cartItems.Count > 0 ? 30000 : 0)
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var count = await _cartService.GetCartItemCountAsync(userId, sessionId);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetOrCreateSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            var cartItems = cart.CartItems?.Select(ci => new
            {
                ci.Id,
                ci.ProductId,
                ProductName = ci.Product?.Name ?? "",
                ImageUrl = ci.Product?.ImageUrl ?? "",
                UnitPrice = ci.Product?.Price ?? 0,
                ci.Quantity,
                TotalPrice = (ci.Product?.Price ?? 0) * ci.Quantity
            }).ToList();

            return Json(cartItems);
        }
    }
}
