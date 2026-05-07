namespace WuyiPlay_BLL.IServices;

public interface ILoggerService
{
    void LogError(string message, Exception exception);
    void LogInformation(string message);
    void LogWarning(string message);
    void WriteLogDebug(string title, string debug);
}