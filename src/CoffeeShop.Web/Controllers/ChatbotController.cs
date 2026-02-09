using CoffeeShop.Web.Chatbot.Models;
using CoffeeShop.Web.Chatbot.Services;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Web.Controllers;

/// <summary>
/// API controller for the AI chat widget. Returns JSON for /Chatbot/SendMessage.
/// </summary>
[ApiController]
[Route("[controller]")]
[IgnoreAntiforgeryToken]
public class ChatbotController : ControllerBase
{
    private readonly IChatbotService _chatbotService;
    private readonly ILogger<ChatbotController> _logger;

    public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
    {
        _chatbotService = chatbotService;
        _logger = logger;
    }

    /// <summary>
    /// POST /Chatbot/SendMessage - receives user message and optional history, returns bot reply.
    /// </summary>
    [HttpPost("SendMessage")]
    [Produces("application/json")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new ChatResponse { Success = false, Error = "Tin nhắn không được để trống." });

        try
        {
            var response = await _chatbotService.SendAsync(
                request.Message.Trim(),
                request.History,
                cancellationToken);

            if (!response.Success)
            {
                _logger.LogWarning("Chatbot error: {Error}", response.Error);
                return Ok(response); // still 200 so frontend can show error message
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chatbot SendMessage failed");
            return Ok(new ChatResponse { Success = false, Error = "Đã xảy ra lỗi. Vui lòng thử lại sau." });
        }
    }
}
