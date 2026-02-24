namespace DeepSeekSurveyAnalyzer.Models;

public class AppSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.deepseek.com/chat/completions";
    public string PromptText { get; set; } = string.Empty;
    public string Model { get; set; } = "deepseek-reasoner";
    public string LogLevel { get; set; } = "Information";
}