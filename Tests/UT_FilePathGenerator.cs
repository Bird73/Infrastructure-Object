namespace Birdsoft.Utilities.LogManager;

using Birdsoft.Utilities.LogManager;

using Xunit;
using FluentAssertions;

public class UT_FilePathGenerator
{
    /// <summary>
    ///     測試預設的日誌檔案路徑產生器。
    ///     Test the default log path generator.
    /// </summary>
    [Fact]
    public void Test_LogPathGenerator_Default()
    {
        // Arrange
        var logPathGenerator = new LogPathGenerator();

        DateTime testDate = DateTime.Today;
        // 預期路徑
        // Expected path
        string expectedPath = $"Logs\\{testDate:yyyyMM}";

        // Act
        var path = logPathGenerator.GetLogPath(testDate);

        // Assert
        path.Should().Be(expectedPath);
    }

    /// <summary>
    ///     測試絕對路徑的日誌檔案路徑產生器。
    ///     Test the log file path generator with absolute path.
    /// </summary>
    [Fact]
    public void Test_LogPathGenerator_Absolute()
    {
        // Arrange
        var logPathGenerator = new LogPathGenerator(PathType.Absolute, "C:\\Logs");

        DateTime testDate = DateTime.Today;
        // 預期路徑
        // Expected path
        string expectedPath = $"C:\\Logs\\{testDate:yyyyMM}";

        // Act
        var path = logPathGenerator.GetLogPath(testDate);

        // Assert
        path.Should().Be(expectedPath);
    }

    /// <summary>
    ///     測試相對路徑使用不合法字元路徑, 驗證結果應傳回 default 路徑。
    ///     Test the log file path generator with invalid characters in relative path, should return default path.
    /// </summary>
    [Fact]
    public void Test_LogPathGenerator_InvalidRelativePath()
    {
        // Arrange
        var invalidChars = new string(Path.GetInvalidPathChars());

        DateTime testDate = DateTime.Today;

        // 預期路徑
        // Expected path
        string expectedPath = $"Logs\\{testDate:yyyyMM}";

        foreach (var c in invalidChars)
        {
            var logPathGenerator = new LogPathGenerator(PathType.Relative, $"Logs\\InvalidPath_{c}");

            // Act
            var path = logPathGenerator.GetLogPath(testDate);

            // Assert
            path.Should().Be($"Logs\\{testDate:yyyyMM}");
        }
    }

    /// <summary>
    ///     測試預設的日誌檔案名稱產生器。
    ///     Test the default log file name generator.
    /// </summary>
    [Fact]
    public void Test_LogFileNameGenerator_Default()
    {
        // Arrange
        var logFileNameGenerator = new LogFileNameGenerator();

        DateTime testDate = DateTime.Today;
        // 預期檔案名稱
        // Expected file name
        string expectedFileName = $"Log_{testDate:yyyyMMdd}.json";

        // Act
        var fileName = logFileNameGenerator.GetLogFileName(testDate);

        // Assert
        fileName.Should().Be(expectedFileName);
    }
}