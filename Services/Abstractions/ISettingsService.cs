using DeepSeekSurveyAnalyzer.Models;

namespace DeepSeekSurveyAnalyzer.Services.Abstractions;

public interface ISettingsService
{
    AppSettings Load();
    void Save(AppSettings settings);
}