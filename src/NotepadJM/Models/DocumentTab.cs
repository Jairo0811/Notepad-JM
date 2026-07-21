using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace NotepadJM.Models;

public sealed class DocumentTab : INotifyPropertyChanged
{
    private string _content = string.Empty;
    private string? _filePath;
    private bool _isDirty;

    public Guid Id { get; } = Guid.NewGuid();

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value) return;
            _content = value;
            IsDirty = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WordCount));
            OnPropertyChanged(nameof(CharacterCount));
            OnPropertyChanged(nameof(LineCount));
        }
    }

    public string? FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value) return;
            _filePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            if (_isDirty == value) return;
            _isDirty = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Header));
        }
    }

    public string DisplayName => string.IsNullOrWhiteSpace(FilePath) ? "Sin título" : Path.GetFileName(FilePath);
    public string Header => IsDirty ? $"{DisplayName} •" : DisplayName;
    public int CharacterCount => Content.Length;
    public int LineCount => string.IsNullOrEmpty(Content) ? 1 : Content.Count(c => c == '\n') + 1;
    public int WordCount => Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

    public void Load(string content, string? path)
    {
        _content = content;
        _filePath = path;
        _isDirty = false;
        OnPropertyChanged(string.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
