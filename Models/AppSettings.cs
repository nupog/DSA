using System.Collections.Generic;

namespace DeepSeekSurveyAnalyzer.Models;

public class AppSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.deepseek.com/chat/completions";
    public string PromptText { get; set; } = "Проанализируйте результаты опроса и выделите основные выводы и рекомендации";
    public string SwotPromptText { get; set; } = "На основе предоставленного анализа опроса, сформируйте SWOT-анализ. Выделите сильные стороны, слабые стороны, возможности и угрозы.";
    public string Model { get; set; } = "deepseek-reasoner";
    public string LogLevel { get; set; } = "Information";
}