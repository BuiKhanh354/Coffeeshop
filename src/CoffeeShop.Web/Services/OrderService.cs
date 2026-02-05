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
        Task<Order> UpdatePaymentStatusAsync(int orderId, string paymentStatus);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetPagedAsync(int page, int pageSize, string? status = null);
        Task<string> GenerateOrderCodeAsync();
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
                // Generate order code if not provided
                if (string.IsNullOrEmpty(order.OrderCode))
                {
                    order.OrderCode = await GenerateOrderCodeAsync();
                }

                order.OrderDate = DateTime.Now;
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Add order details
                foreach (var detail in orderDetails)
                {
                    detail.OrderId = order.Id;
                    _context.OrderDetails.Add(detail);
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
    }
}
