using DeepSeekSurveyAnalyzer.Models;

namespace DeepSeekSurveyAnalyzer.Services.Abstractions;

public interface IHistoryService
{
    Task SaveEntryAsync(HistoryEntry entry);
    Task<List<HistoryEntry>> LoadEntriesAsync(int limit = 50);
}