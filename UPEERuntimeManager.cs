using UPEE.Core.Bus;
using UPEE.Core.Logging;
using UPEE.Core.Models;
using UPEE.Core.Runtime;
using UPEE.Core.Sandbox;
using UPEE.Core.Scheduler;

namespace UPEE.Runtime;

/// <summary>
/// Runtime Manager: Gestionnaire centralisé de tous les composants UPEE
/// </summary>
public class UPEERuntimeManager
{
    private readonly PolyglotsExecutionEngine _engine;
    private readonly UniversalDataBus _dataBus;
    private readonly UniversalLogger _logger;
    private readonly string _workspaceRoot;

    public UPEERuntimeManager(string workspaceRoot)
    {
        _workspaceRoot = workspaceRoot;
        Directory.CreateDirectory(workspaceRoot);

        _engine = new PolyglotsExecutionEngine(workspaceRoot);
        _dataBus = _engine.GetDataBus();
        _logger = _engine.GetLogger();
    }

    public async Task<ExecutionReport> ExecuteScriptAsync(string scriptPath)
    {
        var report = new ExecutionReport { ScriptPath = scriptPath };

        try
        {
            var result = await _engine.ExecuteScriptAsync(scriptPath);
            report.Success = result.Success;
            report.ExitCode = result.SandboxResult?.ExitCode ?? 1;
            report.Output = result.SandboxResult?.StandardOutput;
            report.Error = result.SandboxResult?.StandardError ?? result.Error;
            report.Directive = result.Directive;
        }
        catch (Exception ex)
        {
            _logger.LogCritical("RuntimeManager", "Execution error", ex);
            report.Success = false;
            report.Error = ex.Message;
            report.ExitCode = -1;
        }

        return report;
    }

    public void SetGlobalVariable(string key, object? value)
    {
        _dataBus.SetValue(key, value);
        _logger.LogDebug("RuntimeManager", $"Global variable set: {key}");
    }

    public object? GetGlobalVariable(string key) => _dataBus.GetValue(key);

    public T? GetGlobalVariable<T>(string key) => _dataBus.GetValue<T>(key);

    public void ScheduleScript(string scriptPath, ExecutionPriority priority = ExecutionPriority.Medium)
    {
        _engine.ScheduleScript(scriptPath, priority);
    }

    public async Task WatchDirectoryAsync(CancellationToken cancellationToken = default)
    {
        await _engine.WatchDirectoryAsync(cancellationToken);
    }

    public IEnumerable<LogEntry> GetRecentLogs(int count = 100) => _logger.GetRecentLogs(count);

    public void Dispose()
    {
        _engine?.Dispose();
        _dataBus?.Dispose();
        _logger?.Dispose();
    }
}

public class ExecutionReport
{
    public string ScriptPath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
    public DirectiveHeader? Directive { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
