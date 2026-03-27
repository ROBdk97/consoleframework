using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFramework.Controls;

[ContentProperty("Items")]
public class ContextMenu
{
    private readonly ObservableList<MenuItemBase> items = new(
        []);

    public IList<MenuItemBase> Items
    {
        get { return items; }
    }

    private MenuItem.Popup popup;
    private bool expanded;

    private bool popupShadow = true;
    public bool PopupShadow
    {
        get { return popupShadow; }
        set { popupShadow = value; }
    }

    /// <summary>
    /// Forces all open submenus to be closed.
    /// </summary>
    public void CloseAllSubmenus()
    {
        List<MenuItem> expandedSubmenus = [];
        MenuItem currentItem = (MenuItem)Items.SingleOrDefault(
            item => item is MenuItem item1 && item1.expanded);
        while (null != currentItem)
        {
            expandedSubmenus.Add(currentItem);
            currentItem = (MenuItem)currentItem.Items.SingleOrDefault(
                item => item is MenuItem menuItem && menuItem.expanded);
        }
        expandedSubmenus.Reverse();
        foreach (MenuItem expandedSubmenu in expandedSubmenus)
        {
            expandedSubmenu.Close();
        }
    }

    private WindowsHost windowsHost;
    private RoutedEventHandler windowsHostClick;
    private KeyEventHandler windowsHostControlKeyPressed;

    public void OpenMenu(WindowsHost windowsHost, Point point)
    {
        if (expanded) return;

        // We attach a handler for the MenuItem.ClickEvent to the WindowsHost,
        // so that we can catch the moment when a menu item is selected from one of the modal popup windows.
        // The thing is, these windows are not child elements of the Menu control,
        // but are directly child elements of WindowsHost (since it is WindowsHost that creates
        // the windows). And the event of selecting a menu item from a popup window can be caught 
        // in WindowsHost, but not in Menu. And we need to attach a handler that will close
        // all displayed popups.
        EventManager.AddHandler(windowsHost, MenuItem.ClickEvent,
            windowsHostClick = (sender, args) =>
            {
                CloseAllSubmenus();
                popup.Close();
            }, true);

        EventManager.AddHandler(windowsHost, MenuItem.Popup.ControlKeyPressedEvent,
            windowsHostControlKeyPressed = (sender, args) =>
            {
                CloseAllSubmenus();
                //
                //ConsoleApplication.Instance.FocusManager.SetFocusScope(this);
                if (args.wVirtualKeyCode == VirtualKeys.Right)
                    ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                else if (args.wVirtualKeyCode == VirtualKeys.Left)
                    ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                MenuItem focusedItem = (MenuItem)Items.SingleOrDefault(
                    item => item is MenuItem && item.HasFocus);
                focusedItem.Expand();
            });

        if (null == popup)
        {
            popup = new MenuItem.Popup(Items, popupShadow, 0);
            popup.AddHandler(Window.ClosedEvent, new EventHandler(onPopupClosed));
        }
        popup.X = point.X;
        popup.Y = point.Y;
        windowsHost.ShowModal(popup, true);
        expanded = true;
        this.windowsHost = windowsHost;
    }

    private void onPopupClosed(object sender, EventArgs eventArgs)
    {
        if (!expanded) throw new InvalidOperationException("This shouldn't happen");
        expanded = false;
        EventManager.RemoveHandler(windowsHost, MenuItem.ClickEvent, windowsHostClick);
        EventManager.RemoveHandler(windowsHost, MenuItem.Popup.ControlKeyPressedEvent,
            windowsHostControlKeyPressed);
    }
}