namespace NotepadJM.Services;

public interface IUserDialogService
{
    SaveChangesChoice ConfirmSaveChanges(string documentName);
    bool ConfirmRecovery();
    void ShowError(string message);
    void ShowInformation(string message, string title = "Notepad JM");
}

public enum SaveChangesChoice
{
    Save,
    Discard,
    Cancel
}
