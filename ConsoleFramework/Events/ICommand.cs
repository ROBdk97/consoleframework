using System;

namespace ConsoleFramework.Events;

///<summary>Allows an application author to define a method to be invoked.</summary>
public interface ICommand
{
    /// <summary>Raised when the ability of the command to execute has changed.</summary>
    event EventHandler CanExecuteChanged;

    /// <summary>Returns whether the command can be executed.</summary>
    bool CanExecute(object parameter);

    /// <summary>Defines the method to be executed when the command is invoked.</summary>
    void Execute(object parameter);
}
