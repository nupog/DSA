using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private string _apiKey = string.Empty;
    private string _endpoint = string.Empty;
    private string _model = string.Empty;
    private string _logLevel = string.Empty;

    public string ApiKey
    {
        get => _apiKey;
        set { _apiKey = value; OnPropertyChanged(); }
    }

    public string Endpoint
    {
        get => _endpoint;
        set { _endpoint = value; OnPropertyChanged(); }
    }

    public string Model
    {
        get => _model;
        set { _model = value; OnPropertyChanged(); }
    }

    public string LogLevel
    {
        get => _logLevel;
        set { _logLevel = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        var settings = settingsService.Load();
        ApiKey = settings.ApiKey;
        Endpoint = settings.Endpoint;
        Model = settings.Model;
        LogLevel = settings.LogLevel;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => Close?.Invoke());
    }

    public Action? Close { get; set; }

    private void Save()
    {
        var settings = new AppSettings
        {
            ApiKey = ApiKey,
            Endpoint = Endpoint,
            Model = Model,
            LogLevel = LogLevel,
            PromptText = _settingsService.Load().PromptText
        };
        _settingsService.Save(settings);
        LoggingService.LogInformation("Настройки сохранены");
        Close?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}