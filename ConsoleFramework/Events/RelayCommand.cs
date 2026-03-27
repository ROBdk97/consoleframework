using System;

namespace ConsoleFramework.Events;

/// <summary>
/// Command constructed from delegates.
/// CanExecute = True by default if no canExecute functor provided.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object> action;
    private readonly Func<object, bool>? canExecute;

    public RelayCommand(Action<object> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        this.action = action;
    }

    public RelayCommand(Action<object> action, Func<object, bool> canExecute) : this(action)
    {
        ArgumentNullException.ThrowIfNull(canExecute);
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object parameter) =>
        canExecute is null || canExecute.Invoke(parameter);

    public void Execute(object parameter)
    {
        if (CanExecute(parameter))
            action.Invoke(parameter);
    }

    public void RaiseCanExecuteChanged() =>
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
