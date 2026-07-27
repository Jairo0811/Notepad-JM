using Microsoft.Win32;
using System.Windows;

namespace NotepadJM.Services;

public sealed class FileDialogService : IFileDialogService
{
    private const string OpenFilter =
        "Archivos de texto y código|*.txt;*.md;*.json;*.xml;*.html;*.css;*.js;*.ts;*.cs;*.java;*.py;*.sql|Todos los archivos|*.*";

    private const string SaveFilter =
        "Texto|*.txt|Markdown|*.md|JSON|*.json|HTML|*.html|Todos los archivos|*.*";

    public IReadOnlyList<string> ShowOpenFiles()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir archivo",
            Filter = OpenFilter,
            Multiselect = true,
            CheckFileExists = true
        };

        return ShowDialog(dialog) == true
            ? dialog.FileNames
            : [];
    }

    public string? ShowSaveFile(string suggestedFileName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Guardar como",
            Filter = SaveFilter,
            FileName = suggestedFileName,
            AddExtension = true,
            OverwritePrompt = true
        };

        return ShowDialog(dialog) == true
            ? dialog.FileName
            : null;
    }

    private static bool? ShowDialog(FileDialog dialog)
    {
        var owner = Application.Current?.MainWindow;
        return owner is null
            ? dialog.ShowDialog()
            : dialog.ShowDialog(owner);
    }
}
