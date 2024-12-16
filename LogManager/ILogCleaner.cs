namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌清理介面
///     Interface for log cleaner
/// </summary>
public interface ILogCleaner
{
    /// <summary>
    ///     日誌保留天數
    ///     Retention days of log
    /// </summary>
    public uint RetentionDays { get; set; }

    /// <summary>
    ///     清理日誌
    ///     Cleanup log
    /// </summary>
    /// <returns></returns>
    public Task Cleanup();

    /// <summary>
    ///     清理成功
    ///     Cleanup success
    /// </summary>
    public event EventHandler CleanupSuccess;

    /// <summary>
    ///     清理失敗
    ///     Cleanup failed
    /// </summary>
    public event EventHandler<Exception> CleanupFailed;
}
