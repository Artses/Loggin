using System.Text.Json;
using System.IO;

namespace Loggin.Desktop.Services;

public static class LocalLog
{
    private static readonly object Sync = new();
    private static readonly string DirectoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Artses", "Loggin", "logs");

    public static void Info(string message) => Write("Information", message, null);
    public static void Error(string message, Exception? exception = null) => Write("Error", message, exception);

    private static void Write(string level, string message, Exception? exception)
    {
        try
        {
            Directory.CreateDirectory(DirectoryPath);
            var entry = JsonSerializer.Serialize(new
            {
                timestamp = DateTimeOffset.Now,
                level,
                message,
                exception = exception?.ToString()
            });
            lock (Sync)
                File.AppendAllText(Path.Combine(DirectoryPath, $"loggin-{DateTime.Now:yyyyMMdd}.jsonl"), entry + Environment.NewLine);
        }
        catch
        {
            // Logging must never terminate the desktop client.
        }
    }
}
