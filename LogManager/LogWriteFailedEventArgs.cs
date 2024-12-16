namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌寫入失敗事件參數。
///     The event arguments for the log write failed event.
/// </summary>
public class LogWriteFailedEventArgs : EventArgs
{
    public ILogEntry LogEntry { get; }
    public Exception Exception { get; }

    public LogWriteFailedEventArgs(ILogEntry logEntry, Exception exception)
    {
        LogEntry = logEntry;
        Exception = exception;
    }
}