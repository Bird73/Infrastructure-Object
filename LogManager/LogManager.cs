namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌管理。
///     The log manager.
/// </summary>
public static class LogManager
{
    // 日誌保留天數，預設為 30 天。retention days for log entries, default is 30 days.
    private static uint _retentionDays = 30;

    // 日誌路徑產生器。The log path generator.
    private static ILogPathGenerator _pathGenerator = new LogPathGenerator();

    // 日誌檔案名稱產生器。The log file name generator.
    private static ILogFileNameGenerator _fileNameGenerator = new LogFileNameGenerator();

    // 日誌寫入器。The log writer.
    private static ILogWriter _logWriter = new JsonFileLogWriter(_pathGenerator, _fileNameGenerator);

    // 日誌清理器。The log cleaner.
    private static ILogCleaner _logCleaner = new FileLogCleaner(_pathGenerator, _fileNameGenerator, _retentionDays);

    /// <summary>
    ///     日誌保留天數。
    ///     The number of days to retain log entries.
    /// </summary>
    public static uint RetentionDays
    {
        get => _retentionDays;
        set
        {
            if (value > 0)
            {
                _retentionDays = value;

                if (_logCleaner != null)
                {
                    _logCleaner.RetentionDays = value;
                }
            }
            else
            {
                // 如果設定值小於等於 0，則設定為 30 天。If the value is less than or equal to 0, set it to 30 days.
                _logCleaner.RetentionDays = 30;
            }
        }
    }
    
    static LogManager()
    {
        // 初始化日誌管理
        Initialize(null, null, null, null);

        // 訂閱日誌寫入事件
        _logWriter.LogWritten += (sender, logEntry) => OnLogWritten(logEntry);

        // 訂閱日誌寫入失敗事件
        _logWriter.LogWriteFailed += (sender, args) => OnLogWriteFailed(args.LogEntry, args.Exception);

        // 清除日誌成功事件
        _logCleaner.CleanupSuccess += (sender, e) => CleanupSuccess?.Invoke(null, EventArgs.Empty);

        // 清除日誌失敗事件
        _logCleaner.CleanupFailed += (sender, ex) => CleanupFailed?.Invoke(null, ex);
    }

    /// <summary>
    ///     初始化日誌管理。
    ///     Initializes the log manager.
    /// </summary>
    /// <param name="pathGenerator"></param>
    /// <param name="fileNameGenerator"></param>
    /// <param name="writer"></param>
    /// <param name="logCleaner"></param>
    public static void Initialize(ILogPathGenerator? logPathGenerator, ILogFileNameGenerator? logFileNameGenerator, ILogWriter? logWriter, ILogCleaner? logCleaner)
    {
        bool isGeneratorChanged = false;
        
        if (logPathGenerator != null)
        {
            _pathGenerator = logPathGenerator;
            isGeneratorChanged = true;
        }

        if (logFileNameGenerator != null)
        {
            _fileNameGenerator = logFileNameGenerator;
            isGeneratorChanged = true;
        }

        if (logWriter != null)
        {
            _logWriter = logWriter;
        }
        else if (isGeneratorChanged)
        {
            // 如果有變更產生器，則重新建立日誌寫入器
            _logWriter = new JsonFileLogWriter(_pathGenerator, _fileNameGenerator);
        }

        if (logCleaner != null)
        {
            _logCleaner = logCleaner;
        }
        else if (isGeneratorChanged)
        {
            // 如果有變更產生器，則重新建立日誌清理器
            _logCleaner = new FileLogCleaner(_pathGenerator, _fileNameGenerator, _retentionDays);
        }
    }

    /// <summary>
    ///     寫入日誌。
    ///     Writes a log entry.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <param name="eventId"></param>
    /// <param name="additionalData"></param>
    /// <returns></returns>
    public static void WriteLogEntry(LogLevel level, string message, string? exceptionString = null, int? eventId = null, Dictionary<string, object>? additionalData = null)
    {
        try
        {
            _logWriter.WriteLogEntry(new LogEntry(level, message, exceptionString, eventId, additionalData));
        }
        catch (Exception ex)
        {
            OnLogWriteFailed(new LogEntry(level, message, exceptionString, eventId, additionalData), ex);
        }
    }

    /// <summary>
    ///     寫入日誌。
    ///     Writes a log entry.
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exceptionString"></param>
    /// <param name="eventId"></param>
    /// <param name="additionalData"></param>
    /// <returns></returns>
    public static async Task WriteLogEntryAsync(LogLevel level, string message, string? exceptionString = null, int? eventId = null, Dictionary<string, object>? additionalData = null)
    {
        try
        {
            await _logWriter.WriteLogEntry(new LogEntry(level, message, exceptionString, eventId, additionalData));
        }
        catch (Exception ex)
        {
            OnLogWriteFailed(new LogEntry(level, message, exceptionString, eventId, additionalData), ex);
        }
    }

    /// <summary>
    ///     清除日誌。
    ///     Clears the log.
    /// </summary>
    public static void Cleanup()
    {
        try
        {
            _logCleaner.Cleanup();
            CleanupSuccess?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            CleanupFailed?.Invoke(null, ex);
        }
    }

    /// <summary>
    ///     清除日誌。
    ///     Clears the log.
    /// </summary>
    /// <returns></returns>
    public static async Task CleanupAsync()
    {
        try
        {
            await _logCleaner.Cleanup();
            CleanupSuccess?.Invoke(null, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            CleanupFailed?.Invoke(null, ex);
        }
    }

    /// <summary>
    ///     寫入日誌事件。
    ///     The event that is raised when a log entry is written.
    /// </summary>
    public static event EventHandler<ILogEntry>? LogWritten;

    /// <summary>
    ///     寫入日誌失敗事件。
    ///     The event that is raised when a log entry fails to be written.
    /// </summary>
    public static event EventHandler<LogWriteFailedEventArgs>? LogWriteFailed;

    /// <summary>
    ///     清除日誌成功事件。
    ///     The event that is raised when the log is successfully cleared.
    /// </summary>
    public static event EventHandler? CleanupSuccess;

    /// <summary>
    ///     清除日誌失敗事件。
    ///     The event that is raised when the log fails to be cleared.
    /// </summary>
    public static event EventHandler<Exception>? CleanupFailed;

    /// <summary>
    ///     觸發寫入日誌事件。
    /// </summary>
    /// <param name="logEntry"></param>
    private static void OnLogWritten(ILogEntry logEntry)
    {
        LogWritten?.Invoke(null, logEntry);
    }

    /// <summary>
    ///     觸發寫入日誌失敗事件。
    /// </summary>
    /// <param name="logEntry"></param>
    /// <param name="ex"></param>
    private static void OnLogWriteFailed(ILogEntry logEntry, Exception ex)
    {
        LogWriteFailed?.Invoke(null, new LogWriteFailedEventArgs(logEntry, ex));
    }
}
