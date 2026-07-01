using UPEE.Core.Runtime;

var cmdArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

if (cmdArgs.Length == 0)
{
    Console.WriteLine("UPEE - Universal Polyglot Execution Engine");
    Console.WriteLine("Commands: connect, execute, status");
    return;
}

try
{
    switch (cmdArgs[0].ToLower())
    {
        case "connect":
            if (cmdArgs.Length < 2) 
            {
                Console.WriteLine("Usage: upee connect <path>");
                return;
            }
            var path = cmdArgs[1];
            var root = Path.Combine(path, "UPEE");
            foreach (var dir in new[] { "scripts", "libs", "logs", "bin", "core" })
                Directory.CreateDirectory(Path.Combine(root, dir));
            Console.WriteLine($"UPEE initialized at: {root}");
            break;

        case "execute":
            Console.WriteLine("Execute command");
            break;

        case "status":
            Console.WriteLine("UPEE Status: Active");
            break;

        default:
            Console.WriteLine($"Unknown command: {cmdArgs[0]}");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
