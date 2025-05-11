using LoggerServer.Bll;
using LoggerServer.Services;

namespace LoggerServer.Helpers;

internal class Settings : ISettings
{
    private ILogger<LoggingService> _logger;

    public Settings(ILogger<LoggingService> logger)
    {
        _logger = logger;
        LoadCofing();
    }

    public string LogDirectory { get; private set; } = string.Empty;
    public string AppUrl { get; private set; } = string.Empty;

    private void LoadCofing()
    {
        var builder = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false);

        IConfiguration config = builder.Build();

        LogDirectory = LoadLogDirectory(config);
        AppUrl = LoadAppUrl(config);
    }

    private string LoadAppUrl(IConfiguration config)
    {
        string? applicationUrl = config["ServerSettings:ApplicationUrl"];
        if (string.IsNullOrEmpty(applicationUrl))
        {
            applicationUrl = "https://localhost:7014";
        }

        return applicationUrl;
    }

    private string LoadLogDirectory(IConfiguration config)
    {
        IConfigurationSection? section = config.GetSection("LoggerConfig");
        section = section?.GetSection("LogFilesDirectory");
        string? logDirectory = section != null ? section.Value : string.Empty;

        if (string.IsNullOrEmpty(logDirectory))
        {
            logDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        _logger.LogInformation($"Directory of the received files: {logDirectory}");

        return logDirectory;
    }
}
