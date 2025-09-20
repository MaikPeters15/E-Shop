using Serilog.Core;
using Serilog.Events;

namespace Api.Logging;

public class InMemorySink : ILogEventSink
{
    private readonly InMemoryLogStore _store;

    public InMemorySink(InMemoryLogStore store)
    {
        _store = store;
    }

    public void Emit(LogEvent logEvent)
    {
        var source = logEvent.Properties.TryGetValue("SourceContext", out var v) ? v.ToString() : null;
        var exception = logEvent.Exception?.ToString();
        var message = logEvent.RenderMessage();
        var level = logEvent.Level.ToString();
        _store.Add(new LogEntry(logEvent.Timestamp, level, source, message, exception));
    }
}
