namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌紀錄
///     The interface of the log entry
/// </summary>
public class LogEntry : ILogEntry
{
    /// <summary>
    ///     日誌紀錄的等級
    ///     The level of the log entry
    /// </summary>
    public LogLevel Level { get; set; }

    /// <summary>
    ///     日誌紀錄的訊息
    ///     The message of the log entry
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     時間戳記
    ///     The timestamp
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    ///     例外
    ///     The exception
    /// </summary>
    public string? ExceptionString { get; set; }

    /// <summary>
    ///     事件 ID
    ///     The event ID
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    ///     額外的資料
    ///     Additional data
    /// </summary>
    public virtual Dictionary<string, object>? AdditionalData { get; set; }

    public LogEntry(LogLevel level, string message, string? exceptionString = null, int? eventId = null, Dictionary<string, object>? additionalData = null)
    {
        Level = level;
        Message = message;
        Timestamp = DateTime.Now;
        ExceptionString = exceptionString;
        EventId = eventId;
        AdditionalData = additionalData;
    }
}