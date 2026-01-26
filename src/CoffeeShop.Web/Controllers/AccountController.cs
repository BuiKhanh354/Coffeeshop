using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CoffeeShop.Web.Models;

namespace CoffeeShop.Web.Controllers
{
    public class AccountController : Controller
    {
        // In-memory user storage (demo - should use database in production)
        private static readonly List<User> Users = new()
        {
            new User { Id = 1, Email = "admin@coffeeshop.vn", PasswordHash = HashPassword("Admin@123"), FullName = "Quản trị viên", Phone = "0123456789", Role = "Admin" },
            new User { Id = 2, Email = "user@coffeeshop.vn", PasswordHash = HashPassword("User@123"), FullName = "Khách hàng Demo", Phone = "0987654321", Role = "Customer" }
        };

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
            
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Redirect based on role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email exists
            if (Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng");
                return View(model);
            }

            // Create new user
            var newUser = new User
            {
                Id = Users.Max(u => u.Id) + 1,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                FullName = model.FullName,
                Phone = model.Phone,
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            Users.Add(newUser);

            // Auto login after register
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, newUser.Id.ToString()),
                new Claim(ClaimTypes.Email, newUser.Email),
                new Claim(ClaimTypes.Name, newUser.FullName),
                new Claim(ClaimTypes.Role, newUser.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với Coffee Shop.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AdminLogin(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());
            
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
                return View(model);
            }

            if (user.Role != "Admin")
            {
                ModelState.AddModelError("", "Tài khoản không có quyền quản trị");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(model.RememberMe ? 30 : 1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Admin");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Orders()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        // Simple password hashing (demo - use BCrypt or similar in production)
        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "CoffeeShopSalt"));
            return Convert.ToBase64String(bytes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            if (avatar == null || avatar.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ảnh" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Chỉ chấp nhận file ảnh (jpg, png, gif, webp)" });
            }

            // Validate file size (max 5MB)
            if (avatar.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "File ảnh không được vượt quá 5MB" });
            }

            try
            {
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                var avatarUrl = $"/uploads/avatars/{fileName}";
                
                // Store in memory
                Services.InMemoryDataStore.UserAvatars[userEmail] = avatarUrl;

                return Json(new { success = true, avatarUrl = avatarUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetAvatar()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var avatarUrl = Services.InMemoryDataStore.UserAvatars.GetValueOrDefault(userEmail, "");
            return Json(new { avatarUrl });
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}
