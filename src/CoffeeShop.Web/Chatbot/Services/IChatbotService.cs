using CoffeeShop.Web.Chatbot.Models;

namespace CoffeeShop.Web.Chatbot.Services;

/// <summary>
/// Service for generating chatbot responses using Google Gemini API.
/// </summary>
public interface IChatbotService
{
    /// <summary>
    /// Sends user message and optional history to Gemini, returns assistant reply.
    /// </summary>
    Task<ChatResponse> SendAsync(string userMessage, IReadOnlyList<ChatMessageDto>? history = null, CancellationToken cancellationToken = default);
}
