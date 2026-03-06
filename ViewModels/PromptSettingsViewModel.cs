using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.ViewModels;

public class PromptSettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly bool _isSwotPrompt;
    private string _promptText = string.Empty;

    public string PromptText
    {
        get => _promptText;
        set
        {
            _promptText = value;
            OnPropertyChanged();
        }
    }

    public string WindowTitle => _isSwotPrompt ? "Настройка промта для SWOT-анализа" : "Настройка промта для анализа опросов";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CloseAction { get; set; }

    public PromptSettingsViewModel(ISettingsService settingsService, bool isSwotPrompt = false)
    {
        _settingsService = settingsService;
        _isSwotPrompt = isSwotPrompt;

        var settings = _settingsService.Load();
        _promptText = isSwotPrompt ? settings.SwotPromptText : settings.PromptText;

        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => CloseAction?.Invoke());
    }

    private void Save()
    {
        var settings = _settingsService.Load();
        
        if (_isSwotPrompt)
            settings.SwotPromptText = PromptText;
        else
            settings.PromptText = PromptText;
            
        _settingsService.Save(settings);
        
        CloseAction?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}