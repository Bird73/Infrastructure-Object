namespace Birdsoft.Utilities.LogManager;

using System.Text.Json;

/// <summary>
///     以 JSON 格式寫入日誌的實作。
///     Write log entries in JSON format.
/// </summary>
public class JsonFileLogWriter : ILogWriter
{
    /// <summary>
    ///     日誌路徑產生器。
    ///     Log path generator.
    /// </summary>
    private readonly ILogPathGenerator _pathGenerator;

    /// <summary>
    ///     日誌檔案名稱產生器。
    ///     Log file name generator.
    /// </summary>
    private readonly ILogFileNameGenerator _fileNameGenerator;

    /// <summary>
    ///     建構函式。
    ///     Constructor.
    /// </summary>
    /// <param name="pathGenerator"></param>
    /// <param name="fileNameGenerator"></param>
    public JsonFileLogWriter(ILogPathGenerator pathGenerator, ILogFileNameGenerator fileNameGenerator)
    {
        _pathGenerator = pathGenerator;
        _fileNameGenerator = fileNameGenerator;
    }

    /// <summary>
    ///     寫入日誌。
    /// </summary>
    /// <param name="logEntry"></param>
    /// <returns></returns>
    public async Task WriteLogEntry(ILogEntry logEntry)
    {
        try
        {
            DateTime now = DateTime.Now;
            string path = _pathGenerator.GetLogPath(now);
            string fileName = _fileNameGenerator.GetLogFileName(now);
            string fullPath = Path.Combine(path, fileName);

            EnsureDirectoryExists(path);

            var logEntries = await ReadLogEntriesAsync(fullPath);
            await WriteLogEntryAsync(fullPath, logEntries, logEntry);

            // 觸發成功事件
            OnLogWritten(logEntry);
        }
        catch (Exception ex)
        {
            // 觸發失敗事件
            OnLogWriteFailed(logEntry, ex);
        }
    }

    /// <summary>
    ///     確保目錄存在，若不存在則建立。
    ///     Ensure directory exists, create if not exists.
    /// </summary>
    /// <param name="path"></param>
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    ///    讀取日誌條目。
    ///    Read log entries.
    /// </summary>
    private async Task<List<JsonElement>> ReadLogEntriesAsync(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            return new List<JsonElement>();
        }

        await using var readStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, FileOptions.Asynchronous);
        using var jsonDocument = await JsonDocument.ParseAsync(readStream);

        // 克隆每個 JsonElement，確保脫離 JsonDocument 的生命周期
        return jsonDocument.RootElement.EnumerateArray()
            .Select(element => element.Clone())
            .ToList();
    }

    private async Task WriteLogEntryAsync(string fullPath, List<JsonElement> logEntries, ILogEntry newLogEntry)
    {
        // 將新日誌轉為 JsonElement 並加入列表
        logEntries.Add(ConvertToJsonElement(newLogEntry));

        // 寫回檔案
        await using var writeStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, FileOptions.Asynchronous);
        var options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };
        await JsonSerializer.SerializeAsync(writeStream, logEntries, options);
    }

    private JsonElement ConvertToJsonElement(ILogEntry logEntry)
    {
        var json = JsonSerializer.Serialize(logEntry);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();    // Clone 以避免 Dispose 影響
    }

    /// <summary>
    ///     寫入日誌事件。
    /// </summary>
    public event EventHandler<ILogEntry> LogWritten = delegate { };

    /// <summary>
    ///      寫入日誌失敗事件。
    /// </summary>
    public event EventHandler<LogWriteFailedEventArgs> LogWriteFailed = delegate { };

    /// <summary>
    ///     觸發寫入日誌事件。
    /// </summary>
    /// <param name="logEntry"></param>
    protected virtual void OnLogWritten(ILogEntry logEntry)
    {
        var handler = LogWritten;
        LogWritten?.Invoke(this, logEntry);
    }

    /// <summary>
    ///     觸發寫入日誌失敗事件。
    /// </summary>
    /// <param name="logEntry"></param>
    /// <param name="exception"></param>
    protected virtual void OnLogWriteFailed(ILogEntry logEntry, Exception exception)
    {
        var handler = LogWriteFailed;
        handler?.Invoke(this, new LogWriteFailedEventArgs(logEntry, exception));
    }
}