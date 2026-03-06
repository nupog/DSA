using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class SwotAnalysisViewModel : INotifyPropertyChanged
{
    private readonly IDeepSeekService _deepSeek;
    private readonly ILoggingService _logger;
    private readonly ChatRequest _request;
    private readonly AnalysisResult _analysisResult;
    private CancellationTokenSource? _cts;
    private string _reasoning = string.Empty;
    private string _swotAnalysis = string.Empty;
    private bool _isCompleted;
    private bool _isCancelled;

    public string Reasoning
    {
        get => _reasoning;
        private set
        {
            _reasoning = value;
            OnPropertyChanged();
        }
    }

    public string SwotAnalysis
    {
        get => _swotAnalysis;
        private set
        {
            _swotAnalysis = value;
            OnPropertyChanged();
            if (_analysisResult != null)
                _analysisResult.SwotAnalysis = value;
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        private set
        {
            _isCompleted = value;
            OnPropertyChanged();
            ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
        }
    }

    public bool IsCancelled
    {
        get => _isCancelled;
        private set
        {
            _isCancelled = value;
            OnPropertyChanged();
        }
    }

    public ICommand CancelCommand { get; }
    public ICommand SaveCommand { get; }

    public SwotAnalysisViewModel(IDeepSeekService deepSeek, ILoggingService logger, ChatRequest request, AnalysisResult analysisResult)
    {
        _deepSeek = deepSeek;
        _logger = logger;
        _request = request;
        _analysisResult = analysisResult;

        CancelCommand = new RelayCommand(Cancel, () => !IsCompleted && !IsCancelled);
        SaveCommand = new RelayCommand(Save, () => IsCompleted && !string.IsNullOrWhiteSpace(SwotAnalysis));

        _ = StartReceivingAsync();
    }

    private async Task StartReceivingAsync()
    {
        _cts = new CancellationTokenSource();
        try
        {
            var reasoningBuilder = new StringBuilder();
            var swotBuilder = new StringBuilder();

            await foreach (var chunk in _deepSeek.StreamChatAsync(_request, _cts.Token))
            {
                var delta = chunk.Choices.FirstOrDefault()?.Delta;
                if (delta == null) continue;

                if (!string.IsNullOrEmpty(delta.ReasoningContent))
                {
                    reasoningBuilder.Append(delta.ReasoningContent);
                    Reasoning = reasoningBuilder.ToString();
                    if (_analysisResult != null)
                        _analysisResult.SwotReasoning = Reasoning;
                }

                if (!string.IsNullOrEmpty(delta.Content))
                {
                    swotBuilder.Append(delta.Content);
                    SwotAnalysis = swotBuilder.ToString();
                }
            }

            IsCompleted = true;
        }
        catch (OperationCanceledException)
        {
            IsCancelled = true;
            _logger.Information("SWOT-анализ отменён пользователем");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при получении SWOT-анализа");
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            IsCancelled = true;
        }
    }

    private void Cancel()
    {
        _cts?.Cancel();
    }

    private void Save()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "txt",
            FileName = "swot_analysis.txt"
        };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var content = $"=== SWOT-АНАЛИЗ ===\n{SwotAnalysis}\n\n=== РАЗМЫШЛЕНИЯ ===\n{Reasoning}";
                System.IO.File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
                System.Windows.MessageBox.Show("SWOT-анализ сохранён.", "Успех", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Ошибка сохранения SWOT-анализа");
                System.Windows.MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}