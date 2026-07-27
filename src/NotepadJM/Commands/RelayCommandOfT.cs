using System.Windows.Input;

namespace NotepadJM.Commands;

public sealed class RelayCommand<T>(
    Action<T?> execute,
    Predicate<T?>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) =>
        canExecute?.Invoke(ConvertParameter(parameter)) ?? true;

    public void Execute(object? parameter) =>
        execute(ConvertParameter(parameter));

    public event EventHandler? CanExecuteChanged;

    public void NotifyCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    private static T? ConvertParameter(object? parameter) =>
        parameter is T value ? value : default;
}
