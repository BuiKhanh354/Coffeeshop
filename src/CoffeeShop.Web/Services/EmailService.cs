using System.Net;
using System.Net.Mail;

namespace CoffeeShop.Web.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPassword { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        Task SendContactFormEmailAsync(string name, string email, string phone, string subject, string message);
        Task SendReviewNotificationAsync(string productName, string userName, int rating, string comment);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSettings settings, ILogger<EmailService> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SendContactFormEmailAsync(string name, string email, string phone, string subject, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SenderEmail) || string.IsNullOrEmpty(_settings.SenderPassword))
                {
                    _logger.LogWarning("Email settings not configured. Cannot send contact form email.");
                    return;
                }

                using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, "Coffee Shop Contact Form"),
                    Subject = $"[Contact Form] {subject} - From: {name}",
                    Body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2 style='color: #d97706;'>New Contact Form Submission</h2>
    <table style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Name:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{name}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Email:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{email}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Phone:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{phone ?? "N/A"}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Subject:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{subject}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Message:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{message}</td>
        </tr>
    </table>
    <p style='margin-top: 20px; color: #666; font-size: 12px;'>Sent from Coffee Shop website contact form.</p>
</body>
</html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(_settings.AdminEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Contact form email sent successfully from {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact form email");
                throw;
            }
        }

        public async Task SendReviewNotificationAsync(string productName, string userName, int rating, string comment)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SenderEmail) || string.IsNullOrEmpty(_settings.SenderPassword))
                {
                    _logger.LogWarning("Email settings not configured. Cannot send review notification email.");
                    return;
                }

                using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                    EnableSsl = true
                };

                var stars = new string('★', rating) + new string('☆', 5 - rating);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, "Coffee Shop Reviews"),
                    Subject = $"[New Review] {productName} - {stars} ({rating}/5)",
                    Body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2 style='color: #d97706;'>New Product Review</h2>
    <table style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Product:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{productName}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Reviewer:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{userName}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Rating:</td>
            <td style='padding: 8px; border: 1px solid #ddd; color: #f59e0b; font-size: 18px;'>{stars}</td>
        </tr>
        <tr>
            <td style='padding: 8px; border: 1px solid #ddd; font-weight: bold;'>Comment:</td>
            <td style='padding: 8px; border: 1px solid #ddd;'>{comment ?? "No comment"}</td>
        </tr>
    </table>
    <p style='margin-top: 20px; color: #666; font-size: 12px;'>This review requires admin approval before being published.</p>
</body>
</html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(_settings.AdminEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Review notification email sent for product {productName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send review notification email");
                throw;
            }
        }
    }
}
