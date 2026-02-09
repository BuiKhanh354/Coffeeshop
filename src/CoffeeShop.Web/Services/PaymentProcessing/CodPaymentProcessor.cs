using Microsoft.Extensions.Logging;

namespace CoffeeShop.Web.Services.PaymentProcessing
{
    /// <summary>
    /// Xử lý thanh toán COD (Cash on Delivery).
    /// Thanh toán khi nhận hàng - không cần redirect hay xử lý gì đặc biệt.
    /// </summary>
    public class CodPaymentProcessor : IPaymentMethodProcessor
    {
        private readonly ILogger<CodPaymentProcessor> _logger;

        public CodPaymentProcessor(ILogger<CodPaymentProcessor> logger)
        {
            _logger = logger;
        }

        public string PaymentMethod => "COD";
        
        /// <summary>
        /// COD: Trạng thái ban đầu là "Unpaid" - chờ khách nhận hàng và thanh toán
        /// </summary>
        public string InitialPaymentStatus => "Unpaid";

        public Task<PaymentResult> ProcessAsync(Models.Order order, Models.Payment payment)
        {
            _logger.LogInformation(
                "✅ COD Payment cho đơn hàng {OrderCode}: Không cần xử lý thêm, chờ giao hàng",
                order.OrderCode);

            // COD không cần redirect - đơn hàng được đặt thành công ngay
            return Task.FromResult(PaymentResult.Succeeded());
        }
    }
}
