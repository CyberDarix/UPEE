namespace UPEE.Core.Models;

/// <summary>
/// Représente la directive d'en-tête standardisée pour identifier la nature d'un script.
/// Format: // UPEE_[COMMAND]_[MODULE]
/// Exemples: UPEE_RUN_CPP, UPEE_COMPILE_RUST, UPEE_EXEC_PYTHON
/// </summary>
public class DirectiveHeader
{
    public string Command { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int Priority { get; set; } = (int)ExecutionPriority.Medium;
    public string? Timeout { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();

    public static DirectiveHeader? Parse(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine))
            return null;

        var trimmed = headerLine.Trim();
        if (!trimmed.StartsWith("//"))
            return null;

        var content = trimmed[2..].Trim();
        if (!content.StartsWith("UPEE_"))
            return null;

        var parts = content[5..].Split('_');
        if (parts.Length < 2)
            return null;

        var directive = new DirectiveHeader
        {
            Command = parts[0],
            Module = parts[1]
        };

        for (int i = 2; i < parts.Length; i++)
        {
            if (parts[i].StartsWith("PRIORITY="))
                if (Enum.TryParse<ExecutionPriority>(parts[i][9..], out var priority))
                    directive.Priority = (int)priority;

            if (parts[i].StartsWith("TIMEOUT="))
                directive.Timeout = parts[i][8..];
        }

        return directive;
    }

    public override string ToString() => $"UPEE_{Command}_{Module} (Priority: {(ExecutionPriority)Priority})";
}

public enum ExecutionPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
