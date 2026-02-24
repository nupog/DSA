using System.Text.Json.Serialization;

namespace DeepSeekSurveyAnalyzer.Models;

public class ChatChunk
{
    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    public class Choice
    {
        [JsonPropertyName("delta")]
        public Delta Delta { get; set; } = new();
    }

    public class Delta
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        [JsonPropertyName("reasoning_content")]
        public string? ReasoningContent { get; set; }
    }
}