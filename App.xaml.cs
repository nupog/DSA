using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using DeepSeekSurveyAnalyzer.Services;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using DeepSeekSurveyAnalyzer.ViewModels;
using DeepSeekSurveyAnalyzer.Views;

namespace DeepSeekSurveyAnalyzer
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<SettingsService>();
            services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
            services.AddSingleton<IConfigurationService>(sp => sp.GetRequiredService<SettingsService>());
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IHistoryService, HistoryService>();
            services.AddSingleton<FileReaderFactory>();
            services.AddSingleton<IDeepSeekService, DeepSeekService>();
            services.AddSingleton<AnalysisService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ResponseViewModel>();
            services.AddTransient<PromptSettingsViewModel>();
            services.AddTransient<SwotAnalysisViewModel>();
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
                var mainWindow = new MainWindow
                {
                    DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
                };
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}