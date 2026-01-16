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
            new Category { Id = 4, Name = "Dụng cụ pha chế", Description = "Phụ kiện và dụng cụ pha cà phê" }
        };

        private static readonly List<Product> Products = new()
        {
            new Product { Id = 1, Name = "Espresso Arabica", Description = "Hạt cà phê Arabica rang đậm, hương thơm nồng nàn, vị đắng thanh", Price = 185000, StockQuantity = 50, ImageUrl = "/images/products/espresso.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 2, Name = "Robusta Đặc Biệt", Description = "Hạt Robusta nguyên chất từ Tây Nguyên, đậm đà mạnh mẽ", Price = 165000, StockQuantity = 45, ImageUrl = "/images/products/robusta.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 3, Name = "Cà Phê Phin Truyền Thống", Description = "Cà phê bột pha phin, hương vị Việt Nam đích thực", Price = 95000, StockQuantity = 100, ImageUrl = "/images/products/phin.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 4, Name = "Cà Phê Sữa Đá", Description = "Cà phê bột pha sữa đá, vị ngọt béo quyến rũ", Price = 85000, StockQuantity = 80, ImageUrl = "/images/products/sua-da.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 5, Name = "Cà Phê Hòa Tan 3in1", Description = "Cà phê hòa tan tiện lợi, vị cân bằng", Price = 125000, StockQuantity = 200, ImageUrl = "/images/products/3in1.jpg", CategoryId = 3, CategoryName = "Cà phê hòa tan" },
            new Product { Id = 6, Name = "Cold Brew Concentrate", Description = "Cà phê ủ lạnh 24h, vị mượt mà không chua", Price = 220000, StockQuantity = 30, ImageUrl = "/images/products/cold-brew.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 7, Name = "Phin Inox Cao Cấp", Description = "Phin cà phê inox 304, bền đẹp, pha ngon", Price = 75000, StockQuantity = 60, ImageUrl = "/images/products/phin-inox.jpg", CategoryId = 4, CategoryName = "Dụng cụ pha chế" },
            new Product { Id = 8, Name = "Bình French Press", Description = "Bình pha cà phê kiểu Pháp 350ml, tiện lợi", Price = 285000, StockQuantity = 25, ImageUrl = "/images/products/french-press.jpg", CategoryId = 4, CategoryName = "Dụng cụ pha chế" },
            new Product { Id = 9, Name = "Mocha Blend Premium", Description = "Blend đặc biệt với hương chocolate tự nhiên", Price = 245000, StockQuantity = 35, ImageUrl = "/images/products/mocha.jpg", CategoryId = 1, CategoryName = "Cà phê hạt" },
            new Product { Id = 10, Name = "Cà Phê Đắk Lắk", Description = "Cà phê nguyên chất từ cao nguyên Đắk Lắk", Price = 175000, StockQuantity = 55, ImageUrl = "/images/products/daklak.jpg", CategoryId = 2, CategoryName = "Cà phê bột" },
            new Product { Id = 11, Name = "Cappuccino Instant", Description = "Cà phê hòa tan cappuccino kem béo", Price = 145000, StockQuantity = 120, ImageUrl = "/images/products/cappuccino.jpg", CategoryId = 3, CategoryName = "Cà phê hòa tan" },
            new Product { Id = 12, Name = "Máy Xay Cà Phê Mini", Description = "Máy xay cà phê cầm tay, nhỏ gọn tiện lợi", Price = 450000, StockQuantity = 15, ImageUrl = "/images/products/grinder.jpg", CategoryId = 4, CategoryName = "Dụng cụ pha chế" }
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

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
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
