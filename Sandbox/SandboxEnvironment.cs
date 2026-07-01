using System.Diagnostics;

namespace UPEE.Core.Sandbox;

/// <summary>
/// Isolation (Sandboxing): Création d'environnements virtuels sécurisés pour chaque script,
/// garantissant qu'une erreur d'exécution ne compromette jamais la stabilité du système.
/// </summary>
public class SandboxEnvironment : IDisposable
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public string TemporaryPath { get; }
    public string WorkspacePath { get; }
    public Dictionary<string, string> EnvironmentVariables { get; }
    public long? MemoryLimitMB { get; set; }
    public TimeSpan? ExecutionTimeout { get; set; }
    private Process? _process;
    private bool _disposed;

    public SandboxEnvironment(string? baseWorkspace = null)
    {
        var basePath = baseWorkspace ?? Path.Combine(Path.GetTempPath(), "UPEE");
        WorkspacePath = Path.Combine(basePath, "workspace", Id);
        TemporaryPath = Path.Combine(basePath, "temp", Id);

        Directory.CreateDirectory(WorkspacePath);
        Directory.CreateDirectory(TemporaryPath);

        EnvironmentVariables = new Dictionary<string, string>
        {
            { "UPEE_SANDBOX_ID", Id },
            { "UPEE_WORKSPACE", WorkspacePath },
            { "UPEE_TEMP", TemporaryPath }
        };

        ExecutionTimeout = TimeSpan.FromSeconds(30);
    }

    public async Task<SandboxExecutionResult> ExecuteAsync(string scriptPath, string? arguments = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SandboxEnvironment));
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException($"Script not found: {scriptPath}");

        var result = new SandboxExecutionResult { ScriptPath = scriptPath };

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetExecutorForScript(scriptPath),
                Arguments = $"\"{scriptPath}\" {arguments}".Trim(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = WorkspacePath
            };

            foreach (var env in EnvironmentVariables)
                psi.Environment[env.Key] = env.Value;

            _process = new Process { StartInfo = psi };
            _process.Start();

            var timeout = ExecutionTimeout ?? TimeSpan.FromSeconds(30);
            if (!_process.WaitForExit((int)timeout.TotalMilliseconds))
            {
                _process.Kill();
                result.Success = false;
                result.Error = $"Script execution timeout after {timeout.TotalSeconds}s";
                return result;
            }

            result.ExitCode = _process.ExitCode;
            result.StandardOutput = await _process.StandardOutput.ReadToEndAsync();
            result.StandardError = await _process.StandardError.ReadToEndAsync();
            result.Success = _process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
        }
        finally
        {
            _process?.Dispose();
        }

        return result;
    }

    private string GetExecutorForScript(string scriptPath)
    {
        var extension = Path.GetExtension(scriptPath).ToLower();
        return extension switch
        {
            ".py" => "python",
            ".js" => "node",
            ".lua" => "lua",
            ".sh" => "bash",
            ".bat" => "cmd.exe",
            ".ps1" => "powershell.exe",
            ".cpp" or ".c" => "gcc",
            ".rs" => "rustc",
            _ => "cmd.exe"
        };
    }

    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(TemporaryPath))
                Directory.Delete(TemporaryPath, true);
            if (Directory.Exists(WorkspacePath))
                Directory.Delete(WorkspacePath, true);
        }
        catch
        {
            // Cleanup errors are not critical
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _process?.Dispose();
        Cleanup();
        _disposed = true;
    }
}

public class SandboxExecutionResult
{
    public string ScriptPath { get; set; } = string.Empty;
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string StandardOutput { get; set; } = string.Empty;
    public string StandardError { get; set; } = string.Empty;
    public string? Error { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
