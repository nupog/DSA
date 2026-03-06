using System;
using System.IO;
using System.Text.Json;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.Services;

public class SettingsService : ISettingsService, IConfigurationService
{
    private readonly string _settingsPath;
    private readonly ILoggingService _logger;
    private AppSettings? _cachedSettings;

    public SettingsService(ILoggingService logger)
    {
        _logger = logger;
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appData, "DeepSeekSurveyAnalyzer");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
    }

    public AppSettings Load()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        try
        {
            if (!File.Exists(_settingsPath))
                return _cachedSettings = new AppSettings();
            
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            
            if (!string.IsNullOrEmpty(settings.ApiKey))
                settings.ApiKey = SecureStorage.Decrypt(settings.ApiKey);
                
            return _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка загрузки настроек");
            return _cachedSettings = new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        try
        {
            var toSave = new AppSettings
            {
                Endpoint = settings.Endpoint,
                PromptText = settings.PromptText,
                SwotPromptText = settings.SwotPromptText,
                Model = settings.Model,
                LogLevel = settings.LogLevel,
                ApiKey = string.IsNullOrEmpty(settings.ApiKey) 
                    ? string.Empty 
                    : SecureStorage.Encrypt(settings.ApiKey)
            };
            
            var json = JsonSerializer.Serialize(toSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка сохранения настроек");
            throw;
        }
    }

    // Реализация IConfigurationService
    public string GetLogLevel()
    {
        return Load().LogLevel;
    }
}