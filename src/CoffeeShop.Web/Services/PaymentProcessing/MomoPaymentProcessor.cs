using CoffeeShop.Web.Services.Momo;
using Microsoft.Extensions.Logging;

namespace CoffeeShop.Web.Services.PaymentProcessing
{
    /// <summary>
    /// Xử lý thanh toán MoMo.
    /// Tạo yêu cầu thanh toán và redirect user đến trang MoMo.
    /// </summary>
    public class MomoPaymentProcessor : IPaymentMethodProcessor
    {
        private readonly IMomoPaymentService _momoService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<MomoPaymentProcessor> _logger;

        public MomoPaymentProcessor(
            IMomoPaymentService momoService,
            IPaymentService paymentService,
            ILogger<MomoPaymentProcessor> logger)
        {
            _momoService = momoService;
            _paymentService = paymentService;
            _logger = logger;
        }

        public string PaymentMethod => "MoMo";
        
        /// <summary>
        /// MoMo: Trạng thái ban đầu là "Pending" - chờ thanh toán online
        /// </summary>
        public string InitialPaymentStatus => "Pending";

        public async Task<PaymentResult> ProcessAsync(Models.Order order, Models.Payment payment)
        {
            _logger.LogInformation(
                "🔄 Bắt đầu xử lý thanh toán MoMo cho đơn hàng {OrderCode}, Amount: {Amount}",
                order.OrderCode, order.TotalAmount);

            try
            {
                var momoResponse = await _momoService.CreatePaymentAsync(
                    order.Id,
                    order.OrderCode,
                    order.TotalAmount,
                    $"Thanh toán đơn hàng {order.OrderCode}");

                if (momoResponse.ResultCode == 0 && !string.IsNullOrEmpty(momoResponse.PayUrl))
                {
                    _logger.LogInformation(
                        "✅ MoMo Payment tạo thành công, PayUrl: {PayUrl}",
                        momoResponse.PayUrl);

                    // Redirect user đến trang thanh toán MoMo
                    return PaymentResult.Succeeded(momoResponse.PayUrl);
                }
                else
                {
                    _logger.LogWarning(
                        "❌ MoMo Payment thất bại: ResultCode={ResultCode}, Message={Message}",
                        momoResponse.ResultCode, momoResponse.Message);

                    // Cập nhật payment status thành Failed
                    await _paymentService.UpdateStatusByOrderIdAsync(order.Id, "Failed");

                    return PaymentResult.Failed($"Không thể tạo thanh toán MoMo: {momoResponse.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi nghiêm trọng khi xử lý MoMo Payment");
                
                await _paymentService.UpdateStatusByOrderIdAsync(order.Id, "Failed");
                
                return PaymentResult.Failed($"Lỗi hệ thống khi thanh toán MoMo: {ex.Message}");
            }
        }
    }
}
