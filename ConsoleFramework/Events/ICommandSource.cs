namespace ConsoleFramework.Events;

///<summary>Interface for classes that know how to invoke a Command.</summary>
public interface ICommandSource
{
    /// <summary>The command that will be executed when the class is "invoked."</summary>
    ICommand Command { get; set; }

    /// <summary>The parameter that will be passed to the command when executing.</summary>
    object CommandParameter { get; set; }
}
