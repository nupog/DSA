namespace DeepSeekSurveyAnalyzer.Services.Abstractions;

public interface IFileReader
{
    bool CanRead(string filePath);
    Task<string> ReadTextAsync(string filePath, IProgress<string>? progress = null);
}