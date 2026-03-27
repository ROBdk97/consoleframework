using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFramework.Controls;

public class Menu : Control
{
    private readonly ObservableList<MenuItemBase> items = new(
        []);
    public IList<MenuItemBase> Items
    {
        get { return items; }
    }

    private static void GetGestures(MenuItem item, Dictionary<KeyGesture, MenuItem> map)
    {
        if (item.Gesture != null)
            map.Add(item.Gesture, item);
        if (item.Type == MenuItemType.RootSubmenu ||
             item.Type == MenuItemType.Submenu)
        {
            foreach (MenuItemBase itemBase in item.Items)
            {
                if (itemBase is MenuItem menueItem)
                {
                    GetGestures(menueItem, map);
                }
            }
        }
    }

    public void RefreshKeyGestures()
    {
        gestures = null;
    }

    private Dictionary<KeyGesture, MenuItem>? gestures;

    private Dictionary<KeyGesture, MenuItem> GetGesturesMap()
    {
        if (gestures == null)
        {
            gestures = [];
            foreach (MenuItemBase itemBase in Items)
            {
                if (itemBase is MenuItem item)
                {
                    GetGestures(item, gestures);
                }
            }
        }
        return gestures;
    }

    public bool TryMatchGesture(KeyEventArgs args)
    {
        Dictionary<KeyGesture, MenuItem> map = GetGesturesMap();
        KeyGesture match = map.Keys.FirstOrDefault(gesture => gesture.Matches(args));
        if (match == null) return false;

        CloseAllSubmenus();

        // Activate matches menu item
        MenuItem menuItem = map[match];
        List<MenuItem> path = [];
        MenuItem? currentItem = menuItem;
        while (currentItem != null)
        {
            path.Add(currentItem);
            currentItem = currentItem.ParentItem;
        }
        path.Reverse();

        // Open all menu items in path successively
        int i = 0;
        Action? action = null;
        action = new Action(() =>
        {
            if (i < path.Count)
            {
                MenuItem item = path[i];
                if (item.Type == MenuItemType.Item)
                {
                    item.RaiseClick();
                    return;
                }

                // Activate focus on item
                if (item.ParentItem == null)
                {
                    ConsoleApplication.Instance.FocusManager.SetFocus(this, item);
                }
                else
                {
                    // Set focus to PopupWindow -> item
                    ConsoleApplication.Instance.FocusManager.SetFocus(
                        item.Parent.Parent, item);
                }
                item.Invalidate();
                void handler(object? o, EventArgs eventArgs)
                {
                    item.Expand();
                    item.LayoutRevalidated -= handler;
                    i++;
                    if (i < path.Count)
                    {
                        action?.Invoke();
                    }
                }

                item.LayoutRevalidated += handler;
            }
        });
        action();

        return true;
    }

    /// <summary>
    /// Forces all open submenus to be closed.
    /// </summary>
    public void CloseAllSubmenus()
    {
        List<MenuItem> expandedSubmenus = [];
        MenuItem? currentItem = Items.SingleOrDefault(
            item => item is MenuItem menuItem && menuItem.Expanded) as MenuItem;
        while (currentItem != null)
        {
            expandedSubmenus.Add(currentItem);
            currentItem = currentItem.Items.SingleOrDefault(
                item => item is MenuItem menuItem && menuItem.Expanded) as MenuItem;
        }
        expandedSubmenus.Reverse();
        foreach (MenuItem expandedSubmenu in expandedSubmenus)
        {
            expandedSubmenu.Close();
        }
    }

    public Menu()
    {
        Panel stackPanel = new()
        {
            Orientation = Orientation.Horizontal
        };
        AddChild(stackPanel);

        // Subscribe to Items change and add to Children them
        items.ListChanged += (sender, args) =>
        {
            switch (args.Type)
            {
                case ListChangedEventType.ItemsInserted:
                    {
                        for (int i = 0; i < args.Count; i++)
                        {
                            MenuItemBase item = items[args.Index + i];
                            if (item is Separator)
                                throw new InvalidOperationException("Separator cannot be added to root menu.");
                            if (((MenuItem)item).Type == MenuItemType.Submenu)
                                ((MenuItem)item).Type = MenuItemType.RootSubmenu;
                            stackPanel.Children.Insert(args.Index + i, item);
                        }
                        break;
                    }
                case ListChangedEventType.ItemsRemoved:
                    for (int i = 0; i < args.Count; i++)
                        stackPanel.Children.RemoveAt(args.Index);
                    break;
                case ListChangedEventType.ItemReplaced:
                    {
                        MenuItemBase item = items[args.Index];
                        if (item is Separator)
                            throw new InvalidOperationException("Separator cannot be added to root menu.");
                        if (((MenuItem)item).Type == MenuItemType.Submenu)
                            ((MenuItem)item).Type = MenuItemType.RootSubmenu;
                        stackPanel.Children[args.Index] = item;
                        break;
                    }
            }
        };
        IsFocusScope = true;

        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown));
        AddHandler(PreviewMouseMoveEvent, new MouseEventHandler(OnPreviewMouseMove));
        AddHandler(PreviewMouseDownEvent, new MouseEventHandler(OnPreviewMouseDown));
    }

    protected override void OnParentChanged()
    {
        if (Parent != null)
        {
            Assert(Parent is WindowsHost);

            // Attach to WindowsHost a handler for the MenuItem.ClickEvent event,
            // to catch the moment of menu item selection in one of the modal popup windows.
            // The thing is that these windows are not child elements of the Menu control,
            // but are directly child elements of WindowsHost (because it creates
            // windows). And the menu item selection event from the popup window can be caught
            // in WindowsHost, but not in Menu. And we need to attach a handler that closes
            // all shown popups.
            EventManager.AddHandler(Parent, MenuItem.ClickEvent,
                new RoutedEventHandler((sender, args) => CloseAllSubmenus()), true);

            EventManager.AddHandler(Parent, MenuItem.Popup.ControlKeyPressedEvent,
                new KeyEventHandler((sender, args) =>
                {
                    CloseAllSubmenus();
                    //
                    ConsoleApplication.Instance.FocusManager.SetFocusScope(this);
                    if (args.wVirtualKeyCode == VirtualKeys.Right)
                        ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                    else if (args.wVirtualKeyCode == VirtualKeys.Left)
                        ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                    MenuItem? focusedItem = Items.SingleOrDefault(
                        item => item is MenuItem && item.HasFocus) as MenuItem;
                    focusedItem?.Expand();
                }));
        }
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs args)
    {
        if (args.LeftButton == MouseButtonState.Pressed)
        {
            OnPreviewMouseDown(sender, args);
        }
    }

    private void OnPreviewMouseDown(object sender, MouseEventArgs e)
    {
        PassFocusToChildUnderPoint(e);
    }

    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        if (args.wVirtualKeyCode == VirtualKeys.Right)
        {
            ConsoleApplication.Instance.FocusManager.MoveFocusNext();
            args.Handled = true;
        }
        if (args.wVirtualKeyCode == VirtualKeys.Left)
        {
            ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
            args.Handled = true;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Children[0].Measure(availableSize);
        return Children[0].DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Children[0].Arrange(new Rect(new Point(0, 0), finalSize));
        return finalSize;
    }

    public override void Render(RenderingBuffer buffer)
    {
        Attr attr = Colors.Blend(Color.Black, Color.Gray);
        buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
    }
}
