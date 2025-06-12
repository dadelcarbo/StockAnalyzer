namespace UltimateChartistSync;

public class Logger : IDisposable
{
    #region Singletion

    private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
    private readonly string _logFilePath;


    // Public static property to access the singleton instance
    public static Logger Instance => _instance.Value;

    private readonly StreamWriter _writer;



    #endregion

    private Logger()
    {
        var logFolder = Path.Combine(Directory.GetCurrentDirectory(), "Log");

        _logFilePath = Path.Combine(logFolder, $"{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.log");

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(logFolder);
        CleanupOldLogs(logFolder);

        _writer = new StreamWriter(_logFilePath) { AutoFlush = true };
        _writer.Flush();
    }


    private readonly object _lock = new object();


    public void WriteLine(string message)
    {
        string timestampedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        lock (_lock)
        {
            _writer.WriteLine(timestampedMessage);
        }
    }

    public void CleanupOldLogs(string logDirectory, int days = 7)
    {
        try
        {
            if (Directory.Exists(logDirectory))
            {
                var files = Directory.GetFiles(logDirectory);
                foreach (var file in files)
                {
                    var lastWrite = File.GetLastWriteTime(file);
                    if ((DateTime.Now - lastWrite).TotalDays > days)
                    {
                        File.Delete(file);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }
}

