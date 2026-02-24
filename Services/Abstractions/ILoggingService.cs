namespace DeepSeekSurveyAnalyzer.Services.Abstractions;

public interface ILoggingService
{
    void Information(string message, params object[] properties);
    void Error(Exception ex, string message, params object[] properties);
    void Warning(string message, params object[] properties);
}