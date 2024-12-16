namespace Birdsoft.Utilities.LogManager;

/// <summary>
///     日誌檔案名產生器。
///     The log file name generator.
/// </summary>
public class LogFileNameGenerator : ILogFileNameGenerator
{
    /// <summary>
    ///     預設的日誌檔案名產生器。
    ///     The default log file name generator.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private string DefaultGenerator(DateTime date)
    {
        return $"Log_{date:yyyyMMdd}.json";
    }

    /// <summary>
    ///     產生日誌檔案名核心，可覆寫此方法以自訂檔案名。
    ///     Generate the log file name core, override this method to customize the file name.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    protected virtual string GeneratorCore(DateTime date)
    {
        return DefaultGenerator(date);
    }

    /// <summary>
    ///     生成日誌檔案名。
    ///     Generates the log file name.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public string GetLogFileName(DateTime date)
    {
        string logFileName = GeneratorCore(date);

        char[] invalidFileChars = Path.GetInvalidFileNameChars();

        if (string.IsNullOrWhiteSpace(logFileName) || logFileName.IndexOfAny(invalidFileChars) >= 0)
        {
            // 若有問題，回退到預設檔名, If there is a problem, fallback to the default file name.
            logFileName = DefaultGenerator(date);
        }

        return logFileName;
    }
}