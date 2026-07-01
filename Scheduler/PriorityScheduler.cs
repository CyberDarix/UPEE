using System.Collections.Concurrent;
using UPEE.Core.Models;

namespace UPEE.Core.Scheduler;

/// <summary>
/// Scheduler de Priorité (CPU): Gestionnaire de files intelligent hiérarchisant les tâches
/// (Low/Medium/High/Critical) pour optimiser les ressources matérielles.
/// </summary>
public class PriorityScheduler : IDisposable
{
    private readonly ConcurrentDictionary<ExecutionPriority, ConcurrentQueue<ExecutionTask>> _queues;
    private readonly PriorityQueue<ExecutionTask, int> _priorityQueue;
    private readonly Timer _schedulerTimer;
    private int _activeTaskCount;
    private int _maxConcurrentTasks;
    private bool _disposed;

    public event EventHandler<SchedulerEventArgs>? TaskScheduled;
    public event EventHandler<SchedulerEventArgs>? TaskCompleted;

    public PriorityScheduler(int maxConcurrentTasks = 4)
    {
        _maxConcurrentTasks = maxConcurrentTasks;
        _activeTaskCount = 0;
        _queues = new ConcurrentDictionary<ExecutionPriority, ConcurrentQueue<ExecutionTask>>();
        _priorityQueue = new PriorityQueue<ExecutionTask, int>();

        foreach (ExecutionPriority priority in Enum.GetValues(typeof(ExecutionPriority)))
            _queues.TryAdd(priority, new ConcurrentQueue<ExecutionTask>());

        _schedulerTimer = new Timer(ProcessQueue, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    public void EnqueueTask(ExecutionTask task)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PriorityScheduler));
        if (task == null) throw new ArgumentNullException(nameof(task));

        task.Status = TaskStatus.Queued;
        task.EnqueuedAt = DateTime.UtcNow;

        if (_queues.TryGetValue((ExecutionPriority)task.Priority, out var queue))
        {
            queue.Enqueue(task);
            _priorityQueue.Enqueue(task, (int)(ExecutionPriority)task.Priority);
            TaskScheduled?.Invoke(this, new SchedulerEventArgs { Task = task });
        }
    }

    private void ProcessQueue(object? state)
    {
        if (_disposed) return;

        while (_activeTaskCount < _maxConcurrentTasks && _priorityQueue.Count > 0)
        {
            if (_priorityQueue.TryDequeue(out var task, out _))
            {
                Interlocked.Increment(ref _activeTaskCount);
                task.Status = TaskStatus.Running;
                task.StartedAt = DateTime.UtcNow;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ExecuteTaskAsync(task);
                        task.Status = TaskStatus.Completed;
                        task.CompletedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        task.Status = TaskStatus.Failed;
                        task.CompletedAt = DateTime.UtcNow;
                        task.Error = ex.Message;
                    }
                    finally
                    {
                        Interlocked.Decrement(ref _activeTaskCount);
                        TaskCompleted?.Invoke(this, new SchedulerEventArgs { Task = task });
                    }
                });
            }
        }
    }

    private async Task ExecuteTaskAsync(ExecutionTask task)
    {
        if (task.ExecutionDelegate != null)
            await task.ExecutionDelegate.Invoke();

        await Task.Delay(100);
    }

    public IEnumerable<ExecutionTask> GetQueuedTasks() =>
        _queues.Values.SelectMany(q => q).ToList();

    public int GetActiveTaskCount() => _activeTaskCount;

    public void Dispose()
    {
        if (_disposed) return;
        _schedulerTimer?.Dispose();
        _disposed = true;
    }
}

public class ExecutionTask
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; } = (int)ExecutionPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public Func<Task>? ExecutionDelegate { get; set; }
    public DateTime EnqueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Error { get; set; }
    public TimeSpan? Timeout { get; set; }
}

public enum TaskStatus
{
    Pending,
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

public class SchedulerEventArgs : EventArgs
{
    public ExecutionTask? Task { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
