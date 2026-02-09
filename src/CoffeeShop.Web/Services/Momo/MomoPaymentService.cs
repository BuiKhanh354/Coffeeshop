using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace CoffeeShop.Web.Services.Momo
{
    /// <summary>
    /// Implementation của MoMo Payment Service
    /// Xử lý tạo thanh toán, verify signature, và xử lý callback
    /// ĐÃ SỬA LỖI: Signature ordering, amount format, double-update prevention
    /// </summary>
    public class MomoPaymentService : IMomoPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly MomoOptions _options;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly ILogger<MomoPaymentService> _logger;

        public MomoPaymentService(
            HttpClient httpClient,
            IOptions<MomoOptions> options,
            IPaymentService paymentService,
            IOrderService orderService,
            ILogger<MomoPaymentService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _paymentService = paymentService;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo yêu cầu thanh toán MoMo
        /// FIX: Amount là string không dấu phẩy, orderId/requestId unique
        /// </summary>
        public async Task<MomoPaymentResponse> CreatePaymentAsync(int orderId, string orderCode, decimal amount, string orderInfo)
        {
            try
            {
                // FIX: Tạo requestId và orderId unique
                var requestId = $"{orderCode}_{DateTime.Now.Ticks}";
                var momoOrderId = $"{orderCode}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                
                // FIX: Amount phải là số nguyên, không có dấu phẩy hay decimal
                var amountLong = (long)Math.Floor(amount);
                var amountString = amountLong.ToString(); // String thuần, không format

                // FIX: Raw signature theo đúng thứ tự alphabetical của MoMo
                // accessKey, amount, extraData, ipnUrl, orderId, orderInfo, partnerCode, redirectUrl, requestId, requestType
                var rawSignature = $"accessKey={_options.AccessKey}" +
                                   $"&amount={amountString}" +
                                   $"&extraData=" +
                                   $"&ipnUrl={_options.IpnUrl}" +
                                   $"&orderId={momoOrderId}" +
                                   $"&orderInfo={orderInfo}" +
                                   $"&partnerCode={_options.PartnerCode}" +
                                   $"&redirectUrl={_options.ReturnUrl}" +
                                   $"&requestId={requestId}" +
                                   $"&requestType=captureWallet";

                _logger.LogInformation("MoMo Raw Signature String: {RawSignature}", rawSignature);

                // Ký bằng HMAC SHA256
                var signature = ComputeHmacSha256(rawSignature, _options.SecretKey);
                _logger.LogInformation("MoMo Computed Signature: {Signature}", signature);

                var request = new MomoPaymentRequest
                {
                    PartnerCode = _options.PartnerCode,
                    AccessKey = _options.AccessKey,
                    RequestId = requestId,
                    Amount = amountLong,
                    OrderId = momoOrderId,
                    OrderInfo = orderInfo,
                    RedirectUrl = _options.ReturnUrl,
                    IpnUrl = _options.IpnUrl,
                    RequestType = "captureWallet",
                    ExtraData = "",
                    Lang = "vi",
                    Signature = signature
                };

                _logger.LogInformation("MoMo Request OrderId: {OrderId}, RequestId: {RequestId}, Amount: {Amount}", 
                    momoOrderId, requestId, amountLong);

                // Gửi request đến MoMo API
                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("MoMo Request JSON: {Json}", jsonContent);

                var response = await _httpClient.PostAsync(_options.ApiEndpoint, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("MoMo Response Status: {StatusCode}, Body: {Response}", 
                    response.StatusCode, responseBody);

                var momoResponse = JsonSerializer.Deserialize<MomoPaymentResponse>(responseBody);

                if (momoResponse == null)
                {
                    _logger.LogError("Không thể parse MoMo response");
                    return new MomoPaymentResponse
                    {
                        ResultCode = -1,
                        Message = "Không thể parse response từ MoMo"
                    };
                }

                // Log kết quả chi tiết
                _logger.LogInformation("MoMo Response: ResultCode={ResultCode}, Message={Message}, PayUrl={PayUrl}", 
                    momoResponse.ResultCode, momoResponse.Message, momoResponse.PayUrl ?? "null");

                return momoResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo thanh toán MoMo cho OrderId: {OrderId}", orderId);
                return new MomoPaymentResponse
                {
                    ResultCode = -1,
                    Message = $"Lỗi hệ thống: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Xác thực chữ ký từ MoMo IPN callback
        /// FIX: Đúng thứ tự field alphabetical theo MoMo docs
        /// </summary>
        public bool ValidateSignature(MomoIpnRequest request)
        {
            try
            {
                // FIX: Tạo raw signature theo đúng thứ tự alphabetical
                // accessKey, amount, extraData, message, orderId, orderInfo, orderType, 
                // partnerCode, payType, requestId, responseTime, resultCode, transId
                var rawSignature = $"accessKey={_options.AccessKey}" +
                                   $"&amount={request.Amount}" +
                                   $"&extraData={request.ExtraData}" +
                                   $"&message={request.Message}" +
                                   $"&orderId={request.OrderId}" +
                                   $"&orderInfo={request.OrderInfo}" +
                                   $"&orderType={request.OrderType}" +
                                   $"&partnerCode={request.PartnerCode}" +
                                   $"&payType={request.PayType}" +
                                   $"&requestId={request.RequestId}" +
                                   $"&responseTime={request.ResponseTime}" +
                                   $"&resultCode={request.ResultCode}" +
                                   $"&transId={request.TransId}";

                _logger.LogInformation("IPN Raw Signature String: {RawSignature}", rawSignature);

                var computedSignature = ComputeHmacSha256(rawSignature, _options.SecretKey);

                _logger.LogInformation("IPN Computed Signature: {Computed}, Received: {Received}", 
                    computedSignature, request.Signature);

                var isValid = computedSignature.Equals(request.Signature, StringComparison.OrdinalIgnoreCase);

                if (!isValid)
                {
                    _logger.LogWarning("MoMo IPN signature KHÔNG HỢP LỆ! Có thể bị giả mạo.");
                }
                else
                {
                    _logger.LogInformation("MoMo IPN signature hợp lệ.");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate MoMo signature");
                return false;
            }
        }

        /// <summary>
        /// Xử lý IPN callback từ MoMo
        /// FIX: Thêm double-update prevention, chỉ xử lý nếu chưa Paid
        /// </summary>
        public async Task<bool> ProcessIpnAsync(MomoIpnRequest request)
        {
            try
            {
                _logger.LogInformation("=== Bắt đầu xử lý MoMo IPN ===");
                _logger.LogInformation("OrderId: {OrderId}, ResultCode: {ResultCode}, TransId: {TransId}", 
                    request.OrderId, request.ResultCode, request.TransId);

                // 1. Validate signature TRƯỚC
                if (!ValidateSignature(request))
                {
                    _logger.LogWarning("❌ IPN callback có signature KHÔNG HỢP LỆ: {OrderId}", request.OrderId);
                    return false; // Không xử lý nếu signature sai
                }

                // 2. Chỉ xử lý nếu resultCode = 0 (thành công)
                if (request.ResultCode != 0)
                {
                    _logger.LogWarning("❌ MoMo trả về resultCode != 0: {ResultCode}, Message: {Message}", 
                        request.ResultCode, request.Message);
                    // Vẫn return true để MoMo không gửi lại IPN
                    return true;
                }

                // 3. Parse orderId để lấy orderCode (format: orderCode_timestamp_guid)
                var orderCodeParts = request.OrderId.Split('_');
                if (orderCodeParts.Length < 1)
                {
                    _logger.LogWarning("OrderId không hợp lệ: {OrderId}", request.OrderId);
                    return false;
                }
                var orderCode = orderCodeParts[0];

                // 4. Tìm order theo orderCode
                var order = await _orderService.GetByOrderCodeAsync(orderCode);
                if (order == null)
                {
                    _logger.LogWarning("❌ Không tìm thấy đơn hàng với mã: {OrderCode}", orderCode);
                    return false;
                }

                // 5. FIX: Double-update prevention - Kiểm tra đã xử lý chưa
                if (order.PaymentStatus == "Paid")
                {
                    _logger.LogInformation("⚠️ Đơn hàng {OrderCode} đã được thanh toán trước đó, bỏ qua IPN duplicate", orderCode);
                    return true; // Return true để MoMo không gửi lại
                }

                // 6. Chỉ xử lý nếu PaymentMethod là MoMo
                if (order.PaymentMethod != "MoMo")
                {
                    _logger.LogWarning("❌ Đơn hàng {OrderCode} không phải MoMo, bỏ qua", orderCode);
                    return true;
                }

                // 7. Cập nhật trạng thái
                _logger.LogInformation("✅ Thanh toán MoMo thành công cho đơn hàng {OrderCode}, TransId: {TransId}", 
                    orderCode, request.TransId);

                // Cập nhật payment với transactionId
                var payment = await _paymentService.GetByOrderIdAsync(order.Id);
                if (payment != null)
                {
                    await _paymentService.UpdateStatusAsync(payment.Id, "Paid", request.TransId.ToString());
                }
                else
                {
                    await _paymentService.UpdateStatusByOrderIdAsync(order.Id, "Paid");
                }

                // Cập nhật order payment status
                await _orderService.UpdatePaymentStatusAsync(order.Id, "Paid");

                _logger.LogInformation("=== Hoàn thành xử lý MoMo IPN cho {OrderCode} ===", orderCode);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi nghiêm trọng khi xử lý MoMo IPN callback");
                return false;
            }
        }

        /// <summary>
        /// Tính HMAC SHA256 - Chuẩn MoMo
        /// </summary>
        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);

            // FIX: Chuyển thành lowercase hex string
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}

