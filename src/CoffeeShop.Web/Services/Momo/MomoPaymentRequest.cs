using System.Text.Json.Serialization;

namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// DTO cho request tạo thanh toán MoMo
    /// Theo chuẩn MoMo API v2 - captureWallet
    /// </summary>
    public class MomoPaymentRequest
    {
        [JsonPropertyName("partnerCode")]
        public string PartnerCode { get; set; } = string.Empty;

        [JsonPropertyName("accessKey")]
        public string AccessKey { get; set; } = string.Empty;

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("orderInfo")]
        public string OrderInfo { get; set; } = string.Empty;

        [JsonPropertyName("redirectUrl")]
        public string RedirectUrl { get; set; } = string.Empty;

        [JsonPropertyName("ipnUrl")]
        public string IpnUrl { get; set; } = string.Empty;

        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = "captureWallet";

        [JsonPropertyName("extraData")]
        public string ExtraData { get; set; } = string.Empty;

        [JsonPropertyName("lang")]
        public string Lang { get; set; } = "vi";

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }
}
