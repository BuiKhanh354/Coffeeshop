using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeShop.Web.Chatbot.Models;
using Microsoft.Extensions.Options;

namespace CoffeeShop.Web.Chatbot.Services;

/// <summary>
/// Gọi Llama (qua Groq, OpenAI-compatible) để tạo câu trả lời chatbot.
/// </summary>
public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly ChatbotOptions _options;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ChatbotService(HttpClient httpClient, IOptions<ChatbotOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri("https://api.groq.com/");
    }

    public async Task<ChatResponse> SendAsync(string userMessage, IReadOnlyList<ChatMessageDto>? history = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Llama API key chưa được cấu hình. Vui lòng thêm Gemini:ApiKey (Groq key) vào appsettings."
            };
        }

        // Thiết lập header Authorization cho mỗi request
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var messages = new List<ChatMessage>();

        // System prompt
        var systemPrompt = GetSystemPrompt();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            messages.Add(new ChatMessage
            {
                Role = "system",
                Content = systemPrompt
            });
        }

        // Lịch sử hội thoại
        if (history?.Count > 0)
        {
            foreach (var m in history.TakeLast(_options.MaxHistoryMessages * 2))
            {
                var role = m.Role == "model" || m.Role == "assistant" ? "assistant" : "user";
                messages.Add(new ChatMessage
                {
                    Role = role,
                    Content = m.Text
                });
            }
        }

        // Tin nhắn mới của user
        messages.Add(new ChatMessage
        {
            Role = "user",
            Content = userMessage
        });

        var requestBody = new ChatCompletionRequest
        {
            Model = string.IsNullOrWhiteSpace(_options.Model)
                ? "llama-3.1-8b-instant"
                : _options.Model,
            Messages = messages,
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxOutputTokens
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "openai/v1/chat/completions",
            requestBody,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ChatResponse
            {
                Success = false,
                Error = $"Llama API lỗi: {response.StatusCode}. {errBody}"
            };
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        var completion = JsonSerializer.Deserialize<ChatCompletionResponse>(json, JsonOptions);
        var text = completion?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

        if (string.IsNullOrEmpty(text))
        {
            return new ChatResponse
            {
                Success = false,
                Error = "Không nhận được phản hồi từ Llama."
            };
        }

        return new ChatResponse
        {
            Success = true,
            Message = text
        };
    }

    private string GetSystemPrompt()
    {
        return string.IsNullOrWhiteSpace(_options.SystemPrompt)
            ? "Bạn là trợ lý ảo của Coffee Shop, sử dụng Llama 3 (qua Groq). Trả lời ngắn gọn, thân thiện bằng tiếng Việt. Chỉ tư vấn về đồ uống, menu, giờ mở cửa, đặt bàn, khuyến mãi. Nếu không chắc chắn, gợi ý khách gọi hotline hoặc đến cửa hàng."
            : _options.SystemPrompt!;
    }

    #region Groq / OpenAI Chat DTOs

    private class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public float Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }

    private class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }

    #endregion
}

/// <summary>
/// Cấu hình cho Chatbot / Llama (qua Groq).
/// </summary>
public class ChatbotOptions
{
    // Vẫn dùng section \"Gemini\" cho tiện, không cần đổi Program/appsettings
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Tên model Llama trên Groq, ví dụ \"llama-3.1-8b-instant\".
    /// </summary>
    public string Model { get; set; } = "llama-3.1-8b-instant";

    public float Temperature { get; set; } = 0.7f;

    public int MaxOutputTokens { get; set; } = 1024;

    public int MaxHistoryMessages { get; set; } = 10;

    public string? SystemPrompt { get; set; }
}
