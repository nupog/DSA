namespace DeepSeekSurveyAnalyzer.Models;

public class HistoryEntry
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public List<string> Files { get; set; } = new();
}