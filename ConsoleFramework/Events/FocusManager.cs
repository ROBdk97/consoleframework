using ConsoleFramework.Controls;
using ConsoleFramework.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ConsoleFramework.Events;

/// <summary>
/// Responsible to manage elements that has a keyboard focus.
/// Also maintains the console mouse cursor visibility according to
/// current focused control.
/// </summary>
public sealed class FocusManager
{
    private readonly EventManager eventManager;

    public FocusManager(EventManager eventManager)
    {
        ArgumentNullException.ThrowIfNull(eventManager);
        this.eventManager = eventManager;
    }

    /// <summary>
    /// Refreshes the display of mouse cursor according to current focused element
    /// and its cursor visibility status.
    /// 
    /// Called after layout update (if there were any changes),
    /// as well as when FocusedElement changes or when the local cursor state changes
    /// on the control element that currently holds keyboard focus.
    /// </summary>
    internal void RefreshMouseCursor()
    {
        if (null != focusedElement && focusedElement.CursorVisible && focusedElement.IsPointVisible(focusedElement.CursorPosition))
        {
            ConsoleApplication.Instance.SetCursorPosition(Control.TranslatePoint(focusedElement, focusedElement.CursorPosition, null));
            if (!ConsoleApplication.Instance.CursorIsVisible)
            {
                ConsoleApplication.Instance.ShowCursor();
            }
        }
        else
        {
            if (ConsoleApplication.Instance.CursorIsVisible)
            {
                ConsoleApplication.Instance.HideCursor();
            }
        }
    }

    private Control focusedElement;

    /// <summary>
    /// Control that has a keyboard focus now.
    /// </summary>
    public Control FocusedElement
    {
        get
        {
            return focusedElement;
        }
        private set
        {
            if (focusedElement != value)
            {
                focusedElement = value;
                RefreshMouseCursor();
            }
        }
    }

    /// <summary>
    /// Takes focus away from the current focused element and sets focus to the specified control.
    /// If focusedControl is null, then FocusedElement will be set to null and keyboard
    /// input will not be processed until focus is given to another control.
    /// </summary>
    /// <param name="focusedControl"></param>
    /// <param name="ignorePreviewHandled"></param>
    /// <returns></returns>
    private bool tryChangeFocusedElementTo(Control focusedControl, bool ignorePreviewHandled = false)
    {
        if (focusedControl == FocusedElement)
        {
            return true; // do nothing
        }
        //
        Control oldFocus = FocusedElement;
        // Generate Preview-events
        // If Handled = true for at least one of the Preview-events, the method returns false
        // and focus does not change, and the Focused state for changed elements of the visual tree
        // returns as it was
        if (oldFocus != null)
        {
            KeyboardFocusChangedEventArgs previewLostArgs = new(oldFocus,
                Control.PreviewLostKeyboardFocusEvent, oldFocus, focusedControl);
            if (eventManager.ProcessRoutedEvent(previewLostArgs.RoutedEvent, previewLostArgs) && !ignorePreviewHandled)
            {
                return false;
            }
        }
        if (null != focusedControl)
        {
            KeyboardFocusChangedEventArgs previewGotArgs = new(focusedControl,
                    Control.PreviewGotKeyboardFocusEvent, oldFocus, focusedControl);
            if (eventManager.ProcessRoutedEvent(previewGotArgs.RoutedEvent, previewGotArgs) && !ignorePreviewHandled)
            {
                return false;
            }
        }

        // Change the focused element and generate main events
        FocusedElement = focusedControl;

        if (oldFocus != null)
        {
            KeyboardFocusChangedEventArgs lostArgs = new(oldFocus,
                Control.LostKeyboardFocusEvent, oldFocus, focusedControl);
            eventManager.ProcessRoutedEvent(lostArgs.RoutedEvent, lostArgs);
        }
        if (null != focusedControl)
        {
            KeyboardFocusChangedEventArgs args = new(focusedControl,
                    Control.GotKeyboardFocusEvent, oldFocus, focusedControl);
            eventManager.ProcessRoutedEvent(args.RoutedEvent, args);
        }
        //
        return true;
    }

    private Control currentScope;
    /// <summary>
    /// Current focus area
    /// </summary>
    public Control CurrentScope
    {
        get { return currentScope; }
    }

    /// <summary>
    /// Sets the current focus area. The focus area is set by the parent scope element.
    /// All its child Focusable-elements can then receive focus.
    /// Initially, the first Focusable element will receive focus.
    /// If the focus area does not contain Focusable elements, the operation will not be performed.
    /// </summary>
    /// <param name="scope"></param>
    public void SetFocusScope(Control scope)
    {
        SetFocus(scope, null);
    }

    /// <summary>
    /// Finds the first suitable focus area among the parent elements of the specified
    /// control, and sets the corresponding focus. The first suitable - this is the first
    /// up the hierarchy of controls parent control whose IsFocusScope property = True.
    /// If control is null, then focus will be removed, and keyboard input will no longer be processed
    /// (routed events assigned to the current focused element will not be generated).
    /// </summary>
    /// <param name="control"></param>
    public void SetFocus(Control control)
    {
        if (null == control)
        {
            currentScope = null;
            tryChangeFocusedElementTo(null);
            return;
        }

        Control closestFocusScope = findClosestScope(control);
        if (null == closestFocusScope)
            throw new InvalidOperationException("Cannot set focus to control because no focus scope found up to visual tree");

        SetFocus(closestFocusScope, control);
    }

    /// <summary>
    /// Finds the nearest element up the control hierarchy with IsFocusScope = True.
    /// Returns null if no such control element exists.
    /// </summary>
    private static Control findClosestScope(Control control)
    {
        Debug.Assert(null != control);
        Control currentParent = control.Parent;
        while (currentParent != null)
        {
            if (currentParent.IsFocusScope)
                return currentParent;

            currentParent = currentParent.Parent;
        }
        return null;
    }

    /// <summary>
    /// Sets the current focus area scope and gives focus to the control element
    /// </summary>
    /// <param name="scope"></param>
    /// <param name="control"></param>
    public void SetFocus(Control scope, Control control)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (!scope.IsFocusScope)
            throw new ArgumentException("IsFocusScope property should be true", nameof(scope));

        List<Control> children = getControlsInScope(scope);
        if (children.Count == 0)
        {
            if (tryChangeFocusedElementTo(null))
                currentScope = scope;
            return;
        }

        Control tofocus;
        if (null != control)
        {
            if (!children.Contains(control))
                throw new ArgumentException(
                    "Specified control is not a child of scope or is not visible or is not focusable");
            tofocus = control;
        }
        else
        {
            bool reinitFocus = false;
            // Try to restore focus from StoredFocus field
            if (scope.StoredFocus != null)
            {
                // Check if StoredFocus is not deleted and is Visible & Focusable
                if (!VisualTreeHelper.IsConnectedToRoot(scope.StoredFocus))
                {
                    reinitFocus = true;
                }
                else if (scope.StoredFocus.Visibility != Visibility.Visible)
                {
                    reinitFocus = true;
                }
                else if (!scope.StoredFocus.Focusable)
                {
                    reinitFocus = true;
                }
            }
            else
            {
                reinitFocus = true;
            }
            if (reinitFocus)
                tofocus = children[0];
            else
            {
                tofocus = scope.StoredFocus;
            }
        }

        if (tryChangeFocusedElementTo(tofocus))
        {
            currentScope = scope;
        }
    }

    /// <summary>
    /// returns visible and focusable childs of scope ordered by z-index
    /// </summary>
    private static List<Control> getControlsInScope(Control scope)
    {
        List<Control> children;
        List<Control> processed = [];
        if (scope.Focusable)
        {
            // Add the control itself if it's Focusable
            // this case can be useful if a Focusable control that is also
            // FocusScope has no child elements. In this case focus will be given to the
            // control itself (for example, an empty modal Focusable-window with special rendering)

            // If a Focusable & FocusScope control has at least 1 child Focusable-control,
            // then it will receive focus, and the FocusScope-control itself will never receive focus

            children = [scope];
        }
        else
        {
            children = [.. scope.Children];
        }
        int i = 0;
        while (i < children.Count)
        {
            Control child = children[i];
            List<Control> nested = [.. child.Children];

            // Using OrderBy instead of List<T>.Sort() because the last one is unstable
            // and can reorder elements with equal keys
            nested = nested.OrderBy(control => control.TabOrder).ToList();

            if (nested.Count > 0)
            {
                children.AddRange(nested);
                children.RemoveAt(i);
            }
            else
            {
                i++;
            }
            processed.Add(child);
        }
        List<Control> focusableAndVisible = processed.Where(
                c => c.Visibility == Visibility.Visible && c.Focusable
            ).ToList();
        return focusableAndVisible;
    }

    public void MoveFocusNext()
    {
        if (null == currentScope)
            throw new InvalidOperationException("Focus scope isn't set");
        if (null == FocusedElement)
        {
            SetFocus(currentScope, null);
            return;
        }

        List<Control> children = getControlsInScope(currentScope);
        if (children.Count == 0)
        {
            return;
        }
        int focusedIndex = children.FindIndex(c => c == FocusedElement);
        if (focusedIndex == -1)
        {
            SetFocus(currentScope, null);
            return;
        }
        else
        {
            Control child = children[(focusedIndex + 1) % children.Count];
            tryChangeFocusedElementTo(child);
        }
    }

    public void MoveFocusPrev()
    {
        if (null == currentScope)
            throw new InvalidOperationException("Focus scope isn't set");
        if (null == FocusedElement)
        {
            SetFocus(currentScope, null);
            return;
        }

        List<Control> children = getControlsInScope(currentScope);
        if (children.Count == 0)
        {
            return;
        }
        int focusedIndex = children.FindIndex(c => c == FocusedElement);
        if (focusedIndex == -1)
        {
            SetFocus(currentScope, null);
            return;
        }
        int index = focusedIndex > 0 ? focusedIndex - 1 : children.Count - 1;
        Control child = children[index];
        tryChangeFocusedElementTo(child);
    }

    /// <summary>
    /// Should be called before removing a subtree of elements.
    /// If a subtree of elements containing a control that has focus is removed,
    /// then FocusManager resets FocusedElement to null.
    /// This use case is not canceled by setting Handled = true in Preview-events.
    /// </summary>
    internal void BeforeRemoveElementFromTree(Control control)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (null != FocusedElement && isFocusedElementInSubtree(control))
        {
            tryChangeFocusedElementTo(null, true);
        }
    }

    /// <summary>
    /// Determines whether the visual element subtree starting with control
    /// contains the current element holding keyboard focus - FocusedElement.
    /// </summary>
    private bool isFocusedElementInSubtree(Control control)
    {
        Control current = FocusedElement;
        while (null != current)
        {
            if (current == control)
                return true;
            current = current.Parent;
        }
        return false;
    }

}
