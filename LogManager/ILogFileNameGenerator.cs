namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     生成日誌檔案名的介面。
///     The interface for generating log file names.
/// </summary>
public interface ILogFileNameGenerator
{
    /// <summary>
    ///     生成日誌檔案名。
    ///     Generates the log file name.
    /// </summary>
    /// <returns>日誌檔案名。The log file name.</returns>
    public string GetLogFileName(DateTime date);
}