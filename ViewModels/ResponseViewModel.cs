using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class ResponseViewModel : INotifyPropertyChanged
{
    private readonly IDeepSeekService _deepSeek;
    private readonly ILoggingService _logger;
    private readonly IHistoryService _historyService;
    private readonly ChatRequest _request;
    private readonly List<string> _files;
    private CancellationTokenSource? _cts;
    private string _reasoning = string.Empty;
    private string _answer = string.Empty;
    private bool _isCompleted;
    private bool _isCancelled;

    public string Reasoning
    {
        get => _reasoning;
        private set { _reasoning = value; OnPropertyChanged(); }
    }

    public string Answer
    {
        get => _answer;
        private set { _answer = value; OnPropertyChanged(); }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        private set { _isCompleted = value; OnPropertyChanged(); }
    }

    public bool IsCancelled
    {
        get => _isCancelled;
        private set { _isCancelled = value; OnPropertyChanged(); }
    }

    public ICommand CancelCommand { get; }
    public ICommand SaveAnswerCommand { get; }

    public ResponseViewModel(IDeepSeekService deepSeek, ILoggingService logger, ChatRequest request, IHistoryService historyService, List<string> files)
    {
        _deepSeek = deepSeek;
        _logger = logger;
        _historyService = historyService;
        _request = request;
        _files = files;

        CancelCommand = new RelayCommand(Cancel, () => !IsCompleted && !IsCancelled);
        SaveAnswerCommand = new RelayCommand(SaveAnswer, () => IsCompleted && !string.IsNullOrWhiteSpace(Answer));

        _ = StartReceivingAsync();
    }

    private async Task StartReceivingAsync()
    {
        _cts = new CancellationTokenSource();
        try
        {
            var reasoningBuilder = new StringBuilder();
            var answerBuilder = new StringBuilder();

            await foreach (var chunk in _deepSeek.StreamChatAsync(_request, _cts.Token))
            {
                var delta = chunk.Choices.FirstOrDefault()?.Delta;
                if (delta == null) continue;

                if (!string.IsNullOrEmpty(delta.ReasoningContent))
                {
                    reasoningBuilder.Append(delta.ReasoningContent);
                    Reasoning = reasoningBuilder.ToString();
                }

                if (!string.IsNullOrEmpty(delta.Content))
                {
                    answerBuilder.Append(delta.Content);
                    Answer = answerBuilder.ToString();
                }
            }

            IsCompleted = true;

            // Сохраняем историю
            var entry = new HistoryEntry
            {
                Timestamp = DateTime.Now,
                Prompt = _request.Messages.Last(m => m.Role == "user").Content,
                Reasoning = Reasoning,
                Answer = Answer,
                Model = _request.Model,
                Files = _files
            };
            await _historyService.SaveEntryAsync(entry);
        }
        catch (OperationCanceledException)
        {
            IsCancelled = true;
            _logger.Information("Запрос отменён пользователем");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении ответа");
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            IsCancelled = true;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void SaveAnswer()
{
    var dialog = new Microsoft.Win32.SaveFileDialog
    {
        Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
        DefaultExt = "txt",
        FileName = "response.txt"
    };
    if (dialog.ShowDialog() == true)
    {
        try
        {
            var content = $"=== РАЗМЫШЛЕНИЯ ===\n{Reasoning}\n\n=== ОТВЕТ ===\n{Answer}";
            File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
            System.Windows.MessageBox.Show("Ответ сохранён.", "Успех", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка сохранения файла");
            System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}