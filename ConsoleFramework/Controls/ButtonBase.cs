using System;
using ConsoleFramework.Events;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls;

/// <summary>
/// Base class for buttons and toggle buttons (checkboxes and radio buttons).
/// </summary>
public abstract class ButtonBase : Control, ICommandSource
{
    /// <summary>Is button in clicking mode (mouse pressed but not released yet).</summary>
    private bool clicking;

    /// <summary>Is button pressed using mouse now.</summary>
    protected bool pressed;

    /// <summary>
    /// True for ~0.5s after user presses button using keyboard — for animation.
    /// </summary>
    protected bool pressedUsingKeyboard;

    private bool disabled;
    public bool Disabled
    {
        get => disabled;
        set
        {
            if (disabled == value) return;
            disabled = value;
            Focusable = !disabled;
            Invalidate();
        }
    }

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
        "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ButtonBase));

    public event RoutedEventHandler OnClick
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    protected ButtonBase()
    {
        AddHandler(MouseDownEvent, new MouseButtonEventHandler(Button_OnMouseDown));
        AddHandler(MouseUpEvent, new MouseButtonEventHandler(Button_OnMouseUp));
        AddHandler(MouseEnterEvent, new MouseEventHandler(Button_MouseEnter));
        AddHandler(MouseLeaveEvent, new MouseEventHandler(Button_MouseLeave));
        AddHandler(KeyDownEvent, new KeyEventHandler(Button_KeyDown));
        Focusable = true;
    }

    private void Button_KeyDown(object sender, KeyEventArgs args)
    {
        if (Disabled) return;
        if (args.wVirtualKeyCode is not (VirtualKeys.Space or VirtualKeys.Return)) return;

        RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
        if (command?.CanExecute(CommandParameter) == true)
            command.Execute(CommandParameter);

        pressedUsingKeyboard = true;
        Invalidate();
        ConsoleApplication.Instance.Post(() =>
        {
            pressedUsingKeyboard = false;
            Invalidate();
        }, TimeSpan.FromMilliseconds(300));
        args.Handled = true;
    }

    private void Button_MouseEnter(object sender, MouseEventArgs args)
    {
        if (!clicking || pressed) return;
        pressed = true;
        Invalidate();
    }

    private void Button_MouseLeave(object sender, MouseEventArgs args)
    {
        if (!clicking || !pressed) return;
        pressed = false;
        Invalidate();
    }

    private void Button_OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        if (clicking || Disabled) return;
        clicking = true;
        pressed = true;
        ConsoleApplication.Instance.BeginCaptureInput(this);
        Invalidate();
        args.Handled = true;
    }

    private void Button_OnMouseUp(object sender, MouseButtonEventArgs args)
    {
        if (!clicking || Disabled) return;
        clicking = false;
        if (pressed)
        {
            pressed = false;
            Invalidate();
        }
        if (HitTest(args.RawPosition))
        {
            RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
            if (command?.CanExecute(CommandParameter) == true)
                command.Execute(CommandParameter);
        }
        ConsoleApplication.Instance.EndCaptureInput(this);
        args.Handled = true;
    }

    private ICommand? command;
    public ICommand Command
    {
        get => command!;
        set
        {
            if (command == value) return;
            if (command is not null)
                command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            command = value;
            command.CanExecuteChanged += OnCommandCanExecuteChanged;
            RefreshCanExecute();
        }
    }

    private void OnCommandCanExecuteChanged(object? sender, EventArgs args) => RefreshCanExecute();

    private void RefreshCanExecute() =>
        Disabled = command is not null && !command.CanExecute(CommandParameter);

    public object? CommandParameter { get; set; }
}
