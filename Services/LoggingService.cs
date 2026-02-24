using System.IO;
using Serilog;
using Serilog.Events;
using DeepSeekSurveyAnalyzer.Services.Abstractions;
using Serilog.Core;

namespace DeepSeekSurveyAnalyzer.Services;

public static class LoggingService
{
    private static Logger? _logger;

    public static void Init(IConfigurationService configuration)
    {
        var logLevel = GetLogEventLevel(configuration.GetLogLevel());
        var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log-.txt");
        var logDir = Path.GetDirectoryName(logPath);
        if (!string.IsNullOrEmpty(logDir))
        {
            Directory.CreateDirectory(logDir);
        }

        _logger = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private static LogEventLevel GetLogEventLevel(string level) => level switch
    {
        "Debug" => LogEventLevel.Debug,
        "Information" => LogEventLevel.Information,
        "Warning" => LogEventLevel.Warning,
        "Error" => LogEventLevel.Error,
        _ => LogEventLevel.Information
    };

    public static void LogInformation(string message, params object[] properties) =>
        _logger?.Information(message, properties);

    public static void LogError(Exception ex, string message, params object[] properties) =>
        _logger?.Error(ex, message, properties);

    public static void LogWarning(string message, params object[] properties) =>
        _logger?.Warning(message, properties);
}