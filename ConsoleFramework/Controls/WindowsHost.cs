using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Rendering;
using System;
using System.Collections.Generic;

namespace ConsoleFramework.Controls;

/// <summary>
/// A class serving as a host panel for a set of overlapping windows.
/// Stores a list of windows in their Z-Order and draws frames,
/// manages their movement.
/// </summary>
public class WindowsHost : Control
{
    private Menu mainMenu;
    public Menu MainMenu
    {
        get { return mainMenu; }
        set
        {
            if (mainMenu != value)
            {
                if (mainMenu != null)
                {
                    RemoveChild(mainMenu);
                }
                if (value != null)
                {
                    InsertChildAt(0, value);
                }
                mainMenu = value;
            }
        }
    }

    public WindowsHost()
    {
        AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler(onPreviewMouseDown), true);
        AddHandler(PreviewMouseMoveEvent, new MouseEventHandler(onPreviewMouseMove), true);
        AddHandler(PreviewMouseUpEvent, new MouseEventHandler(onPreviewMouseUp), true);
        AddHandler(PreviewKeyDownEvent, new KeyEventHandler(onPreviewKeyDown));
        AddHandler(PreviewMouseWheelEvent, new MouseWheelEventHandler(onPreviewMouseWheel));
    }

    /// <summary>
    /// Interrupts wheel event propagation if its source window is not on top now.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void onPreviewMouseWheel(object sender, MouseWheelEventArgs args)
    {
        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            windowsStartIndex++;
        }
        if (windowsStartIndex < Children.Count)
        {
            Window topWindow = (Window)Children[Children.Count - 1];
            Window sourceWindow = VisualTreeHelper.FindClosestParent<Window>((Control)args.Source);
            if (topWindow != sourceWindow)
            {
                args.Handled = true;
            }
        }
    }

    private void onPreviewKeyDown(object sender, KeyEventArgs args)
    {
        if (mainMenu != null)
        {
            if (mainMenu.TryMatchGesture(args))
            {
                args.Handled = true;
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            mainMenu.Measure(new Size(availableSize.Width, 1));
            windowsStartIndex++;
        }

        // Child windows can occupy any amount of space,
        // but with specified Width/Height their sizes will be taken into account
        // by the layout system automatically
        for (int index = windowsStartIndex; index < Children.Count; index++)
        {
            Control control = Children[index];
            Window window = (Window)control;
            window.Measure(new Size(int.MaxValue, int.MaxValue));
        }
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            mainMenu.Arrange(new Rect(0, 0, finalSize.Width, 1));
            windowsStartIndex++;
        }
        // Child windows get as much as they want
        for (int index = windowsStartIndex; index < Children.Count; index++)
        {
            Control control = Children[index];
            Window window = (Window)control;
            int x;
            if (window.X.HasValue)
            {
                x = window.X.Value;
            }
            else
            {
                x = (finalSize.Width - window.DesiredSize.Width) / 2;
            }
            int y;
            if (window.Y.HasValue)
            {
                y = window.Y.Value;
            }
            else
            {
                y = (finalSize.Height - window.DesiredSize.Height) / 2;
            }
            window.Arrange(new Rect(x, y, window.DesiredSize.Width, window.DesiredSize.Height));
        }
        return finalSize;
    }

    public override void Render(RenderingBuffer buffer)
    {
        buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', ControlTheme.PopupBackground);
    }

    /// <summary>
    /// Makes the specified window active. If it was not active before, then
    /// by Z-index it will be moved to the top, and will receive keyboard focus input.
    /// </summary>
    private void activateWindow(Window window)
    {
        int index = Children.IndexOf(window);
        if (-1 == index)
            throw new InvalidOperationException("Assertion failed.");
        //
        Control oldTopWindow = Children[Children.Count - 1];
        for (int i = index; i < Children.Count - 1; i++)
        {
            SwapChildsZOrder(i, i + 1);
        }

        // If need to change top window
        if (oldTopWindow != window)
        {
            RaiseEvent(Window.DeactivatedEvent, new RoutedEventArgs(oldTopWindow, Window.DeactivatedEvent));
            RaiseEvent(Window.ActivatedEvent, new RoutedEventArgs(window, Window.ActivatedEvent));
        }
        // If need to change focus (it is not only when need to change top window)
        // It may be need to change focus from menu to window, for example
        if (ConsoleApplication.Instance.FocusManager.CurrentScope != window)
        {
            initializeFocusOnActivatedWindow(window);
        }
    }

    private bool isTopWindowModal()
    {
        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            windowsStartIndex++;
        }

        if (Children.Count == windowsStartIndex) return false;
        return windowInfos[(Window)Children[Children.Count - 1]].Modal;
    }

    private void onPreviewMouseMove(object sender, MouseEventArgs args)
    {
        onPreviewMouseEvents(args, 2);
    }

    private void onPreviewMouseDown(object sender, MouseEventArgs args)
    {
        onPreviewMouseEvents(args, 0);
    }

    private void onPreviewMouseUp(object sender, MouseEventArgs args)
    {
        onPreviewMouseEvents(args, 1);
    }

    /// <summary>
    /// The handler is responsible for bringing inactive windows clicked with the mouse to the foreground,
    /// and for handling the mouse when there is a modal window - in this case the handler does not pass
    /// events that go past the modal window, further down the tree (Tunneling) - by setting
    /// Handled to True, or closes the modal window if it was shown with the flag
    /// OutsideClickClosesWindow.
    /// eventType = 0 - PreviewMouseDown
    /// eventType = 1 - PreviewMouseUp
    /// eventType = 2 - PreviewMouseMove
    /// </summary>
    private void onPreviewMouseEvents(MouseEventArgs args, int eventType)
    {
        bool handle = false;
    check:
        if (isTopWindowModal())
        {
            Window modalWindow = (Window)Children[Children.Count - 1];
            Window windowClicked = VisualTreeHelper.FindClosestParent<Window>((Control)args.Source);
            if (windowClicked != modalWindow)
            {
                if (windowInfos[modalWindow].OutsideClickClosesWindow
                    && (eventType == 0 || eventType == 2 && args.LeftButton == MouseButtonState.Pressed))
                {
                    // Close the current modal window
                    CloseWindow(modalWindow);

                    // Then process the event normally
                    handle = true;

                    // If there's a modal window again below, the check needs to be repeated, and close it
                    // too, and so on. This could be refactored as a call to a subroutine
                    // like while (closeTopModalWindowIfNeed()) ;
                    goto check;
                }
                else
                {
                    // Stop the propagation of the event (although controls that subscribed with the
                    // handledEventsToo flag will receive it anyway) and the generation of the corresponding
                    // paired non-preview event
                    args.Handled = true;
                }
            }
        }
        else
        {
            handle = true;
        }
        if (handle && (eventType == 0 || eventType == 2 && args.LeftButton == MouseButtonState.Pressed))
        {
            Window windowClicked = VisualTreeHelper.FindClosestParent<Window>((Control)args.Source);
            if (null != windowClicked)
            {
                activateWindow(windowClicked);
            }
            else
            {
                Menu menu = VisualTreeHelper.FindClosestParent<Menu>((Control)args.Source);
                if (null != menu)
                {
                    activateMenu();
                }
            }
        }
    }

    private void activateMenu()
    {
        assert(mainMenu != null);
        if (ConsoleApplication.Instance.FocusManager.CurrentScope != mainMenu)
            ConsoleApplication.Instance.FocusManager.SetFocusScope(mainMenu);
    }

    private static void initializeFocusOnActivatedWindow(Window window)
    {
        ConsoleApplication.Instance.FocusManager.SetFocusScope(window);
        // todo : add window.ChildToFocus support again
    }

    private class WindowInfo
    {
        public readonly bool Modal;
        public readonly bool OutsideClickClosesWindow;

        public WindowInfo(bool modal, bool outsideClickClosesWindow)
        {
            Modal = modal;
            OutsideClickClosesWindow = outsideClickClosesWindow;
        }
    }

    private readonly Dictionary<Window, WindowInfo> windowInfos = [];

    /// <summary>
    /// Adds window to window host children and shows it as modal window.
    /// </summary>
    public void ShowModal(Window window, bool outsideClickWillCloseWindow = false)
    {
        showCore(window, true, outsideClickWillCloseWindow);
    }

    /// <summary>
    /// Adds window to window host children and shows it.
    /// </summary>
    public void Show(Window window)
    {
        showCore(window, false, false);
    }

    public Window TopWindow => getTopWindow();
    private Window getTopWindow()
    {
        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            windowsStartIndex++;
        }
        if (Children.Count > windowsStartIndex)
        {
            return (Window)Children[Children.Count - 1];
        }
        return null;
    }

    private void showCore(Window window, bool modal, bool outsideClickWillCloseWindow)
    {
        Control topWindow = getTopWindow();
        if (null != topWindow)
        {
            RaiseEvent(Window.DeactivatedEvent,
                                    new RoutedEventArgs(topWindow, Window.DeactivatedEvent));
        }

        AddChild(window);
        RaiseEvent(Window.ActivatedEvent, new RoutedEventArgs(window, Window.ActivatedEvent));
        initializeFocusOnActivatedWindow(window);
        windowInfos.Add(window, new WindowInfo(modal, outsideClickWillCloseWindow));
    }

    /// <summary>
    /// Removes window from window host.
    /// </summary>
    public void CloseWindow(Window window)
    {
        windowInfos.Remove(window);
        RaiseEvent(Window.DeactivatedEvent, new RoutedEventArgs(window, Window.DeactivatedEvent));
        RemoveChild(window);
        RaiseEvent(Window.ClosedEvent, new RoutedEventArgs(window, Window.ClosedEvent));
        // After removing the window, activate the one that was active before it
        IList<Control> childrenOrderedByZIndex = GetChildrenOrderedByZIndex();

        int windowsStartIndex = 0;
        if (mainMenu != null)
        {
            assert(Children[0] == mainMenu);
            windowsStartIndex++;
        }

        if (childrenOrderedByZIndex.Count > windowsStartIndex)
        {
            Window topWindow = (Window)childrenOrderedByZIndex[childrenOrderedByZIndex.Count - 1];
            RaiseEvent(Window.ActivatedEvent, new RoutedEventArgs(topWindow, Window.ActivatedEvent));
            initializeFocusOnActivatedWindow(topWindow);
            Invalidate();
        }
    }
}
