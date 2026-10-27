using Core.Abstractions;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message) =>
        Console.WriteLine($"    [ConsoleSink -> DI] {message}");
}

public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = path ?? throw new ArgumentNullException(nameof(path));

    public void Notify(string message)
    {
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(_path, $"[FileSink -> DI] {DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}");
    }
}
