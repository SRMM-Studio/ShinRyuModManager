using Serilog.Core;
using Serilog.Events;

namespace ShinRyuModManager.Sinks;

public sealed class InMemoryLogSink : ILogEventSink
{
    public static InMemoryLogSink Instance { get; } = new();
    
    // Don't leak memory
    private const int CAPACITY = 5000;
    
    private readonly Lock _lock = new();
    private readonly Queue<LogLine> _buffer = [];
    
    private event Action<LogLine> LogAdded; 
    
    public void Emit(LogEvent logEvent)
    {
        var text = logEvent.RenderMessage();
        
        if (logEvent.Exception is not null)
        {
            text += Environment.NewLine + logEvent.Exception;
        }
        
        var line = new LogLine(logEvent.Timestamp, logEvent.Level, text);
        
        Action<LogLine> handlers;
        
        lock (_lock)
        {
            _buffer.Enqueue(line);
            
            if (_buffer.Count > CAPACITY)
            {
                _buffer.Dequeue();
            }
            
            handlers = LogAdded;
        }
        
        handlers?.Invoke(line);
    }
    
    public LogLine[] Subscribe(Action<LogLine> handler)
    {
        lock (_lock)
        {
            LogAdded += handler;
            
            return _buffer.ToArray();
        }
    }
    
    public void Unsubscribe(Action<LogLine> handler)
    {
        lock (_lock)
        {
            LogAdded -= handler;
        }
    }
}

public sealed record LogLine(DateTimeOffset Timestamp, LogEventLevel Level, string Text);