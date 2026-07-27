namespace NotepadJM.Services;

public interface IFileDialogService
{
    IReadOnlyList<string> ShowOpenFiles();
    string? ShowSaveFile(string suggestedFileName);
}
