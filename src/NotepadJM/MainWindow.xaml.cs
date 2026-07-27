using NotepadJM.Models;
using NotepadJM.Services;
using NotepadJM.ViewModels;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NotepadJM;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainWindowViewModel(
            new FileDialogService(),
            new UserDialogService(),
            new DocumentFileService(),
            new AppDataService(),
            new ThemeService());

        DataContext = _viewModel;
        _viewModel.ExitRequested += ViewModel_ExitRequested;
        _viewModel.FindReplaceRequested += ViewModel_FindReplaceRequested;
        _viewModel.InsertDateTimeRequested += ViewModel_InsertDateTimeRequested;

        RegisterShortcuts();
    }

    private TextBox? CurrentEditor => FindVisualChild<TextBox>(DocumentsTabControl);

    private void RegisterShortcuts()
    {
        AddShortcut(_viewModel.NewDocumentCommand, Key.N, ModifierKeys.Control);
        AddShortcut(_viewModel.OpenCommand, Key.O, ModifierKeys.Control);
        AddShortcut(_viewModel.SaveCommand, Key.S, ModifierKeys.Control);
        AddShortcut(_viewModel.SaveAsCommand, Key.S, ModifierKeys.Control | ModifierKeys.Shift);
        AddShortcut(_viewModel.SaveAllCommand, Key.S, ModifierKeys.Control | ModifierKeys.Alt);
        AddShortcut(_viewModel.CloseTabCommand, Key.W, ModifierKeys.Control);
        AddShortcut(_viewModel.FindReplaceCommand, Key.H, ModifierKeys.Control);
        AddShortcut(_viewModel.InsertDateTimeCommand, Key.F5, ModifierKeys.None);
        AddShortcut(_viewModel.ZoomInCommand, Key.OemPlus, ModifierKeys.Control);
        AddShortcut(_viewModel.ZoomInCommand, Key.Add, ModifierKeys.Control);
        AddShortcut(_viewModel.ZoomOutCommand, Key.OemMinus, ModifierKeys.Control);
        AddShortcut(_viewModel.ZoomOutCommand, Key.Subtract, ModifierKeys.Control);
        AddShortcut(_viewModel.ResetZoomCommand, Key.D0, ModifierKeys.Control);
    }

    private void AddShortcut(ICommand command, Key key, ModifierKeys modifiers) =>
        InputBindings.Add(new KeyBinding(command, key, modifiers));

    private void ViewModel_ExitRequested(object? sender, EventArgs e) => Close();

    private void ViewModel_FindReplaceRequested(object? sender, EventArgs e)
    {
        if (CurrentEditor is null)
        {
            return;
        }

        new FindReplaceWindow(CurrentEditor) { Owner = this }.Show();
    }

    private void ViewModel_InsertDateTimeRequested(object? sender, EventArgs e)
    {
        if (CurrentEditor is null)
        {
            return;
        }

        CurrentEditor.SelectedText = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        CurrentEditor.Focus();
    }

    private void Editor_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox editor)
        {
            return;
        }

        var caretIndex = editor.CaretIndex;
        var lineIndex = editor.GetLineIndexFromCharacterIndex(caretIndex);
        var lineStart = editor.GetCharacterIndexFromLineIndex(lineIndex);
        var columnIndex = caretIndex - lineStart;

        _viewModel.UpdateCursorPosition(lineIndex + 1, columnIndex + 1);
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
        {
            _viewModel.OpenDocuments(paths.Where(File.Exists));
        }
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = !_viewModel.TryClose();
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _viewModel.ExitRequested -= ViewModel_ExitRequested;
        _viewModel.FindReplaceRequested -= ViewModel_FindReplaceRequested;
        _viewModel.InsertDateTimeRequested -= ViewModel_InsertDateTimeRequested;
        _viewModel.Dispose();
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent is null)
        {
            return null;
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T typedChild)
            {
                return typedChild;
            }

            var nestedChild = FindVisualChild<T>(child);
            if (nestedChild is not null)
            {
                return nestedChild;
            }
        }

        return null;
    }
}
