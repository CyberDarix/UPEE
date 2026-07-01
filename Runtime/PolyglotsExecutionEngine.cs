using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using UPEE.Core.Bus;
using UPEE.Core.Logging;
using UPEE.Core.Models;
using UPEE.Core.Sandbox;
using UPEE.Core.Scheduler;

namespace UPEE.Core.Runtime;

/// <summary>
/// Exécuteur Polyglotte: Orchestre l'exécution de scripts multi-langages
/// avec support complet des directives UPEE.
/// </summary>
public class PolyglotsExecutionEngine : IDisposable
{
    private readonly UniversalDataBus _dataBus;
    private readonly PriorityScheduler _scheduler;
    private readonly UniversalLogger _logger;
    private readonly string _watchDirectory;
    private bool _disposed;

    public PolyglotsExecutionEngine(string workingDirectory)
    {
        _dataBus = new UniversalDataBus();
        _scheduler = new PriorityScheduler(maxConcurrentTasks: 4);
        _logger = new UniversalLogger(Path.Combine(workingDirectory, "logs"));
        _watchDirectory = Path.Combine(workingDirectory, "scripts");

        Directory.CreateDirectory(_watchDirectory);
        _logger.LogInfo("Engine", "UPEE Engine initialized");
    }

    public async Task<ExecutionResult> ExecuteScriptAsync(string scriptPath)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PolyglotsExecutionEngine));
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script not found: {scriptPath}");

        _logger.LogInfo("Engine", $"Processing script: {Path.GetFileName(scriptPath)}");

        var result = new ExecutionResult { ScriptPath = scriptPath };

        try
        {
            var content = await File.ReadAllTextAsync(scriptPath);
            var lines = content.Split(Environment.NewLine);
            var header = DirectiveHeader.Parse(lines.FirstOrDefault() ?? string.Empty);

            if (header == null)
            {
                _logger.LogWarning("Engine", "No valid UPEE directive found, using defaults");
                header = new DirectiveHeader { Command = "RUN", Module = "DEFAULT" };
            }

            result.Directive = header;
            _logger.LogInfo("Engine", $"Directive parsed: {header}");

            var scriptContent = string.Join(Environment.NewLine, lines.Skip(1));
            var tempScript = Path.Combine(Path.GetTempPath(), $"upee_{Guid.NewGuid()}.tmp");
            await File.WriteAllTextAsync(tempScript, scriptContent);

            using var sandbox = new SandboxEnvironment(_watchDirectory);
            _logger.LogInfo("Engine", $"Sandbox created: {sandbox.Id}");

            var sandboxResult = await sandbox.ExecuteAsync(tempScript);
            result.SandboxResult = sandboxResult;

            if (sandboxResult.Success)
            {
                _logger.LogInfo("Engine", "Script executed successfully");
                _dataBus.SetValue($"script_{Path.GetFileNameWithoutExtension(scriptPath)}_output", sandboxResult.StandardOutput);
            }
            else
            {
                _logger.LogError("Engine", $"Script execution failed: {sandboxResult.Error}");
                result.Error = sandboxResult.Error;
            }

            result.Success = sandboxResult.Success;
            File.Delete(tempScript);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Engine", "Script execution error", ex);
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    public void ScheduleScript(string scriptPath, ExecutionPriority priority = ExecutionPriority.Medium)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PolyglotsExecutionEngine));

        var task = new ExecutionTask
        {
            Name = Path.GetFileName(scriptPath),
            Priority = (int)priority,
            ExecutionDelegate = async () => await ExecuteScriptAsync(scriptPath)
        };

        _scheduler.EnqueueTask(task);
        _logger.LogInfo("Engine", $"Script scheduled: {task.Name} with priority {priority}");
    }

    public async Task WatchDirectoryAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInfo("Engine", $"Starting directory watch: {_watchDirectory}");

        using var watcher = new FileSystemWatcher(_watchDirectory)
        {
            Filter = "*.*",
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        var queue = new Channel<string>();

        watcher.Created += (s, e) => queue.Writer.TryWrite(e.FullPath);
        watcher.Changed += (s, e) => queue.Writer.TryWrite(e.FullPath);
        watcher.EnableRaisingEvents = true;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var filePath in queue.Reader.ReadAllAsync(cancellationToken))
                {
                    if (IsValidScript(filePath))
                        ScheduleScript(filePath);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError("Engine", "Directory watch error", ex);
            }
        }

        watcher.Dispose();
    }

    private bool IsValidScript(string filePath) =>
        !string.IsNullOrEmpty(Path.GetExtension(filePath)) &&
        (Path.GetExtension(filePath) is ".py" or ".cpp" or ".rs" or ".js" or ".sh" or ".lua" or ".go");

    public UniversalDataBus GetDataBus() => _dataBus;

    public UniversalLogger GetLogger() => _logger;

    public void Dispose()
    {
        if (_disposed) return;
        _dataBus?.Dispose();
        _scheduler?.Dispose();
        _logger?.Dispose();
        _disposed = true;
    }
}

public class ExecutionResult
{
    public string ScriptPath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DirectiveHeader? Directive { get; set; }
    public SandboxExecutionResult? SandboxResult { get; set; }
    public string? Error { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}

// Channel helper for netstandard/older frameworks
public class Channel<T>
{
    private readonly ConcurrentQueue<T> _queue = new();
    public ChannelWriter<T> Writer { get; }
    public ChannelReader<T> Reader { get; }

    public Channel()
    {
        Writer = new ChannelWriter<T>(_queue);
        Reader = new ChannelReader<T>(_queue);
    }
}

public class ChannelWriter<T>
{
    private readonly ConcurrentQueue<T> _queue;
    public ChannelWriter(ConcurrentQueue<T> queue) => _queue = queue;
    public bool TryWrite(T item)
    {
        _queue.Enqueue(item);
        return true;
    }
}

public class ChannelReader<T>
{
    private readonly ConcurrentQueue<T> _queue;
    private readonly TaskCompletionSource<bool> _tcs = new();

    public ChannelReader(ConcurrentQueue<T> queue) => _queue = queue;

    public async IAsyncEnumerable<T> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var delay = new Random();
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var item))
                yield return item;
            else
                await Task.Delay(delay.Next(50, 200), cancellationToken);
        }
    }
}
