using NotepadJM.Models;
using System.IO;
using System.Text.Json;

namespace NotepadJM.Services;

public sealed class AppDataService : IAppDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsPath;
    private readonly string _recoveryDirectory;

    public AppDataService()
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NotepadJM");

        _settingsPath = Path.Combine(appDataDirectory, "settings.json");
        _recoveryDirectory = Path.Combine(appDataDirectory, "Recovery");

        Directory.CreateDirectory(appDataDirectory);
        Directory.CreateDirectory(_recoveryDirectory);
    }

    public AppSettings LoadSettings()
    {
        try
        {
            return File.Exists(_settingsPath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new AppSettings()
                : new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
        catch (IOException)
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch (IOException)
        {
            // Settings are non-critical and will be retried on the next change.
        }
        catch (UnauthorizedAccessException)
        {
            // The editor must remain usable even when local settings cannot be persisted.
        }
    }

    public IReadOnlyList<RecoveredDocument> LoadRecoveredDocuments()
    {
        var recoveredDocuments = new List<RecoveredDocument>();

        foreach (var recoveryFilePath in Directory.GetFiles(_recoveryDirectory, "*.json"))
        {
            try
            {
                var recovery = JsonSerializer.Deserialize<RecoveryDocument>(
                    File.ReadAllText(recoveryFilePath));

                if (recovery is not null)
                {
                    recoveredDocuments.Add(
                        new RecoveredDocument(
                            recoveryFilePath,
                            recovery.FilePath,
                            recovery.Content));
                }
            }
            catch (JsonException)
            {
                DeleteRecoveryFile(recoveryFilePath);
            }
            catch (IOException)
            {
                // Keep the file so it can be retried in a later session.
            }
        }

        return recoveredDocuments;
    }

    public void SaveRecovery(DocumentTab document)
    {
        try
        {
            var recovery = new RecoveryDocument(document.FilePath, document.Content);
            File.WriteAllText(
                GetRecoveryPath(document),
                JsonSerializer.Serialize(recovery));
        }
        catch (IOException)
        {
            // Recovery is best-effort and must not interrupt editing.
        }
        catch (UnauthorizedAccessException)
        {
            // Recovery is best-effort and must not interrupt editing.
        }
    }

    public void DeleteRecovery(DocumentTab document) =>
        DeleteRecoveryFile(GetRecoveryPath(document));

    public void DeleteRecoveryFile(string recoveryFilePath)
    {
        try
        {
            if (File.Exists(recoveryFilePath))
            {
                File.Delete(recoveryFilePath);
            }
        }
        catch (IOException)
        {
            // A stale recovery file can be cleaned up on the next launch.
        }
        catch (UnauthorizedAccessException)
        {
            // A stale recovery file can be cleaned up on the next launch.
        }
    }

    private string GetRecoveryPath(DocumentTab document) =>
        Path.Combine(_recoveryDirectory, $"{document.Id:N}.json");
}
