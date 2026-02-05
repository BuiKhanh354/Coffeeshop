using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    public interface IReviewService
    {
        Task<Review?> GetByIdAsync(int id);
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId, bool onlyApproved = true);
        Task<IEnumerable<Review>> GetByUserIdAsync(int userId);
        Task<Review> CreateAsync(Review review);
        Task<Review> ApproveAsync(int reviewId);
        Task DeleteAsync(int id);
        Task<double> GetAverageRatingAsync(int productId);
        Task<int> GetReviewCountAsync(int productId);
        Task<bool> HasUserReviewedAsync(int userId, int productId);
    }

    public class ReviewService : IReviewService
    {
        private readonly CoffeeShopDbContext _context;

        public ReviewService(CoffeeShopDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> GetByProductIdAsync(int productId, bool onlyApproved = true)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId);

            if (onlyApproved)
                query = query.Where(r => r.IsApproved);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(int userId)
        {
            return await _context.Reviews
                .Include(r => r.Product)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> CreateAsync(Review review)
        {
            review.CreatedAt = DateTime.Now;
            review.IsApproved = false; // Requires moderation

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> ApproveAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
                throw new InvalidOperationException("Review not found");

            review.IsApproved = true;
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task DeleteAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var ratings = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Select(r => r.Rating)
                .ToListAsync();

            return ratings.Any() ? ratings.Average() : 0;
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId && r.IsApproved);
        }

        public async Task<bool> HasUserReviewedAsync(int userId, int productId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }
    }
}
