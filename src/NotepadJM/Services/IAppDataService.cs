using NotepadJM.Models;

namespace NotepadJM.Services;

public interface IAppDataService
{
    AppSettings LoadSettings();
    void SaveSettings(AppSettings settings);
    IReadOnlyList<RecoveredDocument> LoadRecoveredDocuments();
    void SaveRecovery(DocumentTab document);
    void DeleteRecovery(DocumentTab document);
    void DeleteRecoveryFile(string recoveryFilePath);
}
