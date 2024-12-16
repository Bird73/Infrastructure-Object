using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Birdsoft.Utilities.LogManager;

public class FileLogCleaner : LogCleanerBase
{
    public FileLogCleaner(ILogPathGenerator pathGenerator, ILogFileNameGenerator fileNameGenerator, uint retentionDays)
        : base(pathGenerator, fileNameGenerator, retentionDays)
    {
    }

    public override async Task Cleanup()
    {
        bool cleanupSuccess = true;

        DateTime now = DateTime.Now;
        
        // 取得所有要保留的路徑。Get all paths to retain.
        List<string> paths = new List<string>();
        for (int i = 0; i < _retentionDays; i++)
        {
            DateTime date = now.AddDays(-i);

            // add if not already in the list
            var path = _logPathGenerator.GetLogPath(date);
            if (!paths.Contains(path))
            {
                paths.Add(path);
            }
        }

        // 取得所有要保留的檔案。Get all files to retain.
        List<string> retentionFiles = new List<string>();
        for (int i = 0; i < _retentionDays; i++)
        {
            DateTime date = now.AddDays(-i);
            retentionFiles.Add(_logFileNameGenerator.GetLogFileName(date));
        }

        // 刪除不在保留清單中的檔案。Delete files not in the retention list.
        for (uint i = _retentionDays; i < 365 * 10; i++)
        {
            DateTime date = now.AddDays(-i);
            
            try
            {
                string path = _logPathGenerator.GetLogPath(date);
                if (paths.Contains(path))
                {
                    string file = _logFileNameGenerator.GetLogFileName(date);
                    string fullPath = Path.Combine(path, file);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                else
                {
                    // 如果目錄存在且不是根目錄，就刪除整個目錄包含裡面檔案。If the directory exists and is not the root directory, delete the entire directory including the files inside.
                    if (Directory.Exists(path) && !IsRootDirectory(path))
                    {
                        Directory.Delete(path, true);
                    }

                    bool IsRootDirectory(string path) 
                    {
                        var directoryInfo = new DirectoryInfo(path);
                        return directoryInfo.Parent == null; // 如果目錄沒有父目錄，則表示它是根目錄
                    }
                }
            }
            catch (Exception ex)
            {
                cleanupSuccess = false;
                OnCleanupFailed(ex);
            }
        }

        if (cleanupSuccess)
        {
            OnCleanupSuccess();
        }
    }
}