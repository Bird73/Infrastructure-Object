namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌寫入器。
///     The log writer.
/// </summary>
public interface ILogWriter
{
    /// <summary>
    ///     寫入日誌。
    ///     Writes a log entry.
    /// </summary>
    public Task WriteLogEntry(ILogEntry logEntry);

    /// <summary>
    ///     寫入日誌事件。
    ///     The event that is raised when a log entry is written.
    /// </summary>
    public event EventHandler<ILogEntry> LogWritten;

    /// <summary>
    ///     寫入日誌失敗事件。
    ///     The event that is raised when a log entry fails to be written.
    /// </summary>
    public event EventHandler<LogWriteFailedEventArgs> LogWriteFailed;
}