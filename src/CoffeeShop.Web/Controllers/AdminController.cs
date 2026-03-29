using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CoffeeShop.Web.Services;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Data;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPaymentService _paymentService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly CoffeeShopDbContext _context;

        public AdminController(
            IOrderService orderService,
            IUserService userService,
            IProductService productService,
            ICategoryService categoryService,
            IPaymentService paymentService,
            IWebHostEnvironment webHostEnvironment,
            CoffeeShopDbContext context)
        {
            _orderService = orderService;
            _userService = userService;
            _productService = productService;
            _categoryService = categoryService;
            _paymentService = paymentService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get real statistics from MySQL database
            ViewBag.TotalOrders = await _orderService.GetTotalOrderCountAsync();
            ViewBag.TodayRevenue = await _orderService.GetTodayRevenueAsync();
            ViewBag.TotalProducts = await _productService.GetTotalProductCountAsync();
            ViewBag.TotalCustomers = await _userService.GetCustomerCountAsync();
            ViewBag.PendingOrders = await _orderService.GetPendingOrderCountAsync();
            ViewBag.ProcessingOrders = await _orderService.GetProcessingOrderCountAsync();

            return View();
        }

        #region Products Management

        // Allowed image extensions
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImageSize = 5 * 1024 * 1024; // 5MB

        /// <summary>
        /// Validate image file (extension and size)
        /// </summary>
        private bool ValidateImageFile(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                errorMessage = $"Định dạng file không hợp lệ. Chỉ chấp nhận: {string.Join(", ", AllowedImageExtensions)}";
                return false;
            }

            if (file.Length > MaxImageSize)
            {
                errorMessage = "Kích thước file vượt quá 5MB";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Save image file to /uploads/products/ and return the URL path
        /// </summary>
        private async Task<string?> SaveImageFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/products/{uniqueFileName}";
        }

        /// <summary>
        /// Delete image file from server if it's a local upload
        /// </summary>
        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith("/uploads/"))
                return;

            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        /// <summary>
        /// Generate slug from product name
        /// </summary>
        private static string GenerateSlug(string name)
        {
            var slug = name.ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("đ", "d")
                .Replace("á", "a").Replace("à", "a").Replace("ả", "a").Replace("ã", "a").Replace("ạ", "a")
                .Replace("ă", "a").Replace("ắ", "a").Replace("ằ", "a").Replace("ẳ", "a").Replace("ẵ", "a").Replace("ặ", "a")
                .Replace("â", "a").Replace("ấ", "a").Replace("ầ", "a").Replace("ẩ", "a").Replace("ẫ", "a").Replace("ậ", "a")
                .Replace("é", "e").Replace("è", "e").Replace("ẻ", "e").Replace("ẽ", "e").Replace("ẹ", "e")
                .Replace("ê", "e").Replace("ế", "e").Replace("ề", "e").Replace("ể", "e").Replace("ễ", "e").Replace("ệ", "e")
                .Replace("í", "i").Replace("ì", "i").Replace("ỉ", "i").Replace("ĩ", "i").Replace("ị", "i")
                .Replace("ó", "o").Replace("ò", "o").Replace("ỏ", "o").Replace("õ", "o").Replace("ọ", "o")
                .Replace("ô", "o").Replace("ố", "o").Replace("ồ", "o").Replace("ổ", "o").Replace("ỗ", "o").Replace("ộ", "o")
                .Replace("ơ", "o").Replace("ớ", "o").Replace("ờ", "o").Replace("ở", "o").Replace("ỡ", "o").Replace("ợ", "o")
                .Replace("ú", "u").Replace("ù", "u").Replace("ủ", "u").Replace("ũ", "u").Replace("ụ", "u")
                .Replace("ư", "u").Replace("ứ", "u").Replace("ừ", "u").Replace("ử", "u").Replace("ữ", "u").Replace("ự", "u")
                .Replace("ý", "y").Replace("ỳ", "y").Replace("ỷ", "y").Replace("ỹ", "y").Replace("ỵ", "y");

            // Remove special characters
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            return slug.Trim('-');
        }

        public async Task<IActionResult> Products()
        {
            var products = await _productService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = categories;
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles)
        {
            try
            {
                // Validate and upload main image
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!ValidateImageFile(imageFile, out string error))
                    {
                        TempData["Error"] = error;
                        return RedirectToAction(nameof(Products));
                    }
                    product.ImageUrl = await SaveImageFileAsync(imageFile);
                }

                // Validate and upload gallery images
                if (galleryFiles != null && galleryFiles.Count > 0)
                {
                    var galleryUrls = new List<string>();
                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            if (!ValidateImageFile(file, out string error))
                            {
                                TempData["Error"] = $"Lỗi ảnh phụ: {error}";
                                return RedirectToAction(nameof(Products));
                            }
                            var url = await SaveImageFileAsync(file);
                            if (url != null) galleryUrls.Add(url);
                        }
                    }
                    if (galleryUrls.Count > 0)
                    {
                        product.Images = JsonSerializer.Serialize(galleryUrls);
                    }
                }

                // Auto-generate slug if not provided
                if (string.IsNullOrEmpty(product.Slug))
                {
                    product.Slug = GenerateSlug(product.Name);
                }

                product.CreatedAt = DateTime.Now;
                product.IsActive = true;
                await _productService.CreateAsync(product);
                TempData["Success"] = "Thêm sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi thêm sản phẩm: " + ex.Message;
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(Product product, IFormFile? imageFile, List<IFormFile>? galleryFiles, string? existingImages)
        {
            try
            {
                var existingProduct = await _productService.GetByIdAsync(product.Id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm!";
                    return RedirectToAction(nameof(Products));
                }

                // Handle main image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!ValidateImageFile(imageFile, out string error))
                    {
                        TempData["Error"] = error;
                        return RedirectToAction(nameof(Products));
                    }

                    // Delete old main image
                    DeleteImageFile(existingProduct.ImageUrl);

                    existingProduct.ImageUrl = await SaveImageFileAsync(imageFile);
                }

                // Handle gallery images
                var currentImages = new List<string>();

                // Keep existing images that weren't removed
                if (!string.IsNullOrEmpty(existingImages))
                {
                    currentImages = JsonSerializer.Deserialize<List<string>>(existingImages) ?? new List<string>();
                }

                // Find and delete removed images
                if (!string.IsNullOrEmpty(existingProduct.Images))
                {
                    var oldImages = JsonSerializer.Deserialize<List<string>>(existingProduct.Images) ?? new List<string>();
                    foreach (var oldImage in oldImages)
                    {
                        if (!currentImages.Contains(oldImage))
                        {
                            DeleteImageFile(oldImage);
                        }
                    }
                }

                // Add new gallery images
                if (galleryFiles != null && galleryFiles.Count > 0)
                {
                    foreach (var file in galleryFiles)
                    {
                        if (file.Length > 0)
                        {
                            if (!ValidateImageFile(file, out string error))
                            {
                                TempData["Error"] = $"Lỗi ảnh phụ: {error}";
                                return RedirectToAction(nameof(Products));
                            }
                            var url = await SaveImageFileAsync(file);
                            if (url != null) currentImages.Add(url);
                        }
                    }
                }

                existingProduct.Images = currentImages.Count > 0 ? JsonSerializer.Serialize(currentImages) : null;

                // Update other fields
                existingProduct.Name = product.Name;
                existingProduct.Slug = string.IsNullOrEmpty(product.Slug) ? GenerateSlug(product.Name) : product.Slug;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.Price = product.Price;
                existingProduct.OriginalPrice = product.OriginalPrice;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.Description = product.Description;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.IsActive = product.IsActive;
                existingProduct.UpdatedAt = DateTime.Now;

                await _productService.UpdateAsync(existingProduct);
                TempData["Success"] = "Cập nhật sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật sản phẩm: " + ex.Message;
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product != null)
                {
                    // Delete main image
                    DeleteImageFile(product.ImageUrl);

                    // Delete gallery images
                    if (!string.IsNullOrEmpty(product.Images))
                    {
                        var images = JsonSerializer.Deserialize<List<string>>(product.Images);
                        if (images != null)
                        {
                            foreach (var img in images)
                            {
                                DeleteImageFile(img);
                            }
                        }
                    }
                }

                await _productService.DeleteAsync(id);
                TempData["Success"] = "Xóa sản phẩm thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return Json(product);
        }

        #endregion

        #region Users Management

        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUser(int id, string fullName, string email, string? phoneNumber, string role, bool isActive)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng!";
                    return RedirectToAction(nameof(Users));
                }

                user.FullName = fullName;
                user.Email = email;
                user.PhoneNumber = phoneNumber;
                user.Role = role;
                user.IsActive = isActive;
                user.UpdatedAt = DateTime.Now;

                await _userService.UpdateAsync(user);
                TempData["Success"] = "Cập nhật người dùng thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng!";
                    return RedirectToAction(nameof(Users));
                }

                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.Now;
                await _userService.UpdateAsync(user);
                TempData["Success"] = user.IsActive ? "Đã kích hoạt người dùng!" : "Đã vô hiệu hóa người dùng!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _userService.GetByIdAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng!";
                    return RedirectToAction(nameof(Users));
                }

                // Prevent deleting admin accounts
                if (user.Role == "Admin")
                {
                    TempData["Error"] = "Không thể xóa tài khoản Admin!";
                    return RedirectToAction(nameof(Users));
                }

                // Delete user's reviews first (cascade delete may not be enabled)
                var userReviews = await _context.Reviews.Where(r => r.UserId == id).ToListAsync();
                if (userReviews.Any())
                {
                    _context.Reviews.RemoveRange(userReviews);
                }

                // Delete user's orders (or mark as anonymous)
                var userOrders = await _context.Orders.Where(o => o.UserId == id).ToListAsync();
                if (userOrders.Any())
                {
                    // Instead of deleting orders, set UserId to null to preserve order history
                    foreach (var order in userOrders)
                    {
                        order.UserId = null;
                    }
                }

                // Delete user's cart
                var userCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == id);
                if (userCart != null)
                {
                    _context.Carts.Remove(userCart);
                }

                // Finally delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xóa người dùng thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();
            return Json(new { user.Id, user.Username, user.FullName, user.Email, user.PhoneNumber, user.Role, user.IsActive });
        }

        #endregion

        public async Task<IActionResult> Orders()
        {
            var orders = await _orderService.GetAllAsync();

            // Statistics by status
            var allOrders = orders.ToList();
            ViewBag.PendingCount = allOrders.Count(o => o.OrderStatus == "New" || o.OrderStatus == "Pending");
            ViewBag.ProcessingCount = allOrders.Count(o => o.OrderStatus == "Processing");
            ViewBag.ShippingCount = allOrders.Count(o => o.OrderStatus == "Shipping");
            ViewBag.CompletedCount = allOrders.Count(o => o.OrderStatus == "Delivered" || o.OrderStatus == "Completed");
            ViewBag.CancelledCount = allOrders.Count(o => o.OrderStatus == "Cancelled");
            ViewBag.TotalCount = allOrders.Count;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
                return NotFound();

            return Json(new
            {
                order.Id,
                order.OrderCode,
                order.CustomerName,
                order.CustomerPhone,
                order.CustomerEmail,
                order.ShippingAddress,
                order.SubTotal,
                order.ShippingFee,
                order.Discount,
                order.TotalAmount,
                order.PaymentMethod,
                order.PaymentStatus,
                order.OrderStatus,
                order.Note,
                OrderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                OrderDetails = order.OrderDetails?.Select(od => new
                {
                    od.ProductName,
                    od.Quantity,
                    od.UnitPrice,
                    od.TotalPrice,
                    ProductImage = od.Product?.ImageUrl
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var order = await _orderService.GetByIdAsync(orderId);
                if (order == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

                // Update order status
                await _orderService.UpdateStatusAsync(orderId, status);

                // Handle payment status based on order status and payment method
                if (status == "Delivered")
                {
                    // For delivered orders, payment should be marked as Paid regardless of payment method
                    await _orderService.UpdatePaymentStatusAsync(orderId, "Paid");
                    await _paymentService.UpdateStatusByOrderIdAsync(orderId, "Paid");
                }
                else if (status == "Cancelled")
                {
                    // For cancelled orders, mark payment as Failed
                    await _orderService.UpdatePaymentStatusAsync(orderId, "Failed");
                    await _paymentService.UpdateStatusByOrderIdAsync(orderId, "Failed");
                }
                else if (status == "Processing" && order.PaymentMethod == "MoMo" && order.PaymentStatus == "Pending")
                {
                    // For MoMo orders that are moving to Processing and still have Pending payment status,
                    // ensure payment is marked as Paid (this handles cases where IPN might have failed)
                    await _orderService.UpdatePaymentStatusAsync(orderId, "Paid");
                    await _paymentService.UpdateStatusByOrderIdAsync(orderId, "Paid");
                }
                // For COD orders, payment status remains "Unpaid" until delivery (handled above when status becomes "Delivered")

                return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Customers()
        {
            return View();
        }

        public async Task<IActionResult> Reports(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Mặc định: tháng hiện tại
            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Lấy dữ liệu thật từ database
            ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");

            ViewBag.TotalRevenue = await _orderService.GetRevenueByDateRangeAsync(startDate.Value, endDate.Value);
            ViewBag.TotalOrders = await _orderService.GetOrderCountByDateRangeAsync(startDate.Value, endDate.Value);
            ViewBag.AverageOrderValue = await _orderService.GetAverageOrderValueByDateRangeAsync(startDate.Value, endDate.Value);
            ViewBag.NewCustomers = await _orderService.GetNewCustomerCountByDateRangeAsync(startDate.Value, endDate.Value);

            // Đơn hàng theo trạng thái
            ViewBag.OrderStatusCounts = await _orderService.GetOrderCountByStatusAsync(startDate, endDate);

            // Top sản phẩm bán chạy
            ViewBag.TopProducts = await _orderService.GetTopSellingProductsAsync(startDate, endDate, 10);

            // Tính % tăng trưởng so với tháng trước (để demo, có thể tính thật sau)
            var previousMonthStart = startDate.Value.AddMonths(-1);
            var previousMonthEnd = endDate.Value.AddMonths(-1);
            var previousRevenue = await _orderService.GetRevenueByDateRangeAsync(previousMonthStart, previousMonthEnd);
            var previousOrders = await _orderService.GetOrderCountByDateRangeAsync(previousMonthStart, previousMonthEnd);

            ViewBag.RevenueGrowth = previousRevenue > 0
                ? ((ViewBag.TotalRevenue - previousRevenue) / previousRevenue * 100)
                : 0;
            ViewBag.OrderGrowth = previousOrders > 0
                ? ((ViewBag.TotalOrders - previousOrders) / (double)previousOrders * 100)
                : 0;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Mặc định: tháng hiện tại
            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Lấy dữ liệu
            var totalRevenue = await _orderService.GetRevenueByDateRangeAsync(startDate.Value, endDate.Value);
            var totalOrders = await _orderService.GetOrderCountByDateRangeAsync(startDate.Value, endDate.Value);
            var averageOrderValue = await _orderService.GetAverageOrderValueByDateRangeAsync(startDate.Value, endDate.Value);
            var newCustomers = await _orderService.GetNewCustomerCountByDateRangeAsync(startDate.Value, endDate.Value);
            var orderStatusCounts = await _orderService.GetOrderCountByStatusAsync(startDate, endDate);
            var topProducts = await _orderService.GetTopSellingProductsAsync(startDate, endDate, 10);

            // Tạo Excel file
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Báo cáo");

            // Header
            worksheet.Cells[1, 1].Value = "BÁO CÁO DOANH THU";
            worksheet.Cells[1, 1, 1, 3].Merge = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            worksheet.Cells[2, 1].Value = $"Từ ngày: {startDate.Value:dd/MM/yyyy}";
            worksheet.Cells[2, 2].Value = $"Đến ngày: {endDate.Value:dd/MM/yyyy}";

            int row = 4;

            // Tổng quan
            worksheet.Cells[row, 1].Value = "TỔNG QUAN";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;
            worksheet.Cells[row, 1].Value = "Tổng doanh thu:";
            worksheet.Cells[row, 2].Value = totalRevenue;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            row++;
            worksheet.Cells[row, 1].Value = "Tổng đơn hàng:";
            worksheet.Cells[row, 2].Value = totalOrders;
            row++;
            worksheet.Cells[row, 1].Value = "Giá trị đơn trung bình:";
            worksheet.Cells[row, 2].Value = averageOrderValue;
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            row++;
            worksheet.Cells[row, 1].Value = "Khách hàng mới:";
            worksheet.Cells[row, 2].Value = newCustomers;
            row += 2;

            // Đơn hàng theo trạng thái
            worksheet.Cells[row, 1].Value = "ĐƠN HÀNG THEO TRẠNG THÁI";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;
            worksheet.Cells[row, 1].Value = "Trạng thái";
            worksheet.Cells[row, 2].Value = "Số lượng";
            worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;
            row++;
            foreach (var status in orderStatusCounts)
            {
                worksheet.Cells[row, 1].Value = status.Key;
                worksheet.Cells[row, 2].Value = status.Value;
                row++;
            }
            row += 2;

            // Top sản phẩm
            worksheet.Cells[row, 1].Value = "SẢN PHẨM BÁN CHẠY";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;
            worksheet.Cells[row, 1].Value = "STT";
            worksheet.Cells[row, 2].Value = "Tên sản phẩm";
            worksheet.Cells[row, 3].Value = "Số lượng bán";
            worksheet.Cells[row, 4].Value = "Doanh thu";
            worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;
            row++;
            int stt = 1;
            foreach (var product in topProducts)
            {
                worksheet.Cells[row, 1].Value = stt++;
                worksheet.Cells[row, 2].Value = product.ProductName;
                worksheet.Cells[row, 3].Value = product.Sold;
                worksheet.Cells[row, 4].Value = product.Revenue;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                row++;
            }

            // Auto fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileName = $"BaoCao_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.xlsx";
            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Mặc định: tháng hiện tại
            if (!startDate.HasValue)
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (!endDate.HasValue)
                endDate = DateTime.Now;

            // Lấy dữ liệu
            var totalRevenue = await _orderService.GetRevenueByDateRangeAsync(startDate.Value, endDate.Value);
            var totalOrders = await _orderService.GetOrderCountByDateRangeAsync(startDate.Value, endDate.Value);
            var averageOrderValue = await _orderService.GetAverageOrderValueByDateRangeAsync(startDate.Value, endDate.Value);
            var newCustomers = await _orderService.GetNewCustomerCountByDateRangeAsync(startDate.Value, endDate.Value);
            var orderStatusCounts = await _orderService.GetOrderCountByStatusAsync(startDate, endDate);
            var topProducts = await _orderService.GetTopSellingProductsAsync(startDate, endDate, 10);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text("BÁO CÁO DOANH THU")
                        .FontSize(20)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Thông tin khoảng thời gian
                            column.Item().Text($"Từ ngày: {startDate.Value:dd/MM/yyyy} - Đến ngày: {endDate.Value:dd/MM/yyyy}")
                                .FontSize(12)
                                .AlignCenter();

                            // Tổng quan
                            column.Item().Text("TỔNG QUAN").FontSize(14).Bold();
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Tổng doanh thu:");
                                table.Cell().Element(CellStyle).Text(totalRevenue.ToString("N0") + " đ");
                                table.Cell().Element(CellStyle).Text("Tổng đơn hàng:");
                                table.Cell().Element(CellStyle).Text(totalOrders.ToString());
                                table.Cell().Element(CellStyle).Text("Giá trị đơn trung bình:");
                                table.Cell().Element(CellStyle).Text(averageOrderValue.ToString("N0") + " đ");
                                table.Cell().Element(CellStyle).Text("Khách hàng mới:");
                                table.Cell().Element(CellStyle).Text(newCustomers.ToString());
                            });

                            // Đơn hàng theo trạng thái
                            if (orderStatusCounts.Any())
                            {
                                column.Item().Text("ĐƠN HÀNG THEO TRẠNG THÁI").FontSize(14).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Cell().Element(CellStyle).Text("Trạng thái").Bold();
                                    table.Cell().Element(CellStyle).Text("Số lượng").Bold();

                                    foreach (var status in orderStatusCounts)
                                    {
                                        table.Cell().Element(CellStyle).Text(status.Key);
                                        table.Cell().Element(CellStyle).Text(status.Value.ToString());
                                    }
                                });
                            }

                            // Top sản phẩm
                            if (topProducts.Any())
                            {
                                column.Item().Text("SẢN PHẨM BÁN CHẠY").FontSize(14).Bold();
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(1);
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Cell().Element(CellStyle).Text("STT").Bold();
                                    table.Cell().Element(CellStyle).Text("Tên sản phẩm").Bold();
                                    table.Cell().Element(CellStyle).Text("Số lượng").Bold();
                                    table.Cell().Element(CellStyle).Text("Doanh thu").Bold();

                                    int stt = 1;
                                    foreach (var product in topProducts)
                                    {
                                        table.Cell().Element(CellStyle).Text(stt++.ToString());
                                        table.Cell().Element(CellStyle).Text(product.ProductName);
                                        table.Cell().Element(CellStyle).Text(product.Sold.ToString());
                                        table.Cell().Element(CellStyle).Text(product.Revenue.ToString("N0") + " đ");
                                    }
                                });
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            });

            static IContainer CellStyle(IContainer container)
            {
                return container
                    .Border(1)
                    .Padding(5)
                    .Background(Colors.Grey.Lighten3);
            }

            var fileName = $"BaoCao_{startDate.Value:yyyyMMdd}_{endDate.Value:yyyyMMdd}.pdf";
            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}

