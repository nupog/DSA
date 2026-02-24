using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeepSeekSurveyAnalyzer.Services;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using DeepSeekSurveyAnalyzer.ViewModels;
using DeepSeekSurveyAnalyzer.Views;

namespace DeepSeekSurveyAnalyzer;

public partial class App : Application
{
    private readonly ServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        LoggingService.Init(_serviceProvider.GetRequiredService<IConfigurationService>());
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Регистрируем SettingsService как синглтон
        services.AddSingleton<SettingsService>();
        
        // Регистрируем интерфейсы, используя один и тот же экземпляр
        services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
        services.AddSingleton<IConfigurationService>(sp => sp.GetRequiredService<SettingsService>());
        
        // Остальные сервисы
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<FileReaderFactory>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<SettingsViewModel>();

        // HttpClient + сервис DeepSeek
        services.AddHttpClient<IDeepSeekService, DeepSeekService>(client =>
        {
            client.Timeout = TimeSpan.FromMinutes(5);
        });
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var mainWindow = new Views.MainWindow
            {
                DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
            };
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при запуске: {ex.Message}\n\n{ex.StackTrace}", 
                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    public static new App Current => (App)Application.Current;
    public IServiceProvider Services => _serviceProvider;
}