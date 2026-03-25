namespace OneView.VirtualScanner.Core.Services;

public class ScanLogger
{
    private readonly string _logFile;
    private readonly object _lock = new();

    public ScanLogger(string? logFile = null)
    {
        SharedPaths.EnsureDirectories();
        _logFile = logFile ?? Path.Combine(SharedPaths.LogDir,
            $"scan_{DateTime.Now:yyyyMMdd}.log");
    }

    public void Log(string level, string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}";
        lock (_lock)
        {
            File.AppendAllText(_logFile, line + Environment.NewLine);
        }
    }

    public void Info(string msg) => Log("INFO", msg);
    public void Warn(string msg) => Log("WARN", msg);
    public void Error(string msg) => Log("ERROR", msg);

    public string[] GetRecent(int lines = 200)
    {
        if (!File.Exists(_logFile)) return Array.Empty<string>();
        var all = File.ReadAllLines(_logFile);
        return all.Length <= lines ? all : all[^lines..];
    }

    public string LogFile => _logFile;
    public string LogDir => SharedPaths.LogDir;
}
