using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;
using System;
using System.Collections.Generic;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Item of menu.
    /// </summary>
    [ContentProperty("Items")]
    public class MenuItem : MenuItemBase, ICommandSource
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuItem));

        public MenuItem ParentItem { get; internal set; }

        /// <summary>
        /// Call this method if you have changed menu items set
        /// after menu popup has been shown.
        /// </summary>
        public void ReinitializePopup()
        {
            popup?.DisconnectMenuItems();
        }

        public event RoutedEventHandler Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }

        private bool _expanded;
        internal bool expanded
        {
            get { return _expanded; }
            private set
            {
                if (_expanded != value)
                {
                    _expanded = value;
                    Invalidate();
                }
            }
        }

        private bool disabled;
        public bool Disabled
        {
            get { return disabled; }
            set
            {
                if (disabled != value)
                {
                    disabled = value;
                    Focusable = !disabled;
                    Invalidate();
                }
            }
        }

        private KeyGesture gesture;
        public KeyGesture Gesture
        {
            get { return gesture; }
            set { gesture = value; }
        }

        private bool popupShadow = true;
        public bool PopupShadow
        {
            get { return popupShadow; }
            set { popupShadow = value; }
        }

        public MenuItem()
        {
            Focusable = true;

            AddHandler(MouseDownEvent, new MouseEventHandler(onMouseDown));
            AddHandler(MouseMoveEvent, new MouseEventHandler(onMouseMove));
            AddHandler(MouseUpEvent, new MouseEventHandler(onMouseUp));
            AddHandler(KeyDownEvent, new KeyEventHandler(onKeyDown));

            // Stretch by default
            HorizontalAlignment = HorizontalAlignment.Stretch;

            items.ListChanged += (sender, args) =>
            {
                switch (args.Type)
                {
                    case ListChangedEventType.ItemsInserted:
                        {
                            for (int i = 0; i < args.Count; i++)
                            {
                                MenuItemBase itemBase = items[args.Index + i];
                                if (itemBase is MenuItem)
                                {
                                    (itemBase as MenuItem).ParentItem = this;
                                }
                            }
                            break;
                        }
                    case ListChangedEventType.ItemsRemoved:
                        foreach (object removedItem in args.RemovedItems)
                        {
                            if (removedItem is MenuItem)
                                (removedItem as MenuItem).ParentItem = null;
                        }
                        break;
                    case ListChangedEventType.ItemReplaced:
                        {
                            object removedItem = args.RemovedItems[0];
                            if (removedItem is MenuItem)
                                (removedItem as MenuItem).ParentItem = null;

                            MenuItemBase itemBase = items[args.Index];
                            if (itemBase is MenuItem)
                            {
                                (itemBase as MenuItem).ParentItem = this;
                            }
                            break;
                        }
                }
            };
        }

        private void onKeyDown(object sender, KeyEventArgs args)
        {
            if (args.wVirtualKeyCode == VirtualKeys.Return)
            {
                if (Type == MenuItemType.RootSubmenu || Type == MenuItemType.Submenu)
                    openMenu();
                else if (Type == MenuItemType.Item)
                {
                    RaiseClick();
                }
                args.Handled = true;
            }
        }

        private void onMouseUp(object sender, MouseEventArgs args)
        {
            if (Type == MenuItemType.Item)
            {
                RaiseClick();
                args.Handled = true;
            }
        }

        private void onMouseMove(object sender, MouseEventArgs args)
        {
            // Mouse move opens the submenus only in root level
            if (!disabled && args.LeftButton == MouseButtonState.Pressed /*&& Parent.Parent is Menu*/ )
            {
                openMenu();
            }
            args.Handled = true;
        }

        private void onMouseDown(object sender, MouseEventArgs args)
        {
            if (!disabled)
                openMenu();
            args.Handled = true;
        }

        private Popup popup;

        private void openMenu()
        {
            if (expanded) return;

            if (Type == MenuItemType.Submenu || Type == MenuItemType.RootSubmenu)
            {
                if (null == popup)
                {
                    popup = new Popup(Items, popupShadow, ActualWidth);
                    foreach (MenuItemBase itemBase in Items)
                    {
                        if (itemBase is MenuItem menuItem)
                            menuItem.ParentItem = this;
                    }
                    popup.AddHandler(Window.ClosedEvent, new EventHandler(onPopupClosed));
                }
                WindowsHost windowsHost = VisualTreeHelper.FindClosestParent<WindowsHost>(this);
                Point point = TranslatePoint(this, new Point(0, 0), windowsHost);
                popup.X = point.X;
                popup.Y = point.Y;
                windowsHost.ShowModal(popup, true);
                expanded = true;
            }
        }

        private void onPopupClosed(object sender, EventArgs eventArgs)
        {
            assert(expanded);
            expanded = false;
        }

        public string Title { get; set; }

        private string titleRight;
        public string TitleRight
        {
            get
            {
                if (titleRight == null && Type == MenuItemType.Submenu)
                    return new string(UnicodeTable.ArrowRight, 1);
                return titleRight;
            }
            set { titleRight = value; }
        }

        public string Description { get; set; }

        public MenuItemType Type { get; set; }

        private readonly ObservableList<MenuItemBase> items = new([]);

        public IList<MenuItemBase> Items
        {
            get { return items; }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            int length = 2;
            if (!string.IsNullOrEmpty(Title)) length += getTitleLength(Title);
            if (!string.IsNullOrEmpty(TitleRight)) length += TitleRight.Length;
            if (!string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(TitleRight))
                length++;
            return new Size(length, 1);
        }

        /// <summary>
        /// Counts length of string to be rendered with underscore prefixes on.
        /// </summary>
        private static int getTitleLength(string title)
        {
            bool underscore = false;
            int len = 0;
            foreach (char c in title)
            {
                if (underscore)
                {
                    len++;
                    underscore = false;
                }
                else
                {
                    if (c == '_')
                    {
                        underscore = true;
                    }
                    else
                    {
                        len++;
                    }
                }
            }
            return len;
        }

        public override void Render(RenderingBuffer buffer)
        {
            Attr captionAttrs;
            Attr specialAttrs;
            if (HasFocus || expanded)
            {
                captionAttrs = Colors.Blend(Color.White, Color.DarkCyan);
                specialAttrs = Colors.Blend(Color.Yellow, Color.DarkCyan);
            }
            else
            {
                captionAttrs = Colors.Blend(Color.Gray, Color.Black);
                specialAttrs = Colors.Blend(Color.Cyan, Color.Black);
            }
            if (disabled)
                captionAttrs = Colors.Blend(Color.DarkGray, Color.Black);

            buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', captionAttrs);
            if (null != Title)
            {
                renderString(Title, buffer, 1, 0, ActualWidth, captionAttrs,
                    Disabled ? captionAttrs : specialAttrs);
            }
            if (null != TitleRight)
                RenderString(TitleRight, buffer, ActualWidth - TitleRight.Length - 1, 0,
                              TitleRight.Length, captionAttrs);
        }

        /// <summary>
        /// Renders string using attr, but if character is prefixed with underscore,
        /// symbol will use specialAttrs instead. To render underscore pass two underscores.
        /// Example: "_File" renders File when 'F' is rendered using specialAttrs.
        /// </summary>
        private static int renderString(string s, RenderingBuffer buffer,
                                         int x, int y, int maxWidth, Attr attr,
                                         Attr specialAttr)
        {
            bool underscore = false;
            int j = 0;
            for (int i = 0; i < s.Length && j < maxWidth; i++)
            {
                char c;
                if (underscore)
                {
                    c = s[i];
                }
                else
                {
                    if (s[i] == '_')
                    {
                        underscore = true;
                        continue;
                    }
                    else
                    {
                        c = s[i];
                    }
                }

                Attr a;
                if (j + 2 >= maxWidth && j >= 2 && s.Length > maxWidth)
                {
                    c = '.';
                    a = attr;
                }
                else
                {
                    a = underscore ? specialAttr : attr;
                }
                buffer.SetPixel(x + j, y, c, a);

                j++;
                underscore = false;
            }
            return j;
        }

        internal class Popup : Window
        {
            private readonly bool shadow;
            private readonly int parentItemWidth; // Size of the opaque area for mouse clicks in the 1st line of the window
            private readonly Panel panel;

            public static readonly RoutedEvent ControlKeyPressedEvent = EventManager.RegisterRoutedEvent("ControlKeyPressed",
                RoutingStrategy.Bubble, typeof(KeyEventHandler), typeof(Popup));

            /// <summary>
            /// Call this method to remove all menu items that are used as child items.
            /// It is necessary before reuse MenuItems in another Popup instance.
            /// </summary>
            public void DisconnectMenuItems()
            {
                panel.Children.Clear();
            }

            /// <summary>
            /// The first line of the popup window - is special. It is completely transparent from the
            /// rendering perspective. However, Opacity for mouse events in it is different.
            /// The first width pixels in it - opaque for mouse events, but when clicked on them
            /// the window closes by calling Close(). The remaining ActualWidth - width pixels - transparent
            /// for mouse events, and a mouse click in this area causes the window
            /// WindowsHost to close the window as a window with OutsideClickClosesWindow = True.
            /// </summary>
            public Popup(IEnumerable<MenuItemBase> menuItems, bool shadow, int parentItemWidth)
            {
                this.parentItemWidth = parentItemWidth;
                this.shadow = shadow;
                panel = new Panel
                {
                    Orientation = Orientation.Vertical
                };
                foreach (MenuItemBase item in menuItems)
                {
                    panel.Children.Add(item);
                }
                Content = panel;

                // If click on the transparent header, close the popup
                AddHandler(PreviewMouseDownEvent, new MouseButtonEventHandler((sender, args) =>
                {
                    if (Content != null && !Content.RenderSlotRect.Contains(args.GetPosition(this)))
                    {
                        Close();
                        if (new Rect(new Size(parentItemWidth, 1)).Contains(args.GetPosition(this)))
                        {
                            args.Handled = true;
                        }
                    }
                }));

                EventManager.AddHandler(panel, PreviewMouseMoveEvent, new MouseEventHandler(onPanelMouseMove));
            }

            protected override void OnPreviewKeyDown(object sender, KeyEventArgs args)
            {
                switch (args.wVirtualKeyCode)
                {
                    case VirtualKeys.Right:
                        {
                            KeyEventArgs newArgs = new(this, ControlKeyPressedEvent)
                            {
                                wVirtualKeyCode = args.wVirtualKeyCode
                            };
                            RaiseEvent(ControlKeyPressedEvent, newArgs);
                            args.Handled = true;
                            break;
                        }
                    case VirtualKeys.Left:
                        {
                            KeyEventArgs newArgs = new(this, ControlKeyPressedEvent)
                            {
                                wVirtualKeyCode = args.wVirtualKeyCode
                            };
                            RaiseEvent(ControlKeyPressedEvent, newArgs);
                            args.Handled = true;
                            break;
                        }
                    case VirtualKeys.Down:
                        ConsoleApplication.Instance.FocusManager.MoveFocusNext();
                        args.Handled = true;
                        break;
                    case VirtualKeys.Up:
                        ConsoleApplication.Instance.FocusManager.MoveFocusPrev();
                        args.Handled = true;
                        break;
                    case VirtualKeys.Escape:
                        Close();
                        args.Handled = true;
                        break;
                }
            }

            private void onPanelMouseMove(object sender, MouseEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    PassFocusToChildUnderPoint(e);
                }
            }

            protected override void initialize()
            {
                AddHandler(PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
            }

            public override void Render(RenderingBuffer buffer)
            {
                Attr borderAttrs = Colors.Blend(Color.Black, Color.Gray);

                // Background
                buffer.FillRectangle(0, 1, ActualWidth, ActualHeight - 1, ' ', borderAttrs);

                // First width pixels of the first line - transparent, but don't let mouse events through
                // On click on them we close the popup manually
                buffer.SetOpacityRect(0, 0, Math.Min(ActualWidth, parentItemWidth), 1, 2);
                // Remaining pixels of the first line - let mouse events through
                // And WindowsHost will close the popup automatically on click or
                // dragging the pressed cursor over this place
                if (ActualWidth > parentItemWidth)
                    buffer.SetOpacityRect(parentItemWidth, 0, ActualWidth - parentItemWidth, 1, 6);

                if (shadow)
                {
                    buffer.SetOpacity(0, ActualHeight - 1, 2 + 4);
                    buffer.SetOpacity(ActualWidth - 1, 1, 2 + 4);
                    buffer.SetOpacityRect(ActualWidth - 1, 2, 1, ActualHeight - 2, 1 + 4);
                    buffer.FillRectangle(ActualWidth - 1, 2, 1, ActualHeight - 2, UnicodeTable.FullBlock, borderAttrs);
                    buffer.SetOpacityRect(1, ActualHeight - 1, ActualWidth - 1, 1, 3 + 4);
                    buffer.FillRectangle(1, ActualHeight - 1, ActualWidth - 1, 1, UnicodeTable.UpperHalfBlock,
                                          Attr.NO_ATTRIBUTES);
                }

                RenderBorders(buffer, new Point(1, 1),
                               shadow
                                   ? new Point(ActualWidth - 3, ActualHeight - 2)
                                   : new Point(ActualWidth - 2, ActualHeight - 1),
                               true, borderAttrs);
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                if (Content == null) return new Size(0, 0);
                if (shadow)
                {
                    // Reserve 1 line and 1 column for transparent space, the rest goes to Content
                    Content.Measure(new Size(availableSize.Width - 3, availableSize.Height - 4));
                    // +2 for left empty space and right
                    return new Size(Content.DesiredSize.Width + 3 + 2, Content.DesiredSize.Height + 4);
                }
                else
                {
                    // Reserve 1 line and 1 column for transparent space, the rest goes to Content
                    Content.Measure(new Size(availableSize.Width - 2, availableSize.Height - 3));
                    // +2 for left empty space and right
                    return new Size(Content.DesiredSize.Width + 2 + 2, Content.DesiredSize.Height + 3);
                }
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                if (Content != null)
                {
                    if (shadow)
                    {
                        // 1 pixel from all borders - for popup padding
                        // 1 pixel from top - for transparent region
                        // Additional pixel from right and bottom - for shadow
                        Content.Arrange(new Rect(new Point(2, 2),
                                                   new Size(finalSize.Width - 5, finalSize.Height - 4)));
                    }
                    else
                    {
                        // 1 pixel from all borders - for popup padding
                        // 1 pixel from top - for transparent region
                        Content.Arrange(new Rect(new Point(2, 2),
                                                   new Size(finalSize.Width - 4, finalSize.Height - 3)));
                    }
                }
                return finalSize;
            }
        }

        internal void Close()
        {
            assert(expanded);
            popup.Close();
        }

        internal void Expand()
        {
            openMenu();
        }

        private ICommand command;
        public ICommand Command
        {
            get
            {
                return command;
            }
            set
            {
                if (command != value)
                {
                    command?.CanExecuteChanged -= onCommandCanExecuteChanged;
                    command = value;
                    command.CanExecuteChanged += onCommandCanExecuteChanged;

                    refreshCanExecute();
                }
            }
        }

        private void onCommandCanExecuteChanged(object sender, EventArgs args)
        {
            refreshCanExecute();
        }

        private void refreshCanExecute()
        {
            if (command == null)
            {
                Disabled = false;
                return;
            }

            Disabled = !command.CanExecute(CommandParameter);
        }

        public object CommandParameter
        {
            get;
            set;
        }

        internal void RaiseClick()
        {
            RaiseEvent(ClickEvent, new RoutedEventArgs(this, ClickEvent));
            if (command != null && command.CanExecute(CommandParameter))
            {
                command.Execute(CommandParameter);
            }
        }
    }
}
