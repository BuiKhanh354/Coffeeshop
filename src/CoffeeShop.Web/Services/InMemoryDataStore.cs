using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Services
{
    /// <summary>
    /// In-memory data store for demo purposes.
    /// Data is lost when the application restarts.
    /// </summary>
    public static class InMemoryDataStore
    {
        // User avatars: Dictionary<UserEmail, AvatarUrl>
        public static Dictionary<string, string> UserAvatars { get; } = new();

        // Product reviews: List of all reviews
        public static List<Review> Reviews { get; } = new()
        {
            // Demo reviews
            new Review { Id = 1, ProductId = 1, UserId = 1, UserName = "Nguyễn Văn A", Rating = 5, Comment = "Cà phê rất ngon, hương vị đậm đà!", CreatedAt = DateTime.Now.AddDays(-5) },
            new Review { Id = 2, ProductId = 1, UserId = 2, UserName = "Trần Thị B", Rating = 4, Comment = "Giao hàng nhanh, đóng gói cẩn thận.", CreatedAt = DateTime.Now.AddDays(-3) },
            new Review { Id = 3, ProductId = 2, UserId = 3, UserName = "Lê Minh C", Rating = 5, Comment = "Sản phẩm tuyệt vời, sẽ mua lại!", CreatedAt = DateTime.Now.AddDays(-1) }
        };

        private static int _nextReviewId = 4;

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
