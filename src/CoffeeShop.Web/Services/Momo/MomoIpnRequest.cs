using System.Text.Json.Serialization;

namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// DTO cho IPN callback từ MoMo khi thanh toán hoàn tất
    /// </summary>
    public class MomoIpnRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; } = string.Empty;

        /// <summary>
        /// Mã giao dịch của MoMo
        /// </summary>
        [JsonPropertyName("transId")]
        public long TransId { get; set; }

        /// <summary>
        /// Mã kết quả: 0 = thành công
        /// </summary>
        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Loại thanh toán: webApp, app, qr
        /// </summary>
        [JsonPropertyName("payType")]
        public string PayType { get; set; } = string.Empty;

        [JsonPropertyName("responseTime")]
        public long ResponseTime { get; set; }

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        /// <summary>
        /// Chữ ký để verify callback
        /// </summary>
        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
