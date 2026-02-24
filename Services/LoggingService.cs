using System.IO;
using Serilog;
using Serilog.Events;
using DeepSeekSurveyAnalyzer.Services.Abstractions;

namespace DeepSeekSurveyAnalyzer.Services;

public class LoggingService : ILoggingService
{
    private readonly ILogger _logger;
    private readonly IConfigurationService _configuration;

    public LoggingService(IConfigurationService configuration)
    {
        _configuration = configuration;
        
        var logLevel = GetLogEventLevel(_configuration.GetLogLevel());
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

    private LogEventLevel GetLogEventLevel(string level) => level switch
    {
        "Debug" => LogEventLevel.Debug,
        "Information" => LogEventLevel.Information,
        "Warning" => LogEventLevel.Warning,
        "Error" => LogEventLevel.Error,
        _ => LogEventLevel.Information
    };

    public void Information(string message, params object[] properties) => 
        _logger.Information(message, properties);

    public void Error(Exception ex, string message, params object[] properties) => 
        _logger.Error(ex, message, properties);

    public void Warning(string message, params object[] properties) => 
        _logger.Warning(message, properties);
}