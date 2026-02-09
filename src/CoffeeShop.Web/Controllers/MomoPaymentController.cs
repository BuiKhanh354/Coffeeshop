using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Services;
using CoffeeShop.Web.Services.Momo;

namespace CoffeeShop.Web.Controllers
{
    /// <summary>
    /// Controller xử lý thanh toán MoMo
    /// </summary>
    public class MomoPaymentController : Controller
    {
        private readonly IMomoPaymentService _momoPaymentService;
        private readonly IOrderService _orderService;
        private readonly ILogger<MomoPaymentController> _logger;

        public MomoPaymentController(
            IMomoPaymentService momoPaymentService,
            IOrderService orderService,
            ILogger<MomoPaymentController> logger)
        {
            _momoPaymentService = momoPaymentService;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Nhận IPN callback từ MoMo khi thanh toán hoàn tất
        /// POST /Momo/IpnCallback
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> IpnCallback([FromBody] MomoIpnRequest request)
        {
            _logger.LogInformation("Nhận IPN callback từ MoMo: {@Request}", request);

            try
            {
                var result = await _momoPaymentService.ProcessIpnAsync(request);

                if (result)
                {
                    // MoMo yêu cầu trả về 204 NoContent khi xử lý thành công
                    return NoContent();
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý IPN callback từ MoMo");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Xử lý redirect sau khi thanh toán MoMo
        /// GET /Momo/PaymentReturn
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PaymentReturn(
            [FromQuery] string partnerCode,
            [FromQuery] string orderId,
            [FromQuery] string requestId,
            [FromQuery] long amount,
            [FromQuery] string orderInfo,
            [FromQuery] string orderType,
            [FromQuery] long transId,
            [FromQuery] int resultCode,
            [FromQuery] string message,
            [FromQuery] string payType,
            [FromQuery] long responseTime,
            [FromQuery] string extraData,
            [FromQuery] string signature)
        {
            _logger.LogInformation("MoMo Payment Return - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
                orderId, resultCode, message);

            try
            {
                // Parse orderId để lấy orderCode (format: orderCode_timestamp)
                var orderCodeParts = orderId.Split('_');
                var orderCode = orderCodeParts.Length > 0 ? orderCodeParts[0] : orderId;

                // Tìm order
                var order = await _orderService.GetByOrderCodeAsync(orderCode);

                if (resultCode == 0)
                {
                    // Thanh toán thành công - cập nhật trạng thái thanh toán
                    if (order != null)
                    {
                        // Cập nhật trạng thái thanh toán cho đơn hàng
                        await _orderService.UpdatePaymentStatusAsync(order.Id, "Paid");

                        // Cập nhật trạng thái đơn hàng từ "Pending" sang "Processing" sau khi thanh toán thành công
                        await _orderService.UpdateOrderStatusAsync(order.Id, "Processing");
                    }

                    TempData["OrderSuccess"] = true;
                    TempData["OrderCode"] = orderCode;
                    TempData["TotalAmount"] = amount.ToString("N0");
                    TempData["PaymentMethod"] = "MoMo";
                    TempData["MomoTransId"] = transId.ToString();

                    return RedirectToAction("Success", "Checkout");
                }
                else
                {
                    // Thanh toán thất bại hoặc bị hủy
                    if (order != null)
                    {
                        // Cập nhật trạng thái thanh toán thất bại
                        await _orderService.UpdatePaymentStatusAsync(order.Id, "Failed");
                        await _orderService.UpdateOrderStatusAsync(order.Id, "Cancelled");

                        TempData["OrderCode"] = orderCode;
                    }

                    TempData["Error"] = $"Thanh toán MoMo không thành công: {message}";
                    return RedirectToAction("PaymentFailed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý MoMo payment return");
                TempData["Error"] = "Đã có lỗi xảy ra khi xử lý thanh toán.";
                return RedirectToAction("PaymentFailed");
            }
        }

        /// <summary>
        /// Trang hiển thị khi thanh toán thất bại
        /// </summary>
        public IActionResult PaymentFailed()
        {
            return View();
        }
    }
}
