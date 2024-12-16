namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌檔案路徑產生器介面。
///     The log file path generator interface.
/// </summary>
public interface ILogPathGenerator
{
    /// <summary>
    ///     生成日誌檔案路徑。
    ///     Generates the log file path.
    /// </summary>
    /// <returns>日誌檔案路徑。The log file path.</returns>
    public string GetLogPath(DateTime date);
}