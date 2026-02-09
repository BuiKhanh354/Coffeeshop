namespace CoffeeShop.Web.Chatbot.Models;

/// <summary>
/// Response model for chatbot API.
/// </summary>
public class ChatResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
