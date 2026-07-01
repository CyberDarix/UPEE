using System.Collections.Concurrent;

namespace UPEE.Core.Logging;

/// <summary>
/// Système de traçage complet avec notification d'erreurs en temps réel.
/// Format: [TIMESTAMP] [LEVEL] [COMPONENT] Message
/// </summary>
public class UniversalLogger : IDisposable
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly ConcurrentQueue<LogEntry> _logBuffer;
    private readonly Timer _flushTimer;
    private readonly object _fileLock = new();
    private bool _disposed;

    public event EventHandler<LogEntry>? LogEntryAdded;

    public UniversalLogger(string? logDirectory = null)
    {
        _logDirectory = logDirectory ?? Path.Combine(Path.GetTempPath(), "UPEE", "logs");
        Directory.CreateDirectory(_logDirectory);
        _logFilePath = Path.Combine(_logDirectory, "history.log");
        _logBuffer = new ConcurrentQueue<LogEntry>();
        _flushTimer = new Timer(_ => FlushBuffer(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public void Log(LogLevel level, string component, string message, Exception? exception = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalLogger));

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Component = component,
            Message = message,
            Exception = exception?.ToString()
        };

        _logBuffer.Enqueue(entry);
        LogEntryAdded?.Invoke(this, entry);

        if (level == LogLevel.Error || level == LogLevel.Critical)
            PrintToConsole(entry);
    }

    public void LogDebug(string component, string message) =>
        Log(LogLevel.Debug, component, message);

    public void LogInfo(string component, string message) =>
        Log(LogLevel.Info, component, message);

    public void LogWarning(string component, string message) =>
        Log(LogLevel.Warning, component, message);

    public void LogError(string component, string message, Exception? ex = null) =>
        Log(LogLevel.Error, component, message, ex);

    public void LogCritical(string component, string message, Exception? ex = null) =>
        Log(LogLevel.Critical, component, message, ex);

    private void PrintToConsole(LogEntry entry)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = entry.Level switch
        {
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Info => ConsoleColor.Green,
            _ => ConsoleColor.Gray
        };

        var logLine = FormatLogEntry(entry);
        Console.WriteLine(logLine);
        Console.ForegroundColor = originalColor;
    }

    private string FormatLogEntry(LogEntry entry) =>
        $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.Level,8}] [{entry.Component,20}] {entry.Message}";

    private void FlushBuffer()
    {
        if (_disposed || _logBuffer.IsEmpty) return;

        var entries = new List<LogEntry>();
        while (_logBuffer.TryDequeue(out var entry))
            entries.Add(entry);

        if (entries.Count == 0) return;

        lock (_fileLock)
        {
            try
            {
                using var writer = new StreamWriter(_logFilePath, true);
                foreach (var entry in entries)
                    writer.WriteLine(FormatLogEntry(entry));
            }
            catch
            {
                // Logging errors should not throw
            }
        }
    }

    public IEnumerable<LogEntry> GetRecentLogs(int count = 100)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalLogger));

        lock (_fileLock)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                    return Enumerable.Empty<LogEntry>();

                var lines = File.ReadLines(_logFilePath).TakeLast(count).ToList();
                return lines.Select(ParseLogLine).Where(l => l != null).Cast<LogEntry>();
            }
            catch
            {
                return Enumerable.Empty<LogEntry>();
            }
        }
    }

    private LogEntry? ParseLogLine(string line)
    {
        try
        {
            if (line.Length < 40) return null;

            var entry = new LogEntry { Message = line };
            return entry;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        FlushBuffer();
        _flushTimer?.Dispose();
        _disposed = true;
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Component { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }

    public override string ToString() =>
        $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Component}: {Message}";
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
