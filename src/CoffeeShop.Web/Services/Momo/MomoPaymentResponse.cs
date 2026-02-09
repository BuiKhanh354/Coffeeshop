using System.Text.Json.Serialization;

namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// DTO cho response từ MoMo API khi tạo thanh toán
    /// </summary>
    public class MomoPaymentResponse
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("responseTime")]
        public long ResponseTime { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        /// <summary>
        /// URL để redirect user đến trang thanh toán MoMo
        /// </summary>
        [JsonPropertyName("payUrl")]
        public string PayUrl { get; set; } = string.Empty;

        /// <summary>
        /// Deeplink mở app MoMo trên mobile
        /// </summary>
        [JsonPropertyName("deeplink")]
        public string Deeplink { get; set; } = string.Empty;

        /// <summary>
        /// QR Code URL để quét thanh toán
        /// </summary>
        [JsonPropertyName("qrCodeUrl")]
        public string QrCodeUrl { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
