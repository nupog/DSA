using System;

namespace DeepSeekSurveyAnalyzer.Models;

public class AnalysisResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string OriginalPrompt { get; set; } = string.Empty;
    public string AnalysisText { get; set; } = string.Empty;
    public string ReasoningText { get; set; } = string.Empty;
    public string SwotAnalysis { get; set; } = string.Empty;
    public string SwotReasoning { get; set; } = string.Empty;
}