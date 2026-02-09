namespace CoffeeShop.Web.Services.PaymentProcessing
{
    /// <summary>
    /// Kết quả xử lý thanh toán
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        
        /// <summary>
        /// URL redirect (dành cho MoMo, VNPay...)
        /// Nếu có, Controller sẽ redirect user đến URL này
        /// </summary>
        public string? RedirectUrl { get; set; }
        
        /// <summary>
        /// Thông báo lỗi nếu thanh toán thất bại
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Tạo kết quả thành công
        /// </summary>
        public static PaymentResult Succeeded(string? redirectUrl = null)
        {
            return new PaymentResult { Success = true, RedirectUrl = redirectUrl };
        }
        
        /// <summary>
        /// Tạo kết quả thất bại
        /// </summary>
        public static PaymentResult Failed(string errorMessage)
        {
            return new PaymentResult { Success = false, ErrorMessage = errorMessage };
        }
    }

    /// <summary>
    /// Interface chung cho các payment method processors.
    /// Sử dụng Strategy Pattern để dễ dàng thêm phương thức thanh toán mới.
    /// </summary>
    public interface IPaymentMethodProcessor
    {
        /// <summary>
        /// Mã phương thức thanh toán (COD, MoMo, VNPay, etc.)
        /// </summary>
        string PaymentMethod { get; }
        
        /// <summary>
        /// Trạng thái thanh toán ban đầu khi tạo đơn
        /// - COD: "Unpaid" (chờ nhận hàng)
        /// - MoMo: "Pending" (chờ thanh toán online)
        /// </summary>
        string InitialPaymentStatus { get; }
        
        /// <summary>
        /// Xử lý thanh toán sau khi đã tạo Order và Payment record
        /// </summary>
        Task<PaymentResult> ProcessAsync(Models.Order order, Models.Payment payment);
    }
}
