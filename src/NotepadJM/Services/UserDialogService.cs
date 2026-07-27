using System.Windows;

namespace NotepadJM.Services;

public sealed class UserDialogService : IUserDialogService
{
    public SaveChangesChoice ConfirmSaveChanges(string documentName)
    {
        var result = ShowMessage(
            $"¿Deseas guardar los cambios en «{documentName}»?",
            "Notepad JM",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        return result switch
        {
            MessageBoxResult.Yes => SaveChangesChoice.Save,
            MessageBoxResult.No => SaveChangesChoice.Discard,
            _ => SaveChangesChoice.Cancel
        };
    }

    public bool ConfirmRecovery() =>
        ShowMessage(
            "Notepad JM encontró documentos sin guardar de una sesión anterior. ¿Deseas recuperarlos?",
            "Recuperación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question) == MessageBoxResult.Yes;

    public void ShowError(string message) =>
        ShowMessage(
            message,
            "Notepad JM",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

    public void ShowInformation(string message, string title = "Notepad JM") =>
        ShowMessage(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Information);

    private static MessageBoxResult ShowMessage(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage icon)
    {
        var owner = Application.Current?.MainWindow;
        return owner is null
            ? MessageBox.Show(message, title, buttons, icon)
            : MessageBox.Show(owner, message, title, buttons, icon);
    }
}
