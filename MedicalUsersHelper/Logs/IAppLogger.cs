namespace MedicalUsersHelper.Logs;

public interface IAppLogger
{
    void LogInformation(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
}