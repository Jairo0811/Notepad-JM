using System.Windows;
using System.Windows.Controls;

namespace NotepadJM;

public partial class FindReplaceWindow : Window
{
    private readonly TextBox _editor;

    public FindReplaceWindow(TextBox editor)
    {
        InitializeComponent();
        _editor = editor;
        FindTextBox.Text = editor.SelectedText;
        FindTextBox.Focus();
        FindTextBox.SelectAll();
    }

    private StringComparison Comparison => MatchCaseCheckBox.IsChecked == true
        ? StringComparison.CurrentCulture
        : StringComparison.CurrentCultureIgnoreCase;

    private bool FindNext()
    {
        var query = FindTextBox.Text;
        if (string.IsNullOrEmpty(query)) return false;

        var start = _editor.SelectionStart + _editor.SelectionLength;
        var index = _editor.Text.IndexOf(query, start, Comparison);
        if (index < 0) index = _editor.Text.IndexOf(query, 0, Comparison);
        if (index < 0)
        {
            MessageBox.Show(this, "No se encontraron más coincidencias.", "Notepad JM", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        _editor.Select(index, query.Length);
        _editor.Focus();
        return true;
    }

    private void FindNext_Click(object sender, RoutedEventArgs e) => FindNext();

    private void Replace_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_editor.SelectedText) && string.Equals(_editor.SelectedText, FindTextBox.Text, Comparison))
            _editor.SelectedText = ReplaceTextBox.Text;
        FindNext();
    }

    private void ReplaceAll_Click(object sender, RoutedEventArgs e)
    {
        var query = FindTextBox.Text;
        if (string.IsNullOrEmpty(query)) return;

        var text = _editor.Text;
        var index = text.IndexOf(query, Comparison);
        var count = 0;
        while (index >= 0)
        {
            text = text.Remove(index, query.Length).Insert(index, ReplaceTextBox.Text);
            index = text.IndexOf(query, index + ReplaceTextBox.Text.Length, Comparison);
            count++;
        }
        _editor.Text = text;
        MessageBox.Show(this, $"Se reemplazaron {count} coincidencias.", "Notepad JM", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
