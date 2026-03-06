using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using DeepSeekSurveyAnalyzer.Views;
using Microsoft.Win32;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDeepSeekService _deepSeek;
    private readonly ISettingsService _settings;
    private readonly ILoggingService _logger;
    private readonly FileReaderFactory _fileReaderFactory;
    private readonly IHistoryService _historyService;
    private readonly AnalysisService _analysisService;
    private bool _isBusy;
    private string _progressMessage = string.Empty;

    public ObservableCollection<string> SelectedFiles { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((RelayCommand)SendCommand).RaiseCanExecuteChanged();
            ((RelayCommand)GenerateSwotCommand).RaiseCanExecuteChanged();
        }
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set
        {
            _progressMessage = value;
            OnPropertyChanged();
        }
    }

    public ICommand AddFilesCommand { get; }
    public ICommand ClearFilesCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand GenerateSwotCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenPromptSettingsCommand { get; }
    public ICommand OpenSwotPromptSettingsCommand { get; }

    public MainViewModel(
        IDeepSeekService deepSeek,
        ISettingsService settings,
        ILoggingService logger,
        FileReaderFactory fileReaderFactory,
        IHistoryService historyService,
        AnalysisService analysisService)
    {
        _deepSeek = deepSeek;
        _settings = settings;
        _logger = logger;
        _fileReaderFactory = fileReaderFactory;
        _historyService = historyService;
        _analysisService = analysisService;

        AddFilesCommand = new RelayCommand(AddFiles);
        ClearFilesCommand = new RelayCommand(() => SelectedFiles.Clear(), () => SelectedFiles.Any());
        RemoveFileCommand = new RelayCommand<string>(file => { if (file != null) SelectedFiles.Remove(file); });
        SendCommand = new RelayCommand(Send, CanSend);
        GenerateSwotCommand = new RelayCommand(GenerateSwot, CanGenerateSwot);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        OpenPromptSettingsCommand = new RelayCommand(OpenPromptSettings);
        OpenSwotPromptSettingsCommand = new RelayCommand(OpenSwotPromptSettings);
    }

    private void AddFiles()
    {
        var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Поддерживаемые файлы (*.pdf;*.docx;*.xlsx)|*.pdf;*.docx;*.xlsx|PDF files (*.pdf)|*.pdf|Word files (*.docx)|*.docx|Excel files (*.xlsx)|*.xlsx"
        };
        if (dialog.ShowDialog() == true)
        {
            foreach (var file in dialog.FileNames)
                SelectedFiles.Add(file);
        }
    }

    private bool CanSend() => !IsBusy && SelectedFiles.Any();

    private bool CanGenerateSwot() => !IsBusy && _analysisService.GetLatestAnalysis() != null;

    private async void Send()
    {
        try
        {
            IsBusy = true;
            ProgressMessage = "Начинаем обработку...";

            var settings = _settings.Load();

            var progress = new Progress<string>(msg => ProgressMessage = msg);
            var allText = new List<string>();

            foreach (var file in SelectedFiles)
            {
                try
                {
                    var reader = _fileReaderFactory.GetReader(file);
                    if (reader == null)
                    {
                        _logger.Warning("Неподдерживаемый формат файла: {File}", file);
                        continue;
                    }
                    var text = await reader.ReadTextAsync(file, progress);
                    allText.Add($"--- {Path.GetFileName(file)} ---\n{text}");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Ошибка чтения файла: {File}", file);
                    System.Windows.MessageBox.Show($"Ошибка чтения {file}: {ex.Message}", "Ошибка", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }

            if (!allText.Any())
            {
                IsBusy = false;
                return;
            }

            // Используем промт из настроек
            var userContent = $"{settings.PromptText}\n\nСодержимое опросов:\n{string.Join("\n\n", allText)}";
            var request = new ChatRequest
            {
                Model = settings.Model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = "Вы — аналитик опросов. Отвечайте на русском языке." },
                    new() { Role = "user", Content = userContent }
                }
            };

            // Создаём объект для сохранения результата
            var analysisResult = new AnalysisResult
            {
                OriginalPrompt = settings.PromptText
            };
            _analysisService.SaveAnalysis(analysisResult);

            var responseWindow = new ResponseWindow();
            var responseViewModel = new ResponseViewModel(_deepSeek, _logger, request, _historyService, SelectedFiles.ToList(), analysisResult);
            responseWindow.DataContext = responseViewModel;
            responseWindow.Show();

            ProgressMessage = "Готово";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка отправки запроса");
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void GenerateSwot()
    {
        try
        {
            var latestAnalysis = _analysisService.GetLatestAnalysis();
            if (latestAnalysis == null)
            {
                System.Windows.MessageBox.Show("Сначала выполните анализ опроса", "Нет данных", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            IsBusy = true;
            ProgressMessage = "Формируем SWOT-анализ...";

            var settings = _settings.Load();

            // Формируем запрос для SWOT-анализа на основе предыдущего анализа
            var userContent = $"{settings.SwotPromptText}\n\nРезультаты анализа опроса:\n{latestAnalysis.AnalysisText}";
            
            var request = new ChatRequest
            {
                Model = settings.Model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = "Вы — аналитик, специалист по стратегическому планированию. Отвечайте на русском языке." },
                    new() { Role = "user", Content = userContent }
                }
            };

            var swotWindow = new SwotAnalysisWindow();
            var swotViewModel = new SwotAnalysisViewModel(_deepSeek, _logger, request, latestAnalysis);
            swotWindow.DataContext = swotViewModel;
            swotWindow.Show();

            ProgressMessage = "SWOT-анализ сформирован";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка формирования SWOT-анализа");
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenSettings()
    {
        var viewModel = new SettingsViewModel(_settings, _logger);
        var window = new SettingsWindow(viewModel);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    private void OpenPromptSettings()
    {
        var viewModel = new PromptSettingsViewModel(_settings, isSwotPrompt: false);
        var window = new PromptSettingsWindow(viewModel);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    private void OpenSwotPromptSettings()
    {
        var viewModel = new PromptSettingsViewModel(_settings, isSwotPrompt: true);
        var window = new PromptSettingsWindow(viewModel);
        window.Owner = System.Windows.Application.Current.MainWindow;
        window.ShowDialog();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}