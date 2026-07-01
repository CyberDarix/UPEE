using Xunit;
using UPEE.Core.Bus;
using UPEE.Core.Logging;
using UPEE.Core.Models;
using UPEE.Core.Scheduler;
using TaskStatus = UPEE.Core.Scheduler.TaskStatus;

namespace UPEE.Tests;

public class DirectiveHeaderTests
{
    [Fact]
    public void Parse_ValidHeader_ReturnsDirective()
    {
        var header = "// UPEE_RUN_CPP";
        var result = DirectiveHeader.Parse(header);

        Assert.NotNull(result);
        Assert.Equal("RUN", result.Command);
        Assert.Equal("CPP", result.Module);
    }

    [Fact]
    public void Parse_InvalidHeader_ReturnsNull()
    {
        var header = "// INVALID_HEADER";
        var result = DirectiveHeader.Parse(header);

        Assert.Null(result);
    }

    [Fact]
    public void Parse_WithPriority_SetsPriority()
    {
        var header = "// UPEE_RUN_PYTHON_PRIORITY=3";
        var result = DirectiveHeader.Parse(header);

        Assert.NotNull(result);
        Assert.Equal((int)ExecutionPriority.Critical, result.Priority);
    }
}

public class UniversalDataBusTests
{
    [Fact]
    public void SetValue_StoresValue()
    {
        using var bus = new UniversalDataBus();
        bus.SetValue("test_key", "test_value");
        var result = bus.GetValue("test_key");

        Assert.NotNull(result);
    }

    [Fact]
    public void GetValue_WithoutSet_ReturnsNull()
    {
        using var bus = new UniversalDataBus();
        var result = bus.GetValue("nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void RemoveValue_RemovesValue()
    {
        using var bus = new UniversalDataBus();
        bus.SetValue("test_key", "test_value");
        var removed = bus.RemoveValue("test_key");
        var result = bus.GetValue("test_key");

        Assert.True(removed);
        Assert.Null(result);
    }

    [Fact]
    public void Clear_RemovesAllData()
    {
        using var bus = new UniversalDataBus();
        bus.SetValue("key1", "value1");
        bus.SetValue("key2", "value2");
        bus.Clear();
        var data = bus.GetAllData();

        Assert.Empty(data);
    }

    [Fact]
    public void DataChanged_RaisesEvent()
    {
        using var bus = new UniversalDataBus();
        var eventRaised = false;

        bus.DataChanged += (s, e) =>
        {
            if (e.Action == "Set")
                eventRaised = true;
        };

        bus.SetValue("test", "value");

        Assert.True(eventRaised);
    }
}

public class PrioritySchedulerTests
{
    [Fact]
    public void EnqueueTask_AddsToQueue()
    {
        using var scheduler = new PriorityScheduler();
        var task = new ExecutionTask { Name = "Test Task" };
        scheduler.EnqueueTask(task);

        Assert.Equal(TaskStatus.Queued, task.Status);
    }

    [Fact]
    public void GetQueuedTasks_ReturnsEnqueuedTasks()
    {
        using var scheduler = new PriorityScheduler();
        var task1 = new ExecutionTask { Name = "Task 1" };
        var task2 = new ExecutionTask { Name = "Task 2" };

        scheduler.EnqueueTask(task1);
        scheduler.EnqueueTask(task2);

        var queued = scheduler.GetQueuedTasks().ToList();
        Assert.NotEmpty(queued);
    }

    [Fact]
    public async Task EnqueueTask_WithDelegate_Executes()
    {
        using var scheduler = new PriorityScheduler();
        var executed = false;

        var task = new ExecutionTask
        {
            Name = "Test",
            ExecutionDelegate = async () =>
            {
                executed = true;
                await Task.Delay(10);
            }
        };

        scheduler.EnqueueTask(task);
        await Task.Delay(500);

        Assert.True(executed);
    }
}

public class UniversalLoggerTests
{
    [Fact]
    public async Task Log_CreatesLogEntry()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        using var logger = new UniversalLogger(tempDir);

        logger.LogInfo("TestComponent", "Test message");
        await Task.Delay(1500); // Wait for flush

        var logs = logger.GetRecentLogs(1);
        Assert.NotEmpty(logs);
    }

    [Fact]
    public void LogEntryAdded_RaisesEvent()
    {
        using var logger = new UniversalLogger();
        var eventRaised = false;

        logger.LogEntryAdded += (s, e) =>
        {
            if (e.Component == "Test")
                eventRaised = true;
        };

        logger.LogInfo("Test", "Message");

        Assert.True(eventRaised);
    }

    [Fact]
    public void LogError_PrintsToConsole()
    {
        using var logger = new UniversalLogger();

        Assert.Throws<ObjectDisposedException>(() =>
        {
            logger.Dispose();
            logger.LogError("Test", "Message");
        });
    }
}
