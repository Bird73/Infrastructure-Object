namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     LogCleanerBase 抽象類別
///     LogCleanerBase abstract class
/// </summary>
public abstract class LogCleanerBase : ILogCleaner
{
    /// <summary>
    ///     LogPathGenerator 物件，用來產生 Log 檔案路徑
    ///     LogPathGenerator object, used to generate log file path
    /// </summary>
    protected readonly ILogPathGenerator _logPathGenerator;

    /// <summary>
    ///     LogFileNameGenerator 物件，用來產生 Log 檔案名稱
    ///     LogFileNameGenerator object, used to generate log file name
    /// </summary>
    protected readonly ILogFileNameGenerator _logFileNameGenerator;

    /// <summary>
    ///     Log 檔保留天數
    ///     Log file retention days
    /// </summary>
    protected uint _retentionDays;

    /// <summary>
    ///     建構涵式
    ///     Constructor
    /// </summary>
    /// <param name="logPathGenerator"></param>
    /// <param name="logFileNameGenerator"></param>
    /// <param name="retentionDays"></param>
    protected LogCleanerBase(ILogPathGenerator logPathGenerator, ILogFileNameGenerator logFileNameGenerator, uint? retentionDays)
    {
        this._logPathGenerator = logPathGenerator;
        this._logFileNameGenerator = logFileNameGenerator;

        if (retentionDays == null)
        {
            this._retentionDays = 30;
        }
        else
        {
            this._retentionDays = retentionDays.Value;
        }
    }

    /// <summary>
    ///     Log 檔保留天數
    ///     Log file retention days
    /// </summary>
    public uint RetentionDays
    {
        get
        {
            return this._retentionDays;
        }

        set
        {
            this._retentionDays = value;
        }
    }

    /// <summary>
    ///     清除 Log 檔案
    ///     Clean up log files
    /// </summary>
    public abstract Task Cleanup();

    /// <summary>
    ///     清理成功
    ///     Cleanup success
    /// </summary>
    public event EventHandler CleanupSuccess = delegate { };

    /// <summary>
    ///     清理失敗
    ///     Cleanup failed
    /// </summary>
    public event EventHandler<Exception> CleanupFailed = delegate { };

    /// <summary>
    ///     執行清理失敗事件
    ///     Execute cleanup failed event
    /// </summary>
    /// <param name="ex"></param>
    protected void OnCleanupFailed(Exception ex)
    {
        CleanupFailed?.Invoke(this, ex);
    }

    /// <summary>
    ///     執行清理成功事件
    ///     Execute cleanup success event
    /// </summary>
    protected void OnCleanupSuccess()
    {
        CleanupSuccess?.Invoke(this, EventArgs.Empty);
    }
}