using DeepSeekSurveyAnalyzer.Models;

namespace DeepSeekSurveyAnalyzer.Services.Abstractions;

public interface IDeepSeekService
{
    IAsyncEnumerable<ChatChunk> StreamChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
}