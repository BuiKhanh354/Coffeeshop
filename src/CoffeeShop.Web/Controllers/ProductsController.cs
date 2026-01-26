using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        // Sample data - trong thực tế sẽ lấy từ database
        private static readonly List<Category> Categories = new()
        {
            new Category { Id = 1, Name = "Cà phê hạt", Description = "Cà phê hạt rang xay nguyên chất" },
            new Category { Id = 2, Name = "Cà phê bột", Description = "Cà phê bột pha phin truyền thống" },
            new Category { Id = 3, Name = "Cà phê hòa tan", Description = "Cà phê hòa tan tiện lợi" },
            new Category { Id = 4, Name = "Bình giữ nhiệt", Description = "Bình giữ nhiệt cao cấp, giữ nóng lâu" },
            new Category { Id = 5, Name = "Ly cà phê", Description = "Ly uống cà phê đa dạng kiểu dáng" },
            new Category { Id = 6, Name = "Cà phê đóng gói", Description = "Túi cà phê đóng gói sẵn tiện lợi" }
        };

        private static readonly List<Product> Products = new()
        {
            new Product { Id = 1, Name = "Espresso Arabica", Description = "Hạt cà phê Arabica rang đậm, hương thơm nồng nàn, vị đắng thanh", Price = 185000, StockQuantity = 50, ImageUrl = "/images/products/espresso.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 2, Name = "Robusta Đặc Biệt", Description = "Hạt Robusta nguyên chất từ Tây Nguyên, đậm đà mạnh mẽ", Price = 165000, StockQuantity = 45, ImageUrl = "/images/products/robusta.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 3, Name = "Cà Phê Phin Truyền Thống", Description = "Cà phê bột pha phin, hương vị Việt Nam đích thực", Price = 95000, StockQuantity = 100, ImageUrl = "/images/products/phin.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 4, Name = "Cà Phê Sữa Đá", Description = "Cà phê bột pha sữa đá, vị ngọt béo quyến rũ", Price = 85000, StockQuantity = 80, ImageUrl = "/images/products/sua-da.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 5, Name = "Cà Phê Hòa Tan 3in1", Description = "Cà phê hòa tan tiện lợi, vị cân bằng", Price = 125000, StockQuantity = 200, ImageUrl = "/images/products/3in1.jpg", CategoryId = 3, CategoryName = "Cà phê hòa tan" },
            new Product { Id = 6, Name = "Cold Brew Concentrate", Description = "Cà phê ủ lạnh 24h, vị mượt mà không chua", Price = 220000, StockQuantity = 30, ImageUrl = "/images/products/cold-brew.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 7, Name = "Bình Giữ Nhiệt Inox 500ml", Description = "Bình giữ nhiệt inox 304, giữ nóng 12h, giữ lạnh 24h", Price = 350000, StockQuantity = 40, ImageUrl = "/images/binhgiunhiet.png", CategoryId = 4, CategoryName = "Bình giữ nhiệt" },
            new Product { Id = 8, Name = "Bình Giữ Nhiệt Mini 350ml", Description = "Bình giữ nhiệt nhỏ gọn, tiện mang theo", Price = 250000, StockQuantity = 55, ImageUrl = "/images/products/binh-mini.jpg", CategoryId = 4, CategoryName = "Bình giữ nhiệt" },
            new Product { Id = 9, Name = "Mocha Blend Premium", Description = "Blend đặc biệt với hương chocolate tự nhiên", Price = 245000, StockQuantity = 35, ImageUrl = "/images/products/mocha.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 10, Name = "Cà Phê Đắk Lắk", Description = "Cà phê nguyên chất từ cao nguyên Đắk Lắk", Price = 175000, StockQuantity = 55, ImageUrl = "/images/products/daklak.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 11, Name = "Cappuccino Instant", Description = "Cà phê hòa tan cappuccino kem béo", Price = 145000, StockQuantity = 120, ImageUrl = "/images/products/cappuccino.jpg", CategoryId = 3, CategoryName = "Cà phê hòa tan" },
            new Product { Id = 12, Name = "Ly Sứ Cao Cấp 300ml", Description = "Ly sứ uống cà phê thiết kế sang trọng", Price = 120000, StockQuantity = 80, ImageUrl = "/images/products/ly-su.jpg", CategoryId = 5, CategoryName = "Ly cà phê" },
            new Product { Id = 13, Name = "Ly Thủy Tinh 2 Lớp", Description = "Ly thủy tinh 2 lớp cách nhiệt, giữ nóng lâu", Price = 150000, StockQuantity = 60, ImageUrl = "/images/products/ly-thuy-tinh.jpg", CategoryId = 5, CategoryName = "Ly cà phê" },
            new Product { Id = 14, Name = "Túi Cà Phê Drip 10 gói", Description = "Cà phê phin giấy tiện lợi, pha nhanh 3 phút", Price = 85000, StockQuantity = 150, ImageUrl = "/images/product-1.png", CategoryId = 6, CategoryName = "Cà phê đóng gói" },
            
        };

        public IActionResult Index(int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, string? search, int page = 1)
        {
            var query = Products.AsQueryable();

            // Filter by search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) || 
                                        p.Description.ToLower().Contains(searchTerm) ||
                                        p.CategoryName.ToLower().Contains(searchTerm));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Filter by price
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // Sort
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Id)
            };

            // Pagination
            int pageSize = 12;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.Categories = Categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sort;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SearchQuery = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Related products (same category)
            var relatedProducts = Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id)
                .Take(4)
                .ToList();

            // Get reviews for this product
            var reviews = Services.InMemoryDataStore.GetProductReviews(id);
            var averageRating = Services.InMemoryDataStore.GetProductAverageRating(id);
            var reviewCount = Services.InMemoryDataStore.GetProductReviewCount(id);

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = averageRating;
            ViewBag.ReviewCount = reviewCount;
            
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(int productId, int rating, string comment)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá" });
            }

            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1-5 sao" });
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung đánh giá" });
            }

            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
            var userName = User.Identity?.Name ?? "Khách hàng";
            var userAvatar = Services.InMemoryDataStore.UserAvatars.GetValueOrDefault(userEmail, "");

            var review = new Review
            {
                Id = Services.InMemoryDataStore.GetNextReviewId(),
                ProductId = productId,
                UserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                UserName = userName,
                UserAvatar = userAvatar,
                Rating = rating,
                Comment = comment.Trim(),
                CreatedAt = DateTime.Now
            };

            Services.InMemoryDataStore.Reviews.Add(review);

            return Json(new { 
                success = true, 
                message = "Cảm ơn bạn đã đánh giá!",
                review = new {
                    review.Id,
                    review.UserName,
                    review.UserAvatar,
                    review.Rating,
                    review.Comment,
                    CreatedAt = review.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpGet]
        public IActionResult Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var searchTerm = q.ToLower();
            var results = Products
                .Where(p => p.Name.ToLower().Contains(searchTerm) || 
                           p.Description.ToLower().Contains(searchTerm) ||
                           p.CategoryName.ToLower().Contains(searchTerm))
                .Take(6)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.ImageUrl,
                    PriceFormatted = p.Price.ToString("N0") + "đ"
                })
                .ToList();

            return Json(results);
        }
    }
}
