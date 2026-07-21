using Microsoft.Win32;
using NotepadJM.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NotepadJM;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const int MaxRecentFiles = 10;
    private readonly string _appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NotepadJM");
    private readonly string _settingsPath;
    private readonly string _recoveryDirectory;
    private readonly System.Windows.Threading.DispatcherTimer _autoSaveTimer;
    private DocumentTab? _selectedDocument;
    private double _editorFontSize = 15;
    private bool _isClosing;
    private AppSettings _settings = new();

    public ObservableCollection<DocumentTab> Documents { get; } = [];

    public DocumentTab? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (_selectedDocument == value) return;
            _selectedDocument = value;
            OnPropertyChanged(nameof(SelectedDocument));
            UpdateStatus();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        Directory.CreateDirectory(_appDataDirectory);
        _settingsPath = Path.Combine(_appDataDirectory, "settings.json");
        _recoveryDirectory = Path.Combine(_appDataDirectory, "Recovery");
        Directory.CreateDirectory(_recoveryDirectory);

        LoadSettings();
        ApplyTheme(_settings.Theme);
        WordWrapMenuItem.IsChecked = _settings.WordWrap;

        _autoSaveTimer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _autoSaveTimer.Tick += (_, _) => SaveRecoveryFiles();
        _autoSaveTimer.Start();

        RegisterShortcuts();
        RestoreRecoveryFiles();
        if (Documents.Count == 0) CreateNewDocument();
    }

    private TextBox? CurrentEditor => FindVisualChild<TextBox>(DocumentsTabControl);

    private void RegisterShortcuts()
    {
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => CreateNewDocument()), Key.N, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => OpenFiles()), Key.O, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSelected()), Key.S, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveSelectedAs()), Key.S, ModifierKeys.Control | ModifierKeys.Shift));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => CloseSelectedDocument()), Key.W, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => ShowFindReplace()), Key.H, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => InsertDateTime()), Key.F5, ModifierKeys.None));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => ChangeZoom(1)), Key.OemPlus, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => ChangeZoom(-1)), Key.OemMinus, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(_ => ResetZoom()), Key.D0, ModifierKeys.Control));
    }

    private void CreateNewDocument(string content = "", string? path = null)
    {
        var document = new DocumentTab();
        document.Load(content, path);
        document.PropertyChanged += Document_PropertyChanged;
        Documents.Add(document);
        SelectedDocument = document;
        DocumentsTabControl.SelectedItem = document;
    }

    private void OpenFiles()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Abrir archivo",
            Filter = "Archivos de texto y código|*.txt;*.md;*.json;*.xml;*.html;*.css;*.js;*.ts;*.cs;*.java;*.py;*.sql|Todos los archivos|*.*",
            Multiselect = true,
            CheckFileExists = true
        };

        if (dialog.ShowDialog(this) != true) return;
        foreach (var path in dialog.FileNames) OpenDocument(path);
    }

    private void OpenDocument(string path)
    {
        try
        {
            var existing = Documents.FirstOrDefault(d => string.Equals(d.FilePath, path, StringComparison.OrdinalIgnoreCase));
            if (existing is not null)
            {
                SelectedDocument = existing;
                DocumentsTabControl.SelectedItem = existing;
                return;
            }

            var content = File.ReadAllText(path, Encoding.UTF8);
            CreateNewDocument(content, path);
            AddRecentFile(path);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"No se pudo abrir el archivo.\n\n{ex.Message}", "Notepad JM", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool SaveSelected()
    {
        if (SelectedDocument is null) return true;
        return string.IsNullOrWhiteSpace(SelectedDocument.FilePath) ? SaveSelectedAs() : SaveDocument(SelectedDocument, SelectedDocument.FilePath);
    }

    private bool SaveSelectedAs()
    {
        if (SelectedDocument is null) return true;
        var dialog = new SaveFileDialog
        {
            Title = "Guardar como",
            Filter = "Texto|*.txt|Markdown|*.md|JSON|*.json|HTML|*.html|Todos los archivos|*.*",
            FileName = SelectedDocument.DisplayName == "Sin título" ? "documento.txt" : SelectedDocument.DisplayName,
            AddExtension = true,
            OverwritePrompt = true
        };

        return dialog.ShowDialog(this) == true && SaveDocument(SelectedDocument, dialog.FileName);
    }

    private bool SaveDocument(DocumentTab document, string path)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);

            var temporaryPath = path + ".tmp";
            File.WriteAllText(temporaryPath, document.Content, new UTF8Encoding(false));
            File.Move(temporaryPath, path, true);

            document.FilePath = path;
            document.IsDirty = false;
            AddRecentFile(path);
            DeleteRecoveryFile(document);
            UpdateStatus();
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"No se pudo guardar el archivo.\n\n{ex.Message}", "Notepad JM", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void SaveAll()
    {
        foreach (var document in Documents.ToList())
        {
            SelectedDocument = document;
            DocumentsTabControl.SelectedItem = document;
            if (document.IsDirty && !SaveSelected()) break;
        }
    }

    private bool ConfirmSave(DocumentTab document)
    {
        if (!document.IsDirty) return true;

        SelectedDocument = document;
        DocumentsTabControl.SelectedItem = document;
        var result = MessageBox.Show(this, $"¿Deseas guardar los cambios en «{document.DisplayName}»?", "Notepad JM", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
        return result switch
        {
            MessageBoxResult.Yes => SaveSelected(),
            MessageBoxResult.No => true,
            _ => false
        };
    }

    private void CloseSelectedDocument()
    {
        if (SelectedDocument is null || !ConfirmSave(SelectedDocument)) return;
        var document = SelectedDocument;
        document.PropertyChanged -= Document_PropertyChanged;
        DeleteRecoveryFile(document);
        Documents.Remove(document);
        if (Documents.Count == 0) CreateNewDocument();
        SelectedDocument = Documents.LastOrDefault();
    }

    private void ShowFindReplace()
    {
        if (CurrentEditor is null) return;
        var dialog = new FindReplaceWindow(CurrentEditor) { Owner = this };
        dialog.Show();
    }

    private void InsertDateTime()
    {
        if (CurrentEditor is null) return;
        CurrentEditor.SelectedText = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        CurrentEditor.Focus();
    }

    private void ChangeZoom(int delta)
    {
        _editorFontSize = Math.Clamp(_editorFontSize + delta, 8, 48);
        if (CurrentEditor is not null) CurrentEditor.FontSize = _editorFontSize;
        StatusZoom.Text = $"{Math.Round(_editorFontSize / 15 * 100)}%";
    }

    private void ResetZoom()
    {
        _editorFontSize = 15;
        if (CurrentEditor is not null) CurrentEditor.FontSize = _editorFontSize;
        StatusZoom.Text = "100%";
    }

    private void ApplyWordWrap()
    {
        if (CurrentEditor is null) return;
        CurrentEditor.TextWrapping = WordWrapMenuItem.IsChecked ? TextWrapping.Wrap : TextWrapping.NoWrap;
        CurrentEditor.HorizontalScrollBarVisibility = WordWrapMenuItem.IsChecked ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
    }

    private void ApplyTheme(string theme)
    {
        var dark = !string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase);
        SetBrush("WindowBackgroundBrush", dark ? "#0B1020" : "#FFFFFF");
        SetBrush("SurfaceBrush", dark ? "#141B2D" : "#F1F5F9");
        SetBrush("SurfaceAltBrush", dark ? "#1B243A" : "#E2E8F0");
        SetBrush("TextBrush", dark ? "#F8FAFC" : "#0F172A");
        SetBrush("MutedTextBrush", dark ? "#94A3B8" : "#475569");
        SetBrush("BorderBrush", dark ? "#26324D" : "#CBD5E1");
        _settings.Theme = dark ? "Dark" : "Light";
        SaveSettings();
    }

    private static void SetBrush(string key, string hex) => Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    private void AddRecentFile(string path)
    {
        _settings.RecentFiles.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
        _settings.RecentFiles.Insert(0, path);
        if (_settings.RecentFiles.Count > MaxRecentFiles) _settings.RecentFiles.RemoveRange(MaxRecentFiles, _settings.RecentFiles.Count - MaxRecentFiles);
        SaveSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath)) _settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_settingsPath)) ?? new AppSettings();
        }
        catch
        {
            _settings = new AppSettings();
        }
    }

    private void SaveSettings()
    {
        try
        {
            _settings.WordWrap = WordWrapMenuItem?.IsChecked ?? true;
            File.WriteAllText(_settingsPath, JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    private void SaveRecoveryFiles()
    {
        foreach (var document in Documents.Where(d => d.IsDirty))
        {
            try
            {
                var recovery = new RecoveryDocument(document.FilePath, document.Content);
                File.WriteAllText(GetRecoveryPath(document), JsonSerializer.Serialize(recovery));
            }
            catch { }
        }
    }

    private void RestoreRecoveryFiles()
    {
        var files = Directory.GetFiles(_recoveryDirectory, "*.json");
        if (files.Length == 0) return;
        if (MessageBox.Show(this, "Notepad JM encontró documentos sin guardar de una sesión anterior. ¿Deseas recuperarlos?", "Recuperación", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            foreach (var file in files) File.Delete(file);
            return;
        }

        foreach (var file in files)
        {
            try
            {
                var recovery = JsonSerializer.Deserialize<RecoveryDocument>(File.ReadAllText(file));
                if (recovery is null) continue;
                CreateNewDocument(recovery.Content, recovery.FilePath);
                Documents[^1].IsDirty = true;
                File.Delete(file);
            }
            catch { }
        }
    }

    private string GetRecoveryPath(DocumentTab document) => Path.Combine(_recoveryDirectory, $"{document.Id:N}.json");
    private void DeleteRecoveryFile(DocumentTab document)
    {
        var path = GetRecoveryPath(document);
        if (File.Exists(path)) File.Delete(path);
    }

    private void UpdateStatus()
    {
        if (SelectedDocument is null) return;
        StatusPath.Text = SelectedDocument.FilePath ?? "Sin título";
        StatusLines.Text = $"Líneas: {SelectedDocument.LineCount}";
        StatusWords.Text = $"Palabras: {SelectedDocument.WordCount}";
        StatusChars.Text = $"Caracteres: {SelectedDocument.CharacterCount}";
        Title = $"{SelectedDocument.Header} — Notepad JM";
    }

    private void Document_PropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateStatus();

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent is null) return null;
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed) return typed;
            var nested = FindVisualChild<T>(child);
            if (nested is not null) return nested;
        }
        return null;
    }

    private void New_Click(object sender, RoutedEventArgs e) => CreateNewDocument();
    private void Open_Click(object sender, RoutedEventArgs e) => OpenFiles();
    private void Save_Click(object sender, RoutedEventArgs e) => SaveSelected();
    private void SaveAs_Click(object sender, RoutedEventArgs e) => SaveSelectedAs();
    private void SaveAll_Click(object sender, RoutedEventArgs e) => SaveAll();
    private void CloseTab_Click(object sender, RoutedEventArgs e) => CloseSelectedDocument();
    private void Exit_Click(object sender, RoutedEventArgs e) => Close();
    private void Undo_Click(object sender, RoutedEventArgs e) => CurrentEditor?.Undo();
    private void Redo_Click(object sender, RoutedEventArgs e) => CurrentEditor?.Redo();
    private void Cut_Click(object sender, RoutedEventArgs e) => CurrentEditor?.Cut();
    private void Copy_Click(object sender, RoutedEventArgs e) => CurrentEditor?.Copy();
    private void Paste_Click(object sender, RoutedEventArgs e) => CurrentEditor?.Paste();
    private void SelectAll_Click(object sender, RoutedEventArgs e) => CurrentEditor?.SelectAll();
    private void FindReplace_Click(object sender, RoutedEventArgs e) => ShowFindReplace();
    private void InsertDateTime_Click(object sender, RoutedEventArgs e) => InsertDateTime();
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => ChangeZoom(1);
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => ChangeZoom(-1);
    private void ZoomReset_Click(object sender, RoutedEventArgs e) => ResetZoom();
    private void DarkTheme_Click(object sender, RoutedEventArgs e) => ApplyTheme("Dark");
    private void LightTheme_Click(object sender, RoutedEventArgs e) => ApplyTheme("Light");
    private void WordWrap_Click(object sender, RoutedEventArgs e) { ApplyWordWrap(); SaveSettings(); }
    private void About_Click(object sender, RoutedEventArgs e) => MessageBox.Show(this, "Notepad JM 2.0\nEditor de texto moderno desarrollado en C# y WPF.\n\nProyecto original de Diseño Centrado en el Usuario — ITLA.", "Acerca de", MessageBoxButton.OK, MessageBoxImage.Information);
    private void Editor_TextChanged(object sender, TextChangedEventArgs e) { if (sender is TextBox editor) { editor.FontSize = _editorFontSize; ApplyWordWrap(); } UpdateStatus(); }
    private void DocumentsTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e) { Dispatcher.BeginInvoke(() => { ApplyWordWrap(); if (CurrentEditor is not null) CurrentEditor.FontSize = _editorFontSize; UpdateStatus(); }); }

    private void Window_DragOver(object sender, DragEventArgs e) => e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            foreach (var path in paths.Where(File.Exists)) OpenDocument(path);
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_isClosing) return;
        foreach (var document in Documents.ToList())
        {
            if (!ConfirmSave(document)) { e.Cancel = true; return; }
        }
        _isClosing = true;
        _autoSaveTimer.Stop();
        SaveSettings();
        foreach (var document in Documents) DeleteRecoveryFile(document);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private sealed class RelayCommand(Action<object?> execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute(parameter);
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    private sealed class AppSettings
    {
        public string Theme { get; set; } = "Dark";
        public bool WordWrap { get; set; } = true;
        public List<string> RecentFiles { get; set; } = [];
    }

    private sealed record RecoveryDocument(string? FilePath, string Content);
}
