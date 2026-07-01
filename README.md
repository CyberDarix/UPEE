# UPEE - Universal Polyglot Execution Engine

## Overview

UPEE is a universal, decentralized execution engine designed to provide total control over a system's hardware resources. Acting as an "Intelligent System Runtime," it allows users to define their own automation, computation, or optimization processes using polyglot scripts.

## Architecture

### Core Modules

#### 1. **UPEE.Core** - Engine Foundations
- **DirectiveHeader**: Parsing of standardized headers (`// UPEE_RUN_CPP`)
- **UniversalDataBus**: JSON shared memory bus for inter-process interoperability
- **PriorityScheduler**: Queue manager with prioritization (Low/Medium/High/Critical)
- **SandboxEnvironment**: Secure environments for script execution
- **UniversalLogger**: Comprehensive tracing system with real-time notifications
- **PolyglotsExecutionEngine**: Main orchestrator for multi-language execution

#### 2. **UPEE.CLI** - Command-Line Interface
```bash
upee connect <path>              # Initialize a workspace
upee execute -s <script>         # Execute a specific script
upee execute -w                  # Monitor for changes
upee status                      # Display service status
```

#### 3. **UPEE.Service** - Windows Service
Registration as a persistent system service with automatic monitoring.

#### 4. **UPEE.Runtime** - Execution Manager
Centralized manager for UPEE components with an API for programmatic use.

#### 5. **UPEE.Tests** - Test Suite
14 unit tests validating all critical components. ## Installation

### Prerequisites
- .NET 10.0
- Windows 10+
- PowerShell 5.0+

### Build
```bash
dotnet build UPEE.sln --configuration Release
```

### Service Installation
```bash
dotnet UPEE.Service\bin\Release\net10.0\UPEE.Service.dll install
```

## Usage

### Initialize a Workspace
```bash
upee connect "C:\My\Workspace"
```

This creates the following structure:
```
C:\My\Workspace\UPEE\
├── scripts/         # Your polyglot scripts
├── libs/            # Shared libraries
├── logs/            # Execution history
├── bin/             # Compiled binaries
└── core/            # System configuration
```

### Create an Executable Script

**exemple.py** with UPEE directive:
```python
// UPEE_RUN_PYTHON_PRIORITY=2

def main():
print("Hello from UPEE!")

if __name__ == "__main__":
main()
```

### Execute a Script
```bash
upee execute -s exemple.py
```

### Watch Mode
```bash
upee execute -w
# Watches the scripts/ folder and automatically executes new files
```

## Directive Format

```
// UPEE_<COMMAND>_<MODULE>_<OPTIONS>
```

### Examples
- `// UPEE_RUN_CPP` - Run C++ code
- `// UPEE_COMPILE_RUST_PRIORITY=3` - Compile Rust with Critical priority
- `// UPEE_EXEC_PYTHON` - Execute Python
- `// UPEE_RUN_GO_TIMEOUT=30` - Run Go with a 30s timeout

### Supported Modules
- `CPP` - C/C++
- `RUST` - Rust
- `PYTHON` - Python
- `GO` - Go
- `JS` - JavaScript/Node.js
- - `LUA` - Lua
- `SHELL` - Bash/Sh
- `DEFAULT` - Default executor

### Commands
- `RUN` - Execute directly
- `COMPILE` - Compile then execute
- `EXEC` - Execute a compiled binary

### Priorities
- `0` or `Low` - Background tasks
- `1` or `Medium` - Standard tasks (default)
- `2` or `High` - Priority tasks
- `3` or `Critical` - Critical system tasks

## Logging

Logs are stored in `/logs/history.log` using the following format:
```
[2024-07-01 14:30:45.123] [ERROR   ] [Engine               ] Error description
```

## Programmatic API

```csharp
using UPEE.Runtime;

var manager = new UPEERuntimeManager("C:\\UPEE\\Workspace");

// Execute a script
var result = await manager.ExecuteScriptAsync("script.py");
Console.WriteLine(result.Success ? "OK" : $"Error: {result.Error}");

// Global variables
manager.SetGlobalVariable("myVar", "value");
var value = manager.GetGlobalVariable<string>("myVar");

// Schedule
manager.ScheduleScript("script.py", ExecutionPriority.High);
``` ```

## Sandbox System

Each script runs in an isolated environment:
- Limited memory (configurable)
- Execution timeout (default: 30s)
- File access limited to the workspace
- Full process isolation

## Performance

- **Scheduler**: Manages up to 4 concurrent tasks
- **Shared memory**: JSON with lock-free reads
- **Asynchronous logging**: 1-second buffer before writing
- **Sandbox**: Process isolation with automatic cleanup

## Build Status

✅ **BUILD SUCCESS** - All projects compiled successfully
- UPEE.Core: ✓
- UPEE.CLI: ✓
- UPEE.Service: ✓
- UPEE.Runtime: ✓
- UPEE.Tests: ✓ (14/14 tests passed)

## Current Limitations

- No distributed multi-node support (planned for v2.0)
- No database persistence (use JSON files)
- Sandbox limited to local process (no containers)

## Roadmap

- v1.1: Docker container support
