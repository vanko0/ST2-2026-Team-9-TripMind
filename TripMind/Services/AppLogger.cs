namespace TripMind.Services;

// Singleton Pattern: guarantees exactly ONE AppLogger exists in the whole app.
// The constructor is private, so nobody can write "new AppLogger()".
// The only way to get it is AppLogger.Instance.
// Lazy<T> creates it the first time it's needed and is thread-safe.
public sealed class AppLogger
{
    private static readonly Lazy<AppLogger> _instance = new(() => new AppLogger());
    public static AppLogger Instance => _instance.Value;

    private readonly List<string> _entries = new();
    private readonly object _lock = new();

    private AppLogger() { }

    public void Log(string message)
    {
        lock (_lock)
        {
            _entries.Add($"{DateTime.Now:HH:mm:ss} {message}");
        }
    }

    public List<string> GetEntries()
    {
        lock (_lock)
        {
            return _entries.ToList();
        }
    }
}
