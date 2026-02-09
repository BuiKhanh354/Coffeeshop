namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// Interface cho MoMo Payment Service
    /// </summary>
    public interface IMomoPaymentService
    {
        /// <summary>
        /// Tạo yêu cầu thanh toán MoMo
        /// </summary>
        /// <param name="orderId">ID đơn hàng trong hệ thống</param>
        /// <param name="orderCode">Mã đơn hàng</param>
        /// <param name="amount">Số tiền thanh toán</param>
        /// <param name="orderInfo">Thông tin đơn hàng</param>
        /// <returns>Response từ MoMo với payUrl để redirect</returns>
        Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, string orderCode, decimal amount, string orderInfo);

        /// <summary>
        /// Xác thực chữ ký từ MoMo callback
        /// </summary>
        /// <param name="request">IPN request từ MoMo</param>
        /// <returns>True nếu signature hợp lệ</returns>
        bool ValidateSignature(MomoIpnRequest request);

        /// <summary>
        /// Xử lý IPN callback từ MoMo
        /// </summary>
        /// <param name="request">IPN request</param>
        /// <returns>True nếu xử lý thành công</returns>
        Task<bool> ProcessIpnAsync(MomoIpnRequest request);
    }
}
