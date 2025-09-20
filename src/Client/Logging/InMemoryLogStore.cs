namespace Client.Logging;

public record LogEntry(DateTimeOffset Timestamp, string Level, string? Source, string Message, string? Exception);

public class InMemoryLogStore(int capacity = 1000)
{
    private readonly LinkedList<LogEntry> _logs = new();
    private readonly object _lock = new();

    public event Action? Changed;

    public void Add(LogEntry entry)
    {
        lock (_lock)
        {
            _logs.AddLast(entry);
            if (_logs.Count > capacity)
            {
                _logs.RemoveFirst();
            }
        }
        Changed?.Invoke();
    }

    public IReadOnlyList<LogEntry> GetAll()
    {
        lock (_lock)
        {
            return _logs.ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _logs.Clear();
        }
        Changed?.Invoke();
    }
}
