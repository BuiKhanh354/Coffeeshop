using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;

namespace CoffeeShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IReviewService reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(int? categoryId, decimal? minPrice, decimal? maxPrice, string? sort, string? search, int page = 1)
        {
            int pageSize = 12;
            var (products, totalCount) = await _productService.GetPagedAsync(page, pageSize, categoryId, search);

            // Apply price filter (client-side for now, can be moved to service)
            var filteredProducts = products.ToList();
            if (minPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice.Value).ToList();
            }
            if (maxPrice.HasValue)
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice.Value).ToList();
            }

            // Apply sorting
            filteredProducts = sort switch
            {
                "price_asc" => filteredProducts.OrderBy(p => p.Price).ToList(),
                "price_desc" => filteredProducts.OrderByDescending(p => p.Price).ToList(),
                "name" => filteredProducts.OrderBy(p => p.Name).ToList(),
                "newest" => filteredProducts.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => filteredProducts
            };

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var categories = await _categoryService.GetActiveAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sort;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SearchQuery = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalCount;

            return View(filteredProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Increment view count
            await _productService.IncrementViewCountAsync(id);

            // Related products (same category)
            var relatedProducts = (await _productService.GetByCategoryAsync(product.CategoryId))
                .Where(p => p.Id != product.Id)
                .Take(4)
                .ToList();

            // Get reviews for this product
            var reviews = await _reviewService.GetByProductIdAsync(id, true);
            var averageRating = await _reviewService.GetAverageRatingAsync(id);
            var reviewCount = await _reviewService.GetReviewCountAsync(id);

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = averageRating;
            ViewBag.ReviewCount = reviewCount;

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
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

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userName = User.Identity?.Name ?? "Khách hàng";

            // Check if user already reviewed this product
            if (await _reviewService.HasUserReviewedAsync(userId, productId))
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi" });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                Comment = comment.Trim()
            };

            await _reviewService.CreateAsync(review);

            return Json(new
            {
                success = true,
                message = "Cảm ơn bạn đã đánh giá! Đánh giá sẽ hiển thị sau khi được duyệt.",
                review = new
                {
                    review.Id,
                    UserName = userName,
                    UserAvatar = "",
                    review.Rating,
                    review.Comment,
                    CreatedAt = review.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new List<object>());
            }

            var products = await _productService.SearchAsync(q);
            var results = products
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
