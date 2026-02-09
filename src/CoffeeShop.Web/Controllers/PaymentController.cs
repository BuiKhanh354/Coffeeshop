using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services.Momo;
using Microsoft.Extensions.Logging;

namespace CoffeeShop.Web.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        private readonly ILogger<PaymentController> _logger;
        
        public PaymentController(IMomoService momoService, ILogger<PaymentController> logger)
        {
            _momoService = momoService;
            _logger = logger;
        }
        
        [HttpPost]
        [Route("CreatePaymentUrl")]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            try
            {
                _logger.LogInformation("Creating MoMo payment for order {OrderId}, amount {Amount}", model.OrderId, model.Amount);
                
                var response = await _momoService.CreatePaymentAsync(model);
                
                if (string.IsNullOrEmpty(response.PayUrl))
                {
                    _logger.LogError("MoMo payment creation failed: No PayUrl returned. ErrorCode: {ErrorCode}, Message: {Message}", 
                        response.ErrorCode, response.Message);
                    
                    TempData["Error"] = "Không thể tạo yêu cầu thanh toán MoMo. Vui lòng thử lại.";
                    return RedirectToAction("Index", "Checkout");
                }
                
                _logger.LogInformation("Redirecting to MoMo payment page: {PayUrl}", response.PayUrl);
                return Redirect(response.PayUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment for order {OrderId}", model.OrderId);
                TempData["Error"] = "Đã xảy ra lỗi khi tạo yêu cầu thanh toán MoMo. Vui lòng thử lại.";
                return RedirectToAction("Index", "Checkout");
            }
        }
    }
}