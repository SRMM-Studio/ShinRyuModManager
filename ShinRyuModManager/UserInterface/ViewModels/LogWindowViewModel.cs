using System.Collections.Concurrent;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Threading;
using Serilog.Events;
using ShinRyuModManager.Sinks;

namespace ShinRyuModManager.UserInterface.ViewModels;

public sealed class LogWindowViewModel : ViewModelBase, IDisposable
{
    public AvaloniaList<LogLineItem> Lines { get; } = [];
        
    public event Action LinesFlushed;
    
    private const int MAX_LINES = 5000;
    
    private readonly ConcurrentQueue<LogLine> _pending = [];
    private readonly DispatcherTimer _flushTimer;
    
    public LogWindowViewModel()
    {
        Lines.AddRange(InMemoryLogSink.Instance.Subscribe(OnLine).Select(ToItem));
        
        
        _flushTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Background, (_, _) => Flush());
        _flushTimer.Start();
    }
    
    public string GetAllText()
    {
        return string.Join(Environment.NewLine, Lines.Select(x => x.Text));
    }
    
    private void OnLine(LogLine line)
    {
        _pending.Enqueue(line);
    }
    
    private void Flush()
    {
        if (_pending.IsEmpty)
            return;
        
        var batch = new List<LogLine>();
        
        while (_pending.TryDequeue(out var line))
        {
            batch.Add(line);
        }
        
        while (Lines.Count > MAX_LINES)
        {
            batch.RemoveRange(0, batch.Count - MAX_LINES);
        }
        
        var overflow = Lines.Count + batch.Count - MAX_LINES;
        
        if (overflow > 0)
        {
            Lines.RemoveRange(0, overflow);
        }
        
        Lines.AddRange(batch.Select(ToItem));
        
        LinesFlushed?.Invoke();
    }
    
    private static LogLineItem ToItem(LogLine line)
    {
        var prefix = line.Level switch
        {
            LogEventLevel.Verbose => "VRB",
            LogEventLevel.Debug => "DBG",
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Fatal => "FTL",
            _ => "???" // Unknown level
        };
        
        var brush = line.Level switch
        {
            LogEventLevel.Verbose or LogEventLevel.Debug => Brushes.Gray,
            LogEventLevel.Warning => Brushes.Gold,
            LogEventLevel.Error or LogEventLevel.Fatal => Brushes.IndianRed,
            _ => Brushes.Gainsboro
        };
        
        return new LogLineItem($"[{line.Timestamp:HH:mm:ss} {prefix}] {line.Text}", brush);
    }
    
    public void Dispose()
    {
        _flushTimer.Stop();
        
        InMemoryLogSink.Instance.Unsubscribe(OnLine);
    }
}

public sealed class LogLineItem
{
    public LogLineItem(string text, IBrush foreground)
    {
        Text = text;
        Foreground = foreground;
    }
    
    public string Text { get; }
    public IBrush Foreground { get; }
}