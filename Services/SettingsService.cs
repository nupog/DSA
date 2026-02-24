using System.Text.Json;
using DeepSeekSurveyAnalyzer.Models;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using System.IO;

namespace DeepSeekSurveyAnalyzer.Services;

public class SettingsService : ISettingsService, IConfigurationService
{
    private readonly string _settingsPath;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appData, "DeepSeekSurveyAnalyzer");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return new AppSettings();
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            if (!string.IsNullOrEmpty(settings.ApiKey))
                settings.ApiKey = SecureStorage.Decrypt(settings.ApiKey);
            return settings;
        }
        catch (Exception ex)
        {
            throw;
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
                Model = settings.Model,
                LogLevel = settings.LogLevel,
                ApiKey = string.IsNullOrEmpty(settings.ApiKey) ? string.Empty : SecureStorage.Encrypt(settings.ApiKey)
            };
            var json = JsonSerializer.Serialize(toSave, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    

    // Реализация IConfigurationService
    string IConfigurationService.GetLogLevel()
    {
        return Load().LogLevel;
    }
}