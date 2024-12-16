namespace Birdsoft.Utilities.LogManager.Tests;

using Birdsoft.Utilities.LogManager;

using Xunit;
using FluentAssertions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Mail;

/// <summary>
///     測試 WriteLogEntry 方法。
///     Tests the WriteLogEntry method.
/// </summary>
public class UT_WriteLogEntry
{
    /// <summary>
    ///     測試 WriteLogEntry 方法，成功寫入日誌。
    ///     Tests the WriteLogEntry method with a successful log entry.
    /// </summary>
    [Fact]
    public void WriteLogEntry_Success()
    {
        // Arrange
        List<LogLevel> logLevels = new List<LogLevel> { LogLevel.Information, LogLevel.Warning, LogLevel.Error };
        List<string> messages = new List<string> { "Information", "Warning", "Error" };
        List<Exception?> exceptions = new List<Exception?> { null, new Exception("Exception"), new Exception("Error") };
        List<int?> eventIds = new List<int?> { null, 1, 2 };
        List<Dictionary<string, object>?> additionalData = new List<Dictionary<string, object>?> 
        {
            null, 
            new Dictionary<string, object> { { "Key1", "Value1" } }, 
            new Dictionary<string, object> { { "Key2", "Value2" } } 
        };

        // 生成日誌檔案路徑。
        // Generates the log file path.
        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        var logPath = logPathGenerator.GetLogPath(today);

        // 生成日誌檔案名稱。
        // Generates the log file name.
        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();
        var logFileName = logFileNameGenerator.GetLogFileName(today);

        var logFullPath = Path.Combine(logPath, logFileName);       // 日誌檔案完整路徑。 The full path of the log file.

        // 加上 evet LogWritten 測試
        // Add event LogWritten test
        bool eventRaised = false;
        int testNumber = 0;
        LogManager.LogWritten += LogManager_LogWritten;

        // 清除日誌檔案。
        // Clears the log file.
        if (File.Exists(logFullPath))
        {
            File.Delete(logFullPath);
        }

        // Act
        // 寫入日誌。 Writes the log entry.       
        for (; testNumber < logLevels.Count; testNumber++)
        {
            eventRaised = false;
            LogManager.WriteLogEntry(logLevels[testNumber], messages[testNumber], exceptions[testNumber]?.ToString(), eventIds[testNumber], additionalData[testNumber]);
            
            //等待事件觸發
            // Waits for the event to be raised
            while (!eventRaised)
            {
                Thread.Sleep(100);
            }
        }

        // Assert
        void LogManager_LogWritten(object? sender, ILogEntry e)
        {
            eventRaised = true;

            // 驗證事件參數。
            // Verifies the event arguments.
            e.Level.Should().Be(logLevels[testNumber]);
            e.Message.Should().Be(messages[testNumber]);
            e.ExceptionString?.Should().Be(exceptions[testNumber]?.ToString());
            e.EventId.Should().Be(eventIds[testNumber]);
            e.AdditionalData.Should().BeEquivalentTo(additionalData[testNumber]);
        }

        // 驗證是否觸發 LogWritten 事件。
        // Verifies that the LogWritten event was raised.
        eventRaised.Should().BeTrue();

        // 驗證日誌檔案是否存在。
        // Verifies that the log file exists.
        File.Exists(logFullPath).Should().BeTrue();

        // 驗證是否成功寫入日誌。
        // Verifies that the log entry was written successfully.
        var logContent = File.ReadAllText(logFullPath);

        var logEntries = JsonSerializer.Deserialize<List<LogEntry>>(logContent);

        logEntries.Should().HaveCount(3);

        // 驗證每個日誌條目。
        // Verifies each log entry.
        for (int i = 0; i < logEntries.Count; i++)
        {
            logEntries[i].Level.Should().Be(logLevels[i]);
            logEntries[i].Message.Should().Be(messages[i]);
            logEntries[i].ExceptionString?.Should().Be(exceptions[i]?.ToString());
            logEntries[i].EventId.Should().Be(eventIds[i]);

            // 比對 AdditionalData
            var expectedAdditionalData = additionalData[i];
            var actualAdditionalData = logEntries[i].AdditionalData;

            if (expectedAdditionalData == null)
            {
                actualAdditionalData.Should().BeNull();
            }
            else
            {
                actualAdditionalData.Should().NotBeNull();

                // 驗證 key-value
                foreach (var expectedKvp in expectedAdditionalData)
                {
                    actualAdditionalData!.Should().ContainKey(expectedKvp.Key); // 驗證 key 存在

                    // 驗證 value
                    var actualValue = actualAdditionalData[expectedKvp.Key];
                    var expectedValue = expectedKvp.Value;

                    actualValue.ToString().Should().BeEquivalentTo(expectedValue.ToString());
                }

                // 驗證沒有多餘的 key
                actualAdditionalData!.Keys.Should().BeEquivalentTo(expectedAdditionalData.Keys);
            }

            //logEntries[i].AdditionalData.Should().BeEquivalentTo(additionalData[i]);
        }
    }

    /// <summary>
    /// 測試 WriteLogEntry 方法，當檔案被鎖定時，寫入日誌失敗。
    /// Tests the WriteLogEntry method when the log entry fails due to a locked file.
    /// </summary>
    [Fact]
    public void WriteLogEntry_Failure_FileLocked()
    {
        // Arrange
        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        var logPath = logPathGenerator.GetLogPath(today);

        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();
        var logFileName = logFileNameGenerator.GetLogFileName(today);

        var logFullPath = Path.Combine(logPath, logFileName);

        // 加上 event LogWriteFailed 測試
        // Add event LogWriteFailed test
        bool eventRaised = false;
        int eventTimes = 0;
        LogManager.LogWriteFailed += LogManager_LogWriteFailed;

        // 確保目標日誌檔案存在
        if (!File.Exists(logFullPath))
        {
            File.Create(logFullPath).Dispose(); // 建立並釋放檔案
        }

        // Act

        // 使用 FileStream 鎖定檔案，模擬檔案被佔用
        using (var fileStream = new FileStream(logFullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // 嘗試寫入，預期會因檔案被鎖定而失敗，並拋出 IOException
            LogManager.WriteLogEntry(LogLevel.Error, "Test error message");
        }

        // Assert
        void LogManager_LogWriteFailed(object? sender, LogWriteFailedEventArgs e)
        {
            eventRaised = true;
            eventTimes++;

            // 驗證事件參數
            // Verifies the event arguments
            e.LogEntry.Level.Should().Be(LogLevel.Error);
            e.LogEntry.Message.Should().Be("Test error message");
            e.Exception.Should().BeOfType<IOException>();
        }

        // 等待事件觸發
        // Waits for the event to be raised
        while (!eventRaised)
        {
            Thread.Sleep(100);
        }

        // 驗證是否觸發 LogWriteFailed 事件
        // Verifies that the LogWriteFailed event was raised
        eventRaised.Should().BeTrue();
        eventTimes.Should().Be(1);
    }

    /// <summary>
    ///     非同步測試 WriteLogEntry 方法，成功寫入日誌。
    ///     Tests the WriteLogEntry method asynchronously with a successful log entry.
    [Fact]
    public async Task WriteLogEntryAsync_Success()
    {
        // Arrange
        var today = DateTime.Today;
        ILogPathGenerator logPathGenerator = new LogPathGenerator();
        var logPath = logPathGenerator.GetLogPath(today);

        ILogFileNameGenerator logFileNameGenerator = new LogFileNameGenerator();
        var logFileName = logFileNameGenerator.GetLogFileName(today);

        var logFullPath = Path.Combine(logPath, logFileName);

        // 加上 event LogWriteFailed 測試
        // Add event LogWriteFailed test
        bool eventRaised = false;
        int eventTimes = 0;
        LogManager.LogWriteFailed += LogManager_LogWriteFailed;

        // 確保目標日誌檔案存在
        if (!File.Exists(logFullPath))
        {
            File.Create(logFullPath).Dispose(); // 建立並釋放檔案
        }

        // Act

        // 使用 FileStream 鎖定檔案，模擬檔案被佔用
        using (var fileStream = new FileStream(logFullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            // 嘗試寫入，預期會因檔案被鎖定而失敗，並拋出 IOException
            await LogManager.WriteLogEntryAsync(LogLevel.Error, "Test error message");
        }

        // Assert
        void LogManager_LogWriteFailed(object? sender, LogWriteFailedEventArgs e)
        {
            eventRaised = true;
            eventTimes++;

            // 驗證事件參數
            // Verifies the event arguments
            e.LogEntry.Level.Should().Be(LogLevel.Error);
            e.LogEntry.Message.Should().Be("Test error message");
            e.Exception.Should().BeOfType<IOException>();
        }

        // 等待事件觸發
        // Waits for the event to be raised
        while (!eventRaised)
        {
            Thread.Sleep(100);
        }

        // 驗證是否觸發 LogWriteFailed 事件
        // Verifies that the LogWriteFailed event was raised
        eventRaised.Should().BeTrue();
        eventTimes.Should().Be(1);
    }
}