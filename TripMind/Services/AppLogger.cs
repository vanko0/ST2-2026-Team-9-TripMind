namespace TripMind.Services;

// One log entry: when it happened, what happened, and the details.
public class LogEntry
{
    public DateTime Time { get; set; }
    public string Action { get; set; } = "";
    public string Description { get; set; } = "";
}

// Singleton Pattern: guarantees exactly ONE AppLogger exists in the whole app.
// The constructor is private, so nobody can write "new AppLogger()".
// The only way to get it is AppLogger.Instance.
// Lazy<T> creates it the first time it's needed and is thread-safe.
public sealed class AppLogger
{
    private static readonly Lazy<AppLogger> _instance = new(() => new AppLogger());
    public static AppLogger Instance => _instance.Value;

    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    private AppLogger() { }

    public void Log(string action, string description = "")
    {
        lock (_lock)
        {
            _entries.Add(new LogEntry
            {
                Time = DateTime.Now,
                Action = action,
                Description = description
            });
        }
    }

    public List<LogEntry> GetEntries()
    {
        lock (_lock)
        {
            return _entries.ToList();
        }
    }
}