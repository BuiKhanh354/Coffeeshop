using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    /// <summary>
    /// In-memory data store for demo purposes.
    /// This file is kept for backward compatibility but is no longer used.
    /// All data is now stored in MySQL database via Entity Framework Core.
    /// </summary>
    [Obsolete("Use database services instead. This class is kept for reference only.")]
    public static class InMemoryDataStore
    {
        // User avatars: Dictionary<UserEmail, AvatarUrl>
        public static Dictionary<string, string> UserAvatars { get; } = new();

        // Product reviews: List of all reviews - DEPRECATED, use ReviewService instead
        public static List<Review> Reviews { get; } = new();

        private static int _nextReviewId = 1;

        public static int GetNextReviewId() => _nextReviewId++;

        public static List<Review> GetProductReviews(int productId)
        {
            return Reviews.Where(r => r.ProductId == productId).OrderByDescending(r => r.CreatedAt).ToList();
        }

        public static double GetProductAverageRating(int productId)
        {
            var productReviews = Reviews.Where(r => r.ProductId == productId).ToList();
            return productReviews.Count > 0 ? productReviews.Average(r => r.Rating) : 0;
        }

        public static int GetProductReviewCount(int productId)
        {
            return Reviews.Count(r => r.ProductId == productId);
        }
    }
}
