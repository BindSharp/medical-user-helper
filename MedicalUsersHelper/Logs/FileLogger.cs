using System.Collections.Concurrent;

namespace MedicalUsersHelper.Logs;

public sealed class FileLogger : IAppLogger, IDisposable
{
    private readonly string _logDirectory;
    private readonly BlockingCollection<string> _logQueue;
    private readonly Task _writerTask;
    private readonly CancellationTokenSource _cts;
    private string _currentLogFile;
    private DateTime _currentLogDate;

    public FileLogger(string logDirectory)
    {
        _logDirectory = logDirectory;
        Directory.CreateDirectory(_logDirectory);
        
        _logQueue = new BlockingCollection<string>();
        _cts = new CancellationTokenSource();
        _currentLogDate = DateTime.Today;
        _currentLogFile = GetLogFilePath(_currentLogDate);
        
        // Start background writer task
        _writerTask = Task.Run(WriteLogsAsync);
    }

    private string GetLogFilePath(DateTime date)
    {
        return Path.Combine(_logDirectory, $"medical-helper-{date:yyyy-MM-dd}.log");
    }

    private async Task WriteLogsAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            try
            {
                if (!_logQueue.TryTake(out string? logEntry, 100, _cts.Token))
                    continue;
                
                if (DateTime.Today != _currentLogDate)
                {
                    _currentLogDate = DateTime.Today;
                    _currentLogFile = GetLogFilePath(_currentLogDate);
                }

                await File.AppendAllTextAsync(_currentLogFile, logEntry + Environment.NewLine, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Swallow logging errors to prevent crashes
            }
        }
    }

    private void Log(string level, string message, params object[] args)
    {
        try
        {
            string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {formattedMessage}";
            _logQueue.Add(logEntry);
        }
        catch
        {
            // Swallow formatting errors
        }
    }

    public void LogInformation(string message, params object[] args) => Log("INFO", message, args);
    public void LogDebug(string message, params object[] args) => Log("DEBUG", message, args);
    public void LogWarning(string message, params object[] args) => Log("WARN", message, args);
    
    public void LogError(Exception ex, string message, params object[] args)
    {
        string formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
        Log("ERROR", $"{formattedMessage} | Exception: {ex.Message} | StackTrace: {ex.StackTrace}");
    }

    public void Dispose()
    {
        _cts.Cancel();
        _logQueue.CompleteAdding();
        _writerTask.Wait(TimeSpan.FromSeconds(5)); // Wait for pending logs to flush
        _logQueue.Dispose();
        _cts.Dispose();
    }
}