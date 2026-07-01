using System.Collections.Concurrent;
using System.Text.Json;

namespace UPEE.Core.Bus;

/// <summary>
/// Bus de Données Universel: Système de mémoire partagée permettant aux scripts
/// de communiquer et d'échanger des variables en temps réel via JSON.
/// </summary>
public class UniversalDataBus : IDisposable
{
    private readonly ConcurrentDictionary<string, JsonElement> _dataStore;
    private readonly ReaderWriterLockSlim _lock;
    private bool _disposed;

    public event EventHandler<DataBusEventArgs>? DataChanged;

    public UniversalDataBus()
    {
        _dataStore = new ConcurrentDictionary<string, JsonElement>();
        _lock = new ReaderWriterLockSlim();
    }

    public void SetValue(string key, object? value)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or empty", nameof(key));

        _lock.EnterWriteLock();
        try
        {
            var json = JsonSerializer.SerializeToElement(value);
            _dataStore[key] = json;
            DataChanged?.Invoke(this, new DataBusEventArgs { Key = key, Value = value, Action = "Set" });
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public object? GetValue(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or empty", nameof(key));

        _lock.EnterReadLock();
        try
        {
            return _dataStore.TryGetValue(key, out var value) ? value.GetRawText() : null;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public T? GetValue<T>(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));

        _lock.EnterReadLock();
        try
        {
            if (_dataStore.TryGetValue(key, out var value))
                return JsonSerializer.Deserialize<T>(value.GetRawText());
            return default;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public bool RemoveValue(string key)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));

        _lock.EnterWriteLock();
        try
        {
            var removed = _dataStore.TryRemove(key, out _);
            if (removed)
                DataChanged?.Invoke(this, new DataBusEventArgs { Key = key, Action = "Remove" });
            return removed;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public Dictionary<string, string> GetAllData()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));

        _lock.EnterReadLock();
        try
        {
            return _dataStore.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetRawText());
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public void Clear()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(UniversalDataBus));

        _lock.EnterWriteLock();
        try
        {
            _dataStore.Clear();
            DataChanged?.Invoke(this, new DataBusEventArgs { Action = "Clear" });
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _lock?.Dispose();
        _disposed = true;
    }
}

public class DataBusEventArgs : EventArgs
{
    public string? Key { get; set; }
    public object? Value { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
