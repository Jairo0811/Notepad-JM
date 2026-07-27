using System.IO;

namespace NotepadJM.Services;

public static class AppLogger
{
    private static readonly object SyncRoot = new();

    public static void Log(Exception exception, string context)
    {
        try
        {
            var logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NotepadJM",
                "Logs");

            Directory.CreateDirectory(logDirectory);

            var logPath = Path.Combine(
                logDirectory,
                $"notepad-jm-{DateTime.Now:yyyy-MM-dd}.log");

            var entry = $"[{DateTime.Now:O}] {context}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";

            lock (SyncRoot)
            {
                File.AppendAllText(logPath, entry);
            }
        }
        catch
        {
            // Logging must never terminate the application.
        }
    }
}
