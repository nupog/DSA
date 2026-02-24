using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using DeepSeekSurveyAnalyzer.Views;
using Microsoft.Win32;
using DeepSeekSurveyAnalyzer.Services;
using System.IO;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IDeepSeekService _deepSeek;
    private readonly ISettingsService _settings;
    private readonly ILoggingService _logger;
    private readonly FileReaderFactory _fileReaderFactory;
    private string _promptText = string.Empty;
    private bool _isBusy;
    private string _progressMessage = string.Empty;
    private readonly IHistoryService _historyService;

    public ObservableCollection<string> SelectedFiles { get; } = new();

    public string PromptText
    {
        get => _promptText;
        set { _promptText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set { _isBusy = value; OnPropertyChanged(); }
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set { _progressMessage = value; OnPropertyChanged(); }
    }

    public ICommand AddFilesCommand { get; }
    public ICommand ClearFilesCommand { get; }
    public ICommand RemoveFileCommand { get; }
    public ICommand SendCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    public MainViewModel(
        IDeepSeekService deepSeek,
        ISettingsService settings,
        ILoggingService logger,
        FileReaderFactory fileReaderFactory,
        IHistoryService historyService)
    {
        _deepSeek = deepSeek;
        _settings = settings;
        _logger = logger;
        _fileReaderFactory = fileReaderFactory;
        _historyService = historyService;

        var appSettings = _settings.Load();
        PromptText = appSettings.PromptText;

        AddFilesCommand = new RelayCommand(AddFiles);
        ClearFilesCommand = new RelayCommand(() => SelectedFiles.Clear(), () => SelectedFiles.Any());
        RemoveFileCommand = new RelayCommand<string>(file => { if (file != null) SelectedFiles.Remove(file); });
        SendCommand = new RelayCommand(Send, CanSend);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
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

    private bool CanSend() => !IsBusy && SelectedFiles.Any() && !string.IsNullOrWhiteSpace(PromptText);

    private async void Send()
    {
        try
        {
            IsBusy = true;
            ProgressMessage = "Начинаем обработку...";

            var settings = _settings.Load();
            settings.PromptText = PromptText;
            _settings.Save(settings);

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
                    System.Windows.MessageBox.Show($"Ошибка чтения {file}: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }

            if (!allText.Any())
            {
                IsBusy = false;
                return;
            }

            var userContent = $"{PromptText}\n\nСодержимое опросов:\n{string.Join("\n\n", allText)}";
            var request = new ChatRequest
            {
                Model = settings.Model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = "Вы — аналитик опросов. Отвечайте на русском языке." },
                    new() { Role = "user", Content = userContent }
                }
            };

            var responseWindow = new ResponseWindow();
            var responseViewModel = new ResponseViewModel(_deepSeek, _logger, request, _historyService, SelectedFiles.ToList());
            responseWindow.DataContext = responseViewModel;
            responseWindow.Show();

            ProgressMessage = "Готово";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка отправки запроса");
            System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow();
        var settingsViewModel = new SettingsViewModel(_settings, _logger);
        settingsWindow.DataContext = settingsViewModel;
        settingsWindow.Owner = System.Windows.Application.Current.MainWindow;
        settingsWindow.ShowDialog();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}