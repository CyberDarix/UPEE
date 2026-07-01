using System.ServiceProcess;
using System.Diagnostics;
using UPEE.Core.Runtime;

namespace UPEE.Service;

public class UPEEWindowsService : ServiceBase
{
    private PolyglotsExecutionEngine? _engine;
    private CancellationTokenSource? _cancellationTokenSource;

    public UPEEWindowsService()
    {
        ServiceName = "UPEE_Engine";
        CanStop = true;
        CanPauseAndContinue = false;
        CanHandleSessionChangeEvent = true;
        AutoLog = true;
    }

    protected override void OnStart(string[] args)
    {
        try
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var workspaceRoot = Path.Combine(appDataPath, "UPEE");
            Directory.CreateDirectory(workspaceRoot);

            _engine = new PolyglotsExecutionEngine(workspaceRoot);
            _cancellationTokenSource = new CancellationTokenSource();

            _ = _engine.WatchDirectoryAsync(_cancellationTokenSource.Token);

            EventLog.WriteEntry("UPEE_Engine", "UPEE Service started successfully", EventLogEntryType.Information);
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry("UPEE_Engine", $"Error starting service: {ex.Message}", EventLogEntryType.Error);
            throw;
        }
    }

    protected override void OnStop()
    {
        try
        {
            _cancellationTokenSource?.Cancel();
            _engine?.Dispose();
            EventLog.WriteEntry("UPEE_Engine", "UPEE Service stopped successfully", EventLogEntryType.Information);
        }
        catch (Exception ex)
        {
            EventLog.WriteEntry("UPEE_Engine", $"Error stopping service: {ex.Message}", EventLogEntryType.Error);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("UPEE Service requires Windows");

        ServiceBase.Run(new UPEEWindowsService());
    }
}
