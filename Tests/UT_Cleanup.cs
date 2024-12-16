namespace Birdsoft.Utilities.LogManager.Tests;

using Birdsoft.Utilities.LogManager;

using Xunit;
using FluentAssertions;

using Xunit.Abstractions;

/// <summary>
///     測試清理日誌檔案
///     Test cleanup log files
/// </summary>
public class UT_Cleanup
{
    /// <summary>
    ///     清理日誌檔案
    ///     Cleanup log files
    /// </summary>
    [Fact]
    public void Cleanup()
    {
        // Arrange
        uint retentionDays = 7;         // 測試保留天數, Test retention days
        int testDays = 1000;            // 測試天數, Test days

        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();

        LogManager.RetentionDays = retentionDays;       // 設定保留天數, Set retention days

        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            // 如果路徑不存在, 則建立路徑, If the path does not exist, create the path
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // 建立一個空的日誌檔案, Create an empty log file
            File.WriteAllText(logFullPath, "");
        }

        bool cleanupSuccess = false;
        LogManager.CleanupSuccess += (sender, e) =>
        {
            cleanupSuccess = true;
        };

        // Act
        LogManager.Cleanup();

        // Assert
        // 驗證是否只保留7天的日誌, Verify that only 7 days of logs are kept
        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            if (i < retentionDays)
            {
                File.Exists(logFullPath).Should().BeTrue();
            }
            else
            {
                File.Exists(logFullPath).Should().BeFalse();
            }
        }

        cleanupSuccess.Should().BeTrue();
    }

    /// <summary>
    ///    清理日誌檔案保留天數為 0
    ///    Cleanup log files retention days is 0
    /// </summary>
    [Fact]
    public void Cleanup_RetentionDays_0()
    {
        // Arrange
        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();

        int testDays = 1000;

        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            // 如果路徑不存在, 則建立路徑, If the path does not exist, create the path
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // 建立一個空的日誌檔案, Create an empty log file
            File.WriteAllText(logFullPath, "");
        }

        LogManager.RetentionDays = 0;       // 保留0天的日誌, Keep 0 days logs

        // 設定清理成功事件, Set cleanup success event
        bool cleanupSuccess = false;
        LogManager.CleanupSuccess += (sender, e) =>
        {
            cleanupSuccess = true;
        };

        // Act
        LogManager.Cleanup();

        // Assert
        // 當保留天數為 0 時, 自動變更為預設 30 天, When the retention days is 0, it is automatically changed to the default 30 days
        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            if (i < 30)
            {
                File.Exists(logFullPath).Should().BeTrue();
            }
            else
            {
                File.Exists(logFullPath).Should().BeFalse();
            }
        }

        cleanupSuccess.Should().BeTrue();
    }

    /// <summary>
    ///     清理日誌檔案失敗
    ///     Cleanup log files failed
    /// </summary>
    [Fact]
    public async void Cleanup_Fail()
    {
        // Arrange
        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();

        int testDays = 1000;

        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            // 如果路徑不存在, 則建立路徑, If the path does not exist, create the path
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }

            // 建立一個空的日誌檔案, Create an empty log file
            File.WriteAllText(logFullPath, "");
        }

        // 鎖定日誌檔案 : today.AddDays(-10)
        // Lock log file : today.AddDays(-10)
        var lockDate = today.AddDays(-10);
        var lockLogFileName = logFileNameGenerator.GetLogFileName(lockDate);
        var lockLogPath = logPathGenerator.GetLogPath(lockDate);
        var lockLogFullPath = Path.Combine(lockLogPath, lockLogFileName);
        using (File.Open(lockLogFullPath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            LogManager.RetentionDays = 7;       // 保留7天的日誌, Keep 7 days logs

            // 設定清理失敗事件, Set cleanup failed event
            bool cleanupFail = false;
            LogManager.CleanupFailed += (sender, e) =>
            {
                cleanupFail = true;
            };

            // Act
            // 清理失敗, Cleanup failed
            LogManager.Cleanup();

            // 等候清理失敗事件觸發半秒, Wait for the cleanup failed event to trigger half a second
            await Task.Delay(500);

            // Assert
            cleanupFail.Should().BeTrue();
        }

        // 清理測試檔案, Cleanup test files
        for (int i = 0; i < testDays; i++)
        {
            var testDate = today.AddDays(-i);
            var logFileName = logFileNameGenerator.GetLogFileName(testDate);
            var logPath = logPathGenerator.GetLogPath(testDate);
            var logFullPath = Path.Combine(logPath, logFileName);

            if (File.Exists(logFullPath))
                File.Delete(logFullPath);
        }
    }
}