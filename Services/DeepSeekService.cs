using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using System.Net.Http;
using System.IO;
using System.Text;

namespace DeepSeekSurveyAnalyzer.Services;

public class DeepSeekService : IDeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly ISettingsService _settingsService;

    public DeepSeekService(HttpClient httpClient, ISettingsService settingsService)
    {
        _httpClient = httpClient;
        _settingsService = settingsService;
    }

    public async IAsyncEnumerable<ChatChunk> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var settings = _settingsService.Load();
    if (string.IsNullOrEmpty(settings.ApiKey))
        throw new InvalidOperationException("API Key не настроен");

    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, settings.Endpoint);
    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
    httpRequest.Content = JsonContent.Create(request);

    LoggingService.LogInformation("Отправка запроса к DeepSeek. Модель: {Model}", request.Model);

    using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    response.EnsureSuccessStatusCode();

    using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
    using var reader = new StreamReader(stream);

    string? line;
    while ((line = await reader.ReadLineAsync()) != null)
    {
        if (cancellationToken.IsCancellationRequested)
            yield break;

        if (line.StartsWith("data: "))
        {
            var data = line["data: ".Length..];
            if (data == "[DONE]")
                break;

            ChatChunk? chunk = null;
            try
            {
                chunk = System.Text.Json.JsonSerializer.Deserialize<ChatChunk>(data);
            }
            catch (Exception ex)
            {
                LoggingService.LogError(ex, "Ошибка десериализации чанка: {Data}", data);
                continue;
            }
            if (chunk != null)
                yield return chunk;
        }
    }
}
}
