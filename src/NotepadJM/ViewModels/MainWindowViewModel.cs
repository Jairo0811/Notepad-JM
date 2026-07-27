using NotepadJM.Commands;
using NotepadJM.Models;
using NotepadJM.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Threading;

namespace NotepadJM.ViewModels;

public sealed class MainWindowViewModel : ObservableObject, IDisposable
{
    private const int MaxRecentFiles = 10;
    private const double DefaultEditorFontSize = 15;
    private const double MinimumEditorFontSize = 8;
    private const double MaximumEditorFontSize = 48;

    private readonly IFileDialogService _fileDialogService;
    private readonly IUserDialogService _userDialogService;
    private readonly IDocumentFileService _documentFileService;
    private readonly IAppDataService _appDataService;
    private readonly IThemeService _themeService;
    private readonly DispatcherTimer _autoSaveTimer;
    private readonly AppSettings _settings;

    private DocumentTab? _selectedDocument;
    private double _editorFontSize = DefaultEditorFontSize;
    private bool _isWordWrapEnabled;
    private string _currentTheme;
    private bool _closedSuccessfully;
    private int _cursorLine = 1;
    private int _cursorColumn = 1;

    public MainWindowViewModel(
        IFileDialogService fileDialogService,
        IUserDialogService userDialogService,
        IDocumentFileService documentFileService,
        IAppDataService appDataService,
        IThemeService themeService)
    {
        _fileDialogService = fileDialogService;
        _userDialogService = userDialogService;
        _documentFileService = documentFileService;
        _appDataService = appDataService;
        _themeService = themeService;

        _settings = _appDataService.LoadSettings();
        _isWordWrapEnabled = _settings.WordWrap;
        _currentTheme = NormalizeTheme(_settings.Theme);

        NewDocumentCommand = new RelayCommand(CreateDocument);
        OpenCommand = new RelayCommand(OpenFromDialog);
        SaveCommand = new RelayCommand(() => SaveSelected(), HasSelectedDocument);
        SaveAsCommand = new RelayCommand(() => SaveSelectedAs(), HasSelectedDocument);
        SaveAllCommand = new RelayCommand(SaveAll, () => Documents.Count > 0);
        CloseTabCommand = new RelayCommand(CloseSelectedDocument, HasSelectedDocument);
        CloseDocumentCommand = new RelayCommand<DocumentTab>(CloseDocument);
        ExitCommand = new RelayCommand(() => ExitRequested?.Invoke(this, EventArgs.Empty));
        FindReplaceCommand = new RelayCommand(
            () => FindReplaceRequested?.Invoke(this, EventArgs.Empty),
            HasSelectedDocument);
        InsertDateTimeCommand = new RelayCommand(
            () => InsertDateTimeRequested?.Invoke(this, EventArgs.Empty),
            HasSelectedDocument);
        ZoomInCommand = new RelayCommand(() => ChangeZoom(1));
        ZoomOutCommand = new RelayCommand(() => ChangeZoom(-1));
        ResetZoomCommand = new RelayCommand(ResetZoom);
        DarkThemeCommand = new RelayCommand(() => ApplyTheme("Dark"));
        LightThemeCommand = new RelayCommand(() => ApplyTheme("Light"));
        AboutCommand = new RelayCommand(ShowAbout);

        _themeService.Apply(_currentTheme);

        _autoSaveTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _autoSaveTimer.Tick += AutoSaveTimer_Tick;

        RestoreRecoveredDocuments();
        if (Documents.Count == 0)
        {
            CreateDocument();
        }

        _autoSaveTimer.Start();
    }

    public ObservableCollection<DocumentTab> Documents { get; } = [];

    public DocumentTab? SelectedDocument
    {
        get => _selectedDocument;
        set
        {
            if (!SetProperty(ref _selectedDocument, value))
            {
                return;
            }

            UpdateCursorPosition(1, 1);
            NotifySelectedDocumentChanged();
        }
    }

    public double EditorFontSize
    {
        get => _editorFontSize;
        private set
        {
            if (SetProperty(ref _editorFontSize, value))
            {
                OnPropertyChanged(nameof(ZoomText));
            }
        }
    }

    public bool IsWordWrapEnabled
    {
        get => _isWordWrapEnabled;
        set
        {
            if (!SetProperty(ref _isWordWrapEnabled, value))
            {
                return;
            }

            _settings.WordWrap = value;
            SaveSettings();
        }
    }

    public string WindowTitle =>
        $"{SelectedDocument?.Header ?? "Sin título"} — Notepad JM";

    public string StatusPath =>
        SelectedDocument?.FilePath ?? "Sin título";

    public string CursorPositionText =>
        $"Ln {_cursorLine}, Col {_cursorColumn}";

    public string StatusLines =>
        $"Líneas: {SelectedDocument?.LineCount ?? 1}";

    public string StatusWords =>
        $"Palabras: {SelectedDocument?.WordCount ?? 0}";

    public string StatusCharacters =>
        $"Caracteres: {SelectedDocument?.CharacterCount ?? 0}";

    public string ZoomText =>
        $"{Math.Round(EditorFontSize / DefaultEditorFontSize * 100)}%";

    public ICommand NewDocumentCommand { get; }
    public ICommand OpenCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand SaveAsCommand { get; }
    public RelayCommand SaveAllCommand { get; }
    public RelayCommand CloseTabCommand { get; }
    public RelayCommand<DocumentTab> CloseDocumentCommand { get; }
    public ICommand ExitCommand { get; }
    public RelayCommand FindReplaceCommand { get; }
    public RelayCommand InsertDateTimeCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand DarkThemeCommand { get; }
    public ICommand LightThemeCommand { get; }
    public ICommand AboutCommand { get; }

    public event EventHandler? ExitRequested;
    public event EventHandler? FindReplaceRequested;
    public event EventHandler? InsertDateTimeRequested;

    public void UpdateCursorPosition(int line, int column)
    {
        _cursorLine = Math.Max(1, line);
        _cursorColumn = Math.Max(1, column);
        OnPropertyChanged(nameof(CursorPositionText));
    }

    public void OpenDocuments(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            if (File.Exists(path))
            {
                OpenDocument(path);
            }
        }
    }

    public bool TryClose()
    {
        if (_closedSuccessfully)
        {
            return true;
        }

        foreach (var document in Documents.ToList())
        {
            if (!ConfirmSave(document))
            {
                return false;
            }
        }

        _autoSaveTimer.Stop();
        SaveSettings();

        foreach (var document in Documents)
        {
            _appDataService.DeleteRecovery(document);
        }

        _closedSuccessfully = true;
        return true;
    }

    public void Dispose()
    {
        _autoSaveTimer.Stop();
        _autoSaveTimer.Tick -= AutoSaveTimer_Tick;

        foreach (var document in Documents)
        {
            document.PropertyChanged -= Document_PropertyChanged;
        }
    }

    private bool HasSelectedDocument() => SelectedDocument is not null;

    private void CreateDocument() => CreateDocument(string.Empty, null, false);

    private void CreateDocument(string content, string? path, bool isDirty)
    {
        var document = new DocumentTab();
        document.Load(content, path);
        document.IsDirty = isDirty;
        document.PropertyChanged += Document_PropertyChanged;

        Documents.Add(document);
        SelectedDocument = document;
        SaveAllCommand.NotifyCanExecuteChanged();
    }

    private void OpenFromDialog() =>
        OpenDocuments(_fileDialogService.ShowOpenFiles());

    private void OpenDocument(string path)
    {
        var existingDocument = Documents.FirstOrDefault(document =>
            string.Equals(document.FilePath, path, StringComparison.OrdinalIgnoreCase));

        if (existingDocument is not null)
        {
            SelectedDocument = existingDocument;
            return;
        }

        try
        {
            CreateDocument(_documentFileService.Read(path), path, false);
            AddRecentFile(path);
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException)
        {
            AppLogger.Log(exception, $"Open file failed: {path}");
            _userDialogService.ShowError(
                $"No se pudo abrir el archivo.\n\n{exception.Message}");
        }
    }

    private bool SaveSelected()
    {
        if (SelectedDocument is null)
        {
            return true;
        }

        return string.IsNullOrWhiteSpace(SelectedDocument.FilePath)
            ? SaveSelectedAs()
            : SaveDocument(SelectedDocument, SelectedDocument.FilePath);
    }

    private bool SaveSelectedAs()
    {
        if (SelectedDocument is null)
        {
            return true;
        }

        var suggestedFileName =
            SelectedDocument.DisplayName == "Sin título"
                ? "documento.txt"
                : SelectedDocument.DisplayName;

        var path = _fileDialogService.ShowSaveFile(suggestedFileName);
        return path is not null && SaveDocument(SelectedDocument, path);
    }

    private bool SaveDocument(DocumentTab document, string path)
    {
        try
        {
            _documentFileService.WriteAtomically(path, document.Content);

            document.FilePath = path;
            document.IsDirty = false;
            AddRecentFile(path);
            _appDataService.DeleteRecovery(document);
            NotifySelectedDocumentChanged();
            return true;
        }
        catch (Exception exception) when (
            exception is IOException
            or UnauthorizedAccessException
            or NotSupportedException)
        {
            AppLogger.Log(exception, $"Save file failed: {path}");
            _userDialogService.ShowError(
                $"No se pudo guardar el archivo.\n\n{exception.Message}");
            return false;
        }
    }

    private void SaveAll()
    {
        foreach (var document in Documents.ToList())
        {
            SelectedDocument = document;

            if (document.IsDirty && !SaveSelected())
            {
                break;
            }
        }
    }

    private bool ConfirmSave(DocumentTab document)
    {
        if (!document.IsDirty)
        {
            return true;
        }

        SelectedDocument = document;

        return _userDialogService.ConfirmSaveChanges(document.DisplayName) switch
        {
            SaveChangesChoice.Save => SaveSelected(),
            SaveChangesChoice.Discard => true,
            _ => false
        };
    }

    private void CloseSelectedDocument()
    {
        if (SelectedDocument is not null)
        {
            CloseDocument(SelectedDocument);
        }
    }

    private void CloseDocument(DocumentTab? document)
    {
        if (document is null || !ConfirmSave(document))
        {
            return;
        }

        document.PropertyChanged -= Document_PropertyChanged;
        _appDataService.DeleteRecovery(document);
        Documents.Remove(document);

        if (Documents.Count == 0)
        {
            CreateDocument();
        }
        else if (ReferenceEquals(SelectedDocument, document))
        {
            SelectedDocument = Documents.Last();
        }

        SaveAllCommand.NotifyCanExecuteChanged();
    }

    private void ChangeZoom(int delta) =>
        EditorFontSize = Math.Clamp(
            EditorFontSize + delta,
            MinimumEditorFontSize,
            MaximumEditorFontSize);

    private void ResetZoom() =>
        EditorFontSize = DefaultEditorFontSize;

    private void ApplyTheme(string theme)
    {
        _currentTheme = NormalizeTheme(theme);
        _settings.Theme = _currentTheme;
        _themeService.Apply(_currentTheme);
        SaveSettings();
    }

    private void AddRecentFile(string path)
    {
        _settings.RecentFiles.RemoveAll(recentPath =>
            string.Equals(recentPath, path, StringComparison.OrdinalIgnoreCase));

        _settings.RecentFiles.Insert(0, path);

        if (_settings.RecentFiles.Count > MaxRecentFiles)
        {
            _settings.RecentFiles.RemoveRange(
                MaxRecentFiles,
                _settings.RecentFiles.Count - MaxRecentFiles);
        }

        SaveSettings();
    }

    private void SaveSettings()
    {
        _settings.Theme = _currentTheme;
        _settings.WordWrap = IsWordWrapEnabled;
        _appDataService.SaveSettings(_settings);
    }

    private void RestoreRecoveredDocuments()
    {
        var recoveredDocuments = _appDataService.LoadRecoveredDocuments();

        if (recoveredDocuments.Count == 0)
        {
            return;
        }

        if (!_userDialogService.ConfirmRecovery())
        {
            foreach (var recoveredDocument in recoveredDocuments)
            {
                _appDataService.DeleteRecoveryFile(recoveredDocument.RecoveryFilePath);
            }

            return;
        }

        foreach (var recoveredDocument in recoveredDocuments)
        {
            CreateDocument(
                recoveredDocument.Content,
                recoveredDocument.FilePath,
                true);

            _appDataService.DeleteRecoveryFile(recoveredDocument.RecoveryFilePath);
        }
    }

    private void SaveRecoveryFiles()
    {
        foreach (var document in Documents.Where(document => document.IsDirty))
        {
            _appDataService.SaveRecovery(document);
        }
    }

    private void ShowAbout() =>
        _userDialogService.ShowInformation(
            "Notepad JM 2.0\n"
            + "Editor de texto moderno desarrollado en C# y WPF.\n\n"
            + "Proyecto original de Diseño Centrado en el Usuario — ITLA.",
            "Acerca de");

    private void NotifySelectedDocumentChanged()
    {
        OnPropertyChanged(nameof(WindowTitle));
        OnPropertyChanged(nameof(StatusPath));
        OnPropertyChanged(nameof(StatusLines));
        OnPropertyChanged(nameof(StatusWords));
        OnPropertyChanged(nameof(StatusCharacters));

        SaveCommand.NotifyCanExecuteChanged();
        SaveAsCommand.NotifyCanExecuteChanged();
        CloseTabCommand.NotifyCanExecuteChanged();
        FindReplaceCommand.NotifyCanExecuteChanged();
        InsertDateTimeCommand.NotifyCanExecuteChanged();
    }

    private void Document_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (ReferenceEquals(sender, SelectedDocument))
        {
            NotifySelectedDocumentChanged();
        }
    }

    private void AutoSaveTimer_Tick(object? sender, EventArgs e) =>
        SaveRecoveryFiles();

    private static string NormalizeTheme(string theme) =>
        string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase)
            ? "Light"
            : "Dark";
}
