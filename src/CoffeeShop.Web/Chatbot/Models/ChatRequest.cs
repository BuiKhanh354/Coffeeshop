namespace CoffeeShop.Web.Chatbot.Models;

/// <summary>
/// Request model for sending a chat message to the bot.
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// User message text.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional: previous messages for context (user/assistant pairs).
    /// </summary>
    public List<ChatMessageDto>? History { get; set; }
}

/// <summary>
/// A single message in chat history.
/// </summary>
public class ChatMessageDto
{
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Text { get; set; } = string.Empty;
}
