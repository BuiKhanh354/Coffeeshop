using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;

namespace CoffeeShop.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        private int? GetCurrentUserId()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            }
            return null;
        }

        private string? GetSessionId()
        {
            return HttpContext.Session.GetString("CartSessionId");
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var sessionId = userId.HasValue ? null : GetSessionId();

            var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
            var cartItems = cart.CartItems?.ToList() ?? new List<CartItem>();

            if (cartItems.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                CartItems = cartItems.Select(ci => new CartItem
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Product = ci.Product
                }).ToList()
            };

            // Pre-fill user info if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                model.FullName = User.Identity.Name ?? "";
                model.Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload cart items
                var userId = GetCurrentUserId();
                var sessionId = userId.HasValue ? null : GetSessionId();
                var cart = await _cartService.GetOrCreateCartAsync(userId, sessionId);
                model.CartItems = cart.CartItems?.Select(ci => new CartItem
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Product = ci.Product
                }).ToList() ?? new List<CartItem>();

                return View("Index", model);
            }

            var currentUserId = GetCurrentUserId();
            var currentSessionId = currentUserId.HasValue ? null : GetSessionId();
            var currentCart = await _cartService.GetOrCreateCartAsync(currentUserId, currentSessionId);
            var currentCartItems = currentCart.CartItems?.ToList() ?? new List<CartItem>();

            if (currentCartItems.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Calculate totals
            var subTotal = currentCartItems.Sum(ci => ci.TotalPrice);
            var shippingFee = 30000m;
            var totalAmount = subTotal + shippingFee;

            // Create order
            var order = new Order
            {
                UserId = currentUserId,
                CustomerName = model.FullName,
                CustomerPhone = model.Phone,
                CustomerEmail = model.Email,
                ShippingAddress = model.Address,
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                Discount = 0,
                TotalAmount = totalAmount,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = "Pending",
                OrderStatus = "New",
                Note = model.Note
            };

            var orderDetails = currentCartItems.Select(ci => new OrderDetail
            {
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? "",
                UnitPrice = ci.Product?.Price ?? 0,
                Quantity = ci.Quantity,
                TotalPrice = ci.TotalPrice
            }).ToList();

            var createdOrder = await _orderService.CreateAsync(order, orderDetails);

            // Create payment record
            var payment = new Payment
            {
                OrderId = createdOrder.Id,
                Amount = totalAmount,
                PaymentMethod = model.PaymentMethod,
                Status = "Pending"
            };
            await _paymentService.CreateAsync(payment);

            // Clear cart
            await _cartService.ClearCartAsync(currentCart.Id);

            TempData["OrderSuccess"] = true;
            TempData["OrderId"] = createdOrder.OrderCode;

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
