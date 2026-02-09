using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;
using CoffeeShop.Web.Services.PaymentProcessing;

namespace CoffeeShop.Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPaymentProcessorFactory _paymentFactory;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPaymentProcessorFactory paymentFactory,
            ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _paymentService = paymentService;
            _paymentFactory = paymentFactory;
            _logger = logger;
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
            _logger.LogInformation("=== PlaceOrder được gọi ===");
            _logger.LogInformation("PaymentMethod: {PaymentMethod}", model.PaymentMethod);
            _logger.LogInformation("FullName: {FullName}, Phone: {Phone}", model.FullName, model.Phone);

            var currentUserId = GetCurrentUserId();
            var currentSessionId = currentUserId.HasValue ? null : GetSessionId();

            try
            {
                // Log ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("❌ ModelState không hợp lệ!");
                    foreach (var error in ModelState)
                    {
                        foreach (var e in error.Value.Errors)
                        {
                            _logger.LogWarning("  - Field: {Field}, Error: {Error}", error.Key, e.ErrorMessage);
                        }
                    }

                    // Reload cart items
                    var cart = await _cartService.GetOrCreateCartAsync(currentUserId, currentSessionId);
                    model.CartItems = cart.CartItems?.Select(ci => new CartItem
                    {
                        Id = ci.Id,
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        Product = ci.Product
                    }).ToList() ?? new List<CartItem>();

                    TempData["Error"] = "Vui lòng kiểm tra lại thông tin đặt hàng.";
                    return View("Index", model);
                }

                _logger.LogInformation("✅ ModelState hợp lệ, tiếp tục xử lý...");

                // ========================================
                // Lấy payment processor từ factory
                // ========================================
                if (!_paymentFactory.IsSupported(model.PaymentMethod))
                {
                    TempData["Error"] = $"Phương thức thanh toán '{model.PaymentMethod}' không được hỗ trợ.";
                    return RedirectToAction("Index");
                }

                var processor = _paymentFactory.GetProcessor(model.PaymentMethod);
                var paymentStatus = processor.InitialPaymentStatus;
                
                _logger.LogInformation("Sử dụng processor: {Processor}, InitialStatus: {Status}", 
                    processor.PaymentMethod, paymentStatus);

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

                // Create order with status from processor
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
                    PaymentStatus = paymentStatus,
                    OrderStatus = "Pending",
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

                // Create order (with stock validation inside transaction)
                var createdOrder = await _orderService.CreateAsync(order, orderDetails);

                // Create payment record
                var payment = new Payment
                {
                    OrderId = createdOrder.Id,
                    Amount = totalAmount,
                    PaymentMethod = model.PaymentMethod,
                    Status = paymentStatus
                };
                await _paymentService.CreateAsync(payment);

                // ========================================
                // Xử lý thanh toán qua processor
                // ========================================
                var result = await processor.ProcessAsync(createdOrder, payment);

                if (!result.Success)
                {
                    TempData["Error"] = result.ErrorMessage ?? "Đã có lỗi xảy ra khi xử lý thanh toán.";
                    return RedirectToAction("Index");
                }

                // Nếu có redirect URL (MoMo, VNPay...), redirect đến đó
                if (!string.IsNullOrEmpty(result.RedirectUrl))
                {
                    // Clear cart trước khi redirect
                    await _cartService.ClearCartAsync(currentCart.Id);
                    return Redirect(result.RedirectUrl);
                }

                // Không có redirect (COD) - success ngay
                await _cartService.ClearCartAsync(currentCart.Id);

                TempData["OrderSuccess"] = true;
                TempData["OrderCode"] = createdOrder.OrderCode;
                TempData["TotalAmount"] = totalAmount.ToString("N0");
                TempData["PaymentMethod"] = model.PaymentMethod;

                return RedirectToAction("Success");
            }
            catch (InvalidOperationException ex)
            {
                // Stock validation error
                _logger.LogWarning("❌ InvalidOperationException: {Message}", ex.Message);
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log chi tiết exception để debug
                _logger.LogError(ex, "❌ Exception nghiêm trọng trong PlaceOrder");
                TempData["Error"] = "Đã có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                return RedirectToAction("Index");
            }
        }

        public IActionResult Success()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.OrderCode = TempData["OrderCode"];
            ViewBag.TotalAmount = TempData["TotalAmount"];
            ViewBag.PaymentMethod = TempData["PaymentMethod"];
            return View();
        }
    }
}
