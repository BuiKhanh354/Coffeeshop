namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// Configuration options for MoMo Payment Gateway
    /// </summary>
    public class MomoOptions
    {
        public const string SectionName = "Momo";

        /// <summary>
        /// Mã đối tác được MoMo cung cấp
        /// </summary>
        public string PartnerCode { get; set; } = string.Empty;

        /// <summary>
        /// Access Key để xác thực với MoMo API
        /// </summary>
        public string AccessKey { get; set; } = string.Empty;

        /// <summary>
        /// Secret Key để tạo chữ ký (signature)
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;

        /// <summary>
        /// Endpoint API của MoMo (Sandbox hoặc Production)
        /// </summary>
        public string ApiEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";

        /// <summary>
        /// URL redirect về sau khi thanh toán
        /// </summary>
        public string ReturnUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL nhận IPN callback từ MoMo
        /// </summary>
        public string IpnUrl { get; set; } = string.Empty;
    }
}
