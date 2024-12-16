namespace Birdsoft.Utilities.LogManager;

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
///     預設的日誌檔案路徑產生器。
///     The default log path generator.
/// </summary>
public class LogPathGenerator : ILogPathGenerator
{
    /// <summary>
    ///     路徑類型。
    ///     The path type.
    /// </summary>
    private PathType _pathType;

    /// <summary>
    ///     路徑。
    ///     The path.
    /// </summary>
    private string _path;

    public LogPathGenerator()
    {
        _pathType = PathType.Relative;
        _path = "Logs";
    }

    /// <summary>
    ///     建構函式。
    ///     The constructor.
    /// </summary>
    /// <param name="pathType"></param>
    /// <param name="path"></param>
    public LogPathGenerator(PathType pathType, string path)
    {
        _pathType = pathType;
        char invalidPathChar = Path.GetInvalidPathChars().FirstOrDefault();
        if (string.IsNullOrWhiteSpace(path) || path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            _pathType = PathType.Relative;
            _path = "Logs";
        }
        else
        {
            _path = path;

            if (pathType == PathType.Absolute)
            {
                // 用 Regular Expression 檢查路徑是否包含磁磁代號且不是根目錄
                // Use Regular Expression to check if the path contains a drive letter and is not the root directory
                if (!Regex.IsMatch(path, @"^[a-zA-Z]:\\(?:[^\\]+\\?)+$"))
                {
                    _pathType = PathType.Relative;
                    _path = "Logs";
                }
            }
        }
    }

    /// <summary>
    ///     預設的日誌檔案路徑產生器。
    ///     The default log path generator.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    private string DefaultGenerator(DateTime date)
    {
        return $"{_path}\\{date:yyyyMM}";
    }

    /// <summary>
    ///     產生日誌檔案路徑核心，可覆寫此方法以自訂路徑。
    ///     Generate the log file path core, override this method to customize the path.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    protected virtual string GeneratorCore(DateTime date)
    {
        return DefaultGenerator(date);
    }

    /// <summary>
    ///     生成日誌檔案路徑。
    ///     Generates the log file path.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public string GetLogPath(DateTime date)
    {
        return GeneratorCore(date);
    }
}