using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface IOrderService
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByOrderCodeAsync(string orderCode);
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> CreateAsync(Order order, IEnumerable<OrderDetail> orderDetails);
        Task<Order> UpdateStatusAsync(int orderId, string status);
        Task<Order> UpdateOrderStatusAsync(int orderId, string orderStatus);
        Task<Order> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, string? status = null);
        Task<string> GenerateOrderCodeAsync();

        // Statistics methods for Admin Dashboard
        Task<int> GetTotalOrderCountAsync();
        Task<decimal> GetTodayRevenueAsync();
        Task<int> GetPendingOrderCountAsync();
        Task<int> GetProcessingOrderCountAsync();

        // Reports methods
        Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrderCountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageOrderValueByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewCustomerCountByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderCountByStatusAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<(string ProductName, int Sold, decimal Revenue)>> GetTopSellingProductsAsync(DateTime? startDate = null, DateTime? endDate = null, int top = 10);
    }

    public class OrderService : IOrderService
    {
        private readonly CoffeeShopDbContext _context;

        public OrderService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)!
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Order?> GetByOrderCodeAsync(string orderCode)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)!
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateAsync(Order order, IEnumerable<OrderDetail> orderDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderDetailsList = orderDetails.ToList();

                // Validate stock for all items
                foreach (var detail in orderDetailsList)
                {
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Sản phẩm không tồn tại: {detail.ProductName}");
                    }
                    if (product.StockQuantity < detail.Quantity)
                    {
                        throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ hàng. Còn lại: {product.StockQuantity}");
                    }
                }

                // Generate order code if not provided
                if (string.IsNullOrEmpty(order.OrderCode))
                {
                    order.OrderCode = await GenerateOrderCodeAsync();
                }

                order.OrderDate = DateTime.Now;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order details and update stock
                foreach (var detail in orderDetailsList)
                {
                    detail.OrderId = order.Id;
                    _context.OrderDetails.Add(detail);

                    // Decrease stock
                    var product = await _context.Products.FindAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= detail.Quantity;
                    }
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return order;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Order> UpdateStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.OrderStatus = status;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateOrderStatusAsync(int orderId, string orderStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.OrderStatus = orderStatus;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdatePaymentStatusAsync(int orderId, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.PaymentStatus = paymentStatus;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? status = null)
        {
            var queryable = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                queryable = queryable.Where(o => o.OrderStatus == status);
            }

            var totalCount = await queryable.CountAsync();

            var orders = await queryable
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<string> GenerateOrderCodeAsync()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = await _context.Orders
                .CountAsync(o => o.OrderCode.StartsWith($"ORD{date}"));
            return $"ORD{date}{(count + 1):D4}";
        }

        // Statistics methods for Admin Dashboard
        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var today = DateTime.Today;
            return await _context.Orders
                .Where(o => o.OrderDate.Date == today && o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetPendingOrderCountAsync()
        {
            return await _context.Orders.CountAsync(o => o.OrderStatus == "Pending");
        }

        public async Task<int> GetProcessingOrderCountAsync()
        {
            return await _context.Orders.CountAsync(o => o.OrderStatus == "Processing");
        }

        // Reports methods implementation
        public async Task<decimal> GetRevenueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<int> GetOrderCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .CountAsync(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
        }

        public async Task<decimal> GetAverageOrderValueByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var paidOrders = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.PaymentStatus == "Paid")
                .ToListAsync();

            if (paidOrders.Count == 0)
                return 0;

            return paidOrders.Average(o => o.TotalAmount);
        }

        public async Task<int> GetNewCustomerCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Users
                .CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate && u.Role == "Customer");
        }

        public async Task<Dictionary<string, int>> GetOrderCountByStatusAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            var orders = await query.ToListAsync();
            return orders
                .GroupBy(o => o.OrderStatus)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<(string ProductName, int Sold, decimal Revenue)>> GetTopSellingProductsAsync(
            DateTime? startDate = null, DateTime? endDate = null, int top = 10)
        {
            var query = _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(od => od.Order!.OrderDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(od => od.Order!.OrderDate <= endDate.Value);

            // Chỉ tính đơn đã thanh toán
            query = query.Where(od => od.Order!.PaymentStatus == "Paid");

            var orderDetails = await query.ToListAsync();

            var result = orderDetails
                .GroupBy(od => od.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Sold = g.Sum(od => od.Quantity),
                    Revenue = g.Sum(od => od.TotalPrice)
                })
                .OrderByDescending(x => x.Sold)
                .Take(top)
                .Select(x => (x.ProductName, x.Sold, x.Revenue))
                .ToList();

            return result;
        }
    }
}
