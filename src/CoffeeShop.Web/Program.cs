using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Web.Data;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;
using CoffeeShop.Web.Chatbot.Services;
using CoffeeShop.Web.Services.Momo;
using CoffeeShop.Web.Models.Momo;
using CoffeeShop.Web.Services.PaymentProcessing;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Chatbot (Llama via Groq)
builder.Services.Configure<ChatbotOptions>(builder.Configuration.GetSection(ChatbotOptions.SectionName));
builder.Services.AddHttpClient<IChatbotService, ChatbotService>();

// Add MySQL Database Context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CoffeeShopDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Register Services (Dependency Injection)
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();

// Connect Momo Payment API
builder.Services.Configure<MomoOptionModel>(builder.Configuration.GetSection("MomoAPI"));
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<IMomoPaymentService, MomoPaymentService>();

// Payment Processors (Strategy Pattern)
builder.Services.AddScoped<IPaymentMethodProcessor, CodPaymentProcessor>();
builder.Services.AddScoped<IPaymentMethodProcessor, MomoPaymentProcessor>();
builder.Services.AddScoped<IPaymentProcessorFactory, PaymentProcessorFactory>();


// Add Session support (for guest cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(7);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

var app = builder.Build();

// Seed or ensure default admin user exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = app.Logger;

    try
    {
        var dbContext = services.GetRequiredService<CoffeeShopDbContext>();

        var adminEmail = "admin@coffeeshop.local";
        var userService = services.GetRequiredService<IUserService>();

        // Tìm admin theo email cố định
        var adminUser = dbContext.Users.FirstOrDefault(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            // Chưa có user này -> tạo mới
            adminUser = new User
            {
                Username = "admin",
                Email = adminEmail,
                FullName = "Coffee Shop Admin",
                Role = "Admin",
                IsActive = true
            };

            // Mật khẩu mặc định: Admin@123
            userService.CreateAsync(adminUser, "Admin@123").GetAwaiter().GetResult();
            logger.LogInformation("Đã seed tài khoản admin mặc định: {Email} / Admin@123", adminEmail);
        }
        else
        {
            // Đã tồn tại user với email này -> đảm bảo là Admin, Active và reset mật khẩu
            adminUser.Role = "Admin";
            adminUser.IsActive = true;
            adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            adminUser.UpdatedAt = DateTime.Now;
            dbContext.SaveChanges();

            logger.LogInformation("Đã đảm bảo tài khoản admin {Email} tồn tại và reset mật khẩu về Admin@123.", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Lỗi khi seed tài khoản admin mặc định");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session middleware
app.UseSession();

// Add Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();