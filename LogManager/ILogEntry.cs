namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌紀錄的介面
///     The interface of the log entry
/// </summary>
public interface ILogEntry
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
    ///     The exception string
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
    public Dictionary<string, object>? AdditionalData { get; set; }
}
