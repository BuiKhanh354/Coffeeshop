namespace CoffeeShop.Web.Services.PaymentProcessing
{
    /// <summary>
    /// Interface cho factory quản lý các payment processors.
    /// </summary>
    public interface IPaymentProcessorFactory
    {
        /// <summary>
        /// Lấy processor phù hợp với phương thức thanh toán.
        /// </summary>
        /// <param name="paymentMethod">Mã phương thức: COD, MoMo, etc.</param>
        IPaymentMethodProcessor GetProcessor(string paymentMethod);
        
        /// <summary>
        /// Kiểm tra xem phương thức thanh toán có được hỗ trợ không.
        /// </summary>
        bool IsSupported(string paymentMethod);
        
        /// <summary>
        /// Lấy danh sách các phương thức thanh toán được hỗ trợ.
        /// </summary>
        IEnumerable<string> GetSupportedMethods();
    }

    /// <summary>
    /// Factory để lấy payment processor phù hợp.
    /// Sử dụng DI để inject tất cả các processors đã đăng ký.
    /// </summary>
    public class PaymentProcessorFactory : IPaymentProcessorFactory
    {
        private readonly Dictionary<string, IPaymentMethodProcessor> _processors;

        public PaymentProcessorFactory(IEnumerable<IPaymentMethodProcessor> processors)
        {
            // Tạo dictionary để lookup nhanh theo PaymentMethod
            _processors = processors.ToDictionary(
                p => p.PaymentMethod,
                p => p,
                StringComparer.OrdinalIgnoreCase);
        }

        public IPaymentMethodProcessor GetProcessor(string paymentMethod)
        {
            if (_processors.TryGetValue(paymentMethod, out var processor))
            {
                return processor;
            }
            
            throw new NotSupportedException(
                $"Phương thức thanh toán '{paymentMethod}' không được hỗ trợ. " +
                $"Các phương thức hỗ trợ: {string.Join(", ", _processors.Keys)}");
        }

        public bool IsSupported(string paymentMethod)
        {
            return _processors.ContainsKey(paymentMethod);
        }

        public IEnumerable<string> GetSupportedMethods()
        {
            return _processors.Keys;
        }
    }
}
