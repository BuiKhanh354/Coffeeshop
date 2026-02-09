using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface IPaymentService
    {
        Task<Payment?> GetByIdAsync(int id);
        Task<Payment?> GetByOrderIdAsync(int orderId);
        Task<Payment?> GetByTransactionIdAsync(string transactionId);
        Task<Payment> CreateAsync(Payment payment);
        Task<Payment> UpdateStatusAsync(int paymentId, string status, string? transactionId = null);
        Task UpdateStatusByOrderIdAsync(int orderId, string status);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public class PaymentService : IPaymentService
    {
        private readonly CoffeeShopDbContext _context;

        public PaymentService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Payment?> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId);
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            payment.CreatedAt = DateTime.Now;
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateStatusAsync(int paymentId, string status, string? transactionId = null)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                throw new InvalidOperationException("Payment not found");

            payment.Status = status;

            if (!string.IsNullOrEmpty(transactionId))
                payment.TransactionId = transactionId;

            if (status == "Paid")
                payment.PaidAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateStatusByOrderIdAsync(int orderId, string status)
        {
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .ToListAsync();

            foreach (var payment in payments)
            {
                payment.Status = status;
                if (status == "Paid")
                    payment.PaidAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
