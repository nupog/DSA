using System.Text.Json.Serialization;

namespace DeepSeekSurveyAnalyzer.Models;

public class ChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "deepseek-reasoner";

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = new();

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = true;
}