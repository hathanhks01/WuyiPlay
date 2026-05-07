using Microsoft.Extensions.Logging;
using WuyiPlay_BLL.IServices;

namespace WuyiPlay_BLL.Services;

public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;

    public LoggerService(ILogger<LoggerService> logger)
    {
        _logger = logger;
    }

    public void LogInformation(string message) =>
        _logger.LogInformation("{Time}: {Message}", DateTime.Now, message);

    public void LogWarning(string message) =>
        _logger.LogWarning("{Time}: {Message}", DateTime.Now, message);

    public void LogError(string message, Exception exception)
    {
        _logger.LogError("_______________________________________");
        _logger.LogError(exception, "{Time}: {Message}", DateTime.Now, message);
        _logger.LogError("_______________________________________");
    }

    public void WriteLogDebug(string title, string debug) =>
        _logger.LogDebug("--------------------------------\n{Time}: {Title}: {Debug}\n--------------------------------",
            DateTime.Now, title, debug);
}
