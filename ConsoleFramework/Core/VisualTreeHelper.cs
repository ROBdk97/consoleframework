using System;
using System.Collections.Generic;
using ConsoleFramework.Controls;

namespace ConsoleFramework.Core;

public class VisualTreeHelper
{
    public static List<Control> FindAllChilds(Control control, Func<Control, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(predicate);

        var result = new List<Control>();
        FindAllChildsRecursively(result, control, predicate);
        return result;
    }

    private static void FindAllChildsRecursively(List<Control> result, Control control, Func<Control, bool> predicate)
    {
        foreach (var child in control.Children)
        {
            if (predicate(child)) result.Add(child);
            FindAllChildsRecursively(result, child, predicate);
        }
    }

    /// <summary>
    /// Recursively searches for a child element by name.
    /// Returns null if no matching element is found.
    /// </summary>
    public static Control? FindChildByName(Control control, string childName)
    {
        ArgumentNullException.ThrowIfNull(control);
        if (string.IsNullOrEmpty(childName)) throw new ArgumentException("String is null or empty", nameof(childName));
        return FindChildByNameRecursively(control, childName);
    }

    private static Control? FindChildByNameRecursively(Control control, string childName)
    {
        foreach (var child in control.Children)
        {
            if (child.Name == childName) return child;
            var result = FindChildByNameRecursively(child, childName);
            if (result is not null) return result;
        }
        return null;
    }

    public static bool IsConnectedToRoot(Control control)
    {
        if (control is null) throw new ArgumentNullException(nameof(control));
        var root = ConsoleApplication.Instance.RootControl;
        var current = control;
        while (current is not null)
        {
            if (current == root) return true;
            current = current.Parent;
        }
        return false;
    }

    /// <summary>
    /// Finds the closest parent of type T.
    /// Returns null if not found.
    /// </summary>
    public static T? FindClosestParent<T>(Control control) where T : Control
    {
        var tmp = control;
        while (tmp is not null and not T)
            tmp = tmp.Parent;
        return tmp as T;
    }

    /// <summary>
    /// Finds the topmost control under the mouse at rawPoint, respecting transparency and visibility.
    /// </summary>
    public static Control? FindTopControlUnderMouse(Control control, Point localPoint)
    {
        ArgumentNullException.ThrowIfNull(control);

        if (control.Children.Count > 0)
        {
            var childrenByZIndex = control.GetChildrenOrderedByZIndex();
            for (int i = childrenByZIndex.Count - 1; i >= 0; i--)
            {
                var child = childrenByZIndex[i];
                if (!Control.HitTest(Control.TranslatePoint(control, localPoint, null), control, child)) continue;
                var found = FindTopControlUnderMouse(child, Control.TranslatePoint(control, localPoint, child));
                if (found is not null) return found;
            }
        }

        var controlRect = new Rect(new Point(0, 0), control.RenderSize);
        if (!controlRect.Contains(localPoint)) return null;
        if (control.Visibility != Visibility.Visible) return null;

        int opacity = ConsoleApplication.Instance.Renderer.getControlOpacityAt(control, localPoint.X, localPoint.Y);
        return opacity is >= 4 and <= 7 ? null : control;
    }
}