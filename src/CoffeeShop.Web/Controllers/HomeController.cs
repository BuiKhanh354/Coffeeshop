using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoffeeShop.Web.Models;
using CoffeeShop.Web.Services;

namespace CoffeeShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(string name, string email, string phone, string subject, string message, string topic)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(message))
        {
            TempData["Error"] = "Vui lòng điền đầy đủ thông tin bắt buộc!";
            return RedirectToAction("Contact");
        }

        try
        {
            await _emailService.SendContactFormEmailAsync(name, email, phone ?? "", topic ?? subject ?? "Liên hệ", message);
            TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong vòng 24 giờ.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending contact form email");
            TempData["Error"] = "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau.";
        }

        return RedirectToAction("Contact");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
