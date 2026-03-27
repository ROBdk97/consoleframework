using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Controls.Events;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFramework.Controls;

/// <summary>
/// A list of elements with the ability to select one of them.
/// </summary>
public class ListBox : Control
{
    /// <summary>
    /// The number of elements that are skipped when processing PgUp and PgDown key presses.
    /// By default null, and PgUp and PgDown presses are equivalent to Home and End presses.
    /// </summary>
    public int? PageSize { get; set; }

    private readonly ObservableList<string> items = new([]);
    public ObservableList<string> Items
    {
        get { return items; }
    }

    private int? selectedItemIndex = null;

    public event EventHandler SelectedItemIndexChanged;

    private readonly List<int> disabledItemsIndexes = [];
    public List<int> DisabledItemsIndexes
    {
        get
        {
            return disabledItemsIndexes;
        }
    }

    public int? SelectedItemIndex
    {
        get { return selectedItemIndex; }
        set
        {
            if (selectedItemIndex != value)
            {
                selectedItemIndex = value;
                SelectedItemIndexChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }
    }

    public ListBox()
    {
        Focusable = true;
        AddHandler(KeyDownEvent, new KeyEventHandler(OnKeyDown));
        AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
        AddHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMove));
        AddHandler(MouseWheelEvent, new MouseWheelEventHandler(onMouseWheel));
        items.ListChanged += (sender, args) =>
        {
            // Shift indexes of disabled items
            switch (args.Type)
            {
                case ListChangedEventType.ItemsInserted:
                    {
                        List<int> disabledIndexesCopy = [.. DisabledItemsIndexes];
                        foreach (int index in disabledIndexesCopy)
                        {
                            if (index >= args.Index)
                            {
                                DisabledItemsIndexes.Remove(index);
                                DisabledItemsIndexes.Add(index + args.Count);
                            }
                        }
                        // Selected item should stay the same after insertion
                        if (selectedItemIndex.HasValue && selectedItemIndex.Value >= args.Index)
                        {
                            SelectedItemIndex = Math.Min(items.Count - 1, selectedItemIndex.Value + args.Count);
                        }
                        break;
                    }
                case ListChangedEventType.ItemsRemoved:
                    {
                        List<int> disabledIndexesCopy = [.. DisabledItemsIndexes];
                        foreach (int index in disabledIndexesCopy)
                        {
                            if (index >= args.Index)
                            {
                                DisabledItemsIndexes.Remove(index);
                                DisabledItemsIndexes.Add(index - args.Count);
                            }
                        }
                        // When removing, selectedItemIndex should be unchanged. If it is impossible
                        // (for example, if selectedItemIndex points to disabled item now) we should reset it to null
                        if (selectedItemIndex.HasValue)
                        {
                            if (selectedItemIndex >= items.Count
                                || disabledItemsIndexes.Contains(selectedItemIndex.Value))
                            {
                                SelectedItemIndex = null;
                            }
                        }
                        break;
                    }
                case ListChangedEventType.ItemReplaced:
                    {
                        // Nothing to do
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }
            Invalidate();
        };
    }

    private void onMouseWheel(object sender, MouseWheelEventArgs args)
    {
        if (args.Delta > 0)
        {
            pageUpCore(2);
        }
        else
        {
            pageDownCore(2);
        }
        args.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs args)
    {
        if (args.LeftButton == MouseButtonState.Pressed)
        {
            int index = args.GetPosition(this).Y;
            if (!disabledItemsIndexes.Contains(index) && SelectedItemIndex != index)
            {
                SelectedItemIndex = index;
                Invalidate();
            }
        }
        args.Handled = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        int index = args.GetPosition(this).Y;
        if (!disabledItemsIndexes.Contains(index) && SelectedItemIndex != index)
        {
            SelectedItemIndex = index;
            Invalidate();
        }
        if (disabledItemsIndexes.Contains(index))
            args.Handled = true;
    }

    private void pageUpCore(int? pageSize)
    {
        if (allItemsAreDisabled) return;
        if (pageSize == null)
        {
            int firstEnabledIndex = 0;
            bool found = false;
            for (int i = 0; i < items.Count && !found; i++)
            {
                if (!disabledItemsIndexes.Contains(firstEnabledIndex)) found = true;
                else firstEnabledIndex = (firstEnabledIndex + 1) % items.Count;
            }
            if (found && selectedItemIndex != firstEnabledIndex)
            {
                SelectedItemIndex = firstEnabledIndex;
            }
        }
        else
        {
            if (SelectedItemIndex.HasValue && SelectedItemIndex != 0)
            {
                int newIndex = Math.Max(0, SelectedItemIndex.Value - pageSize.Value);

                // If it is disabled, take the first non-disabled item before
                while (disabledItemsIndexes.Contains(newIndex)
                        && newIndex > 0)
                {
                    newIndex--;
                }

                if (!disabledItemsIndexes.Contains(newIndex))
                {
                    SelectedItemIndex = newIndex;
                }
            }
        }
        currentItemShouldBeVisibleAtTop();
    }

    /// <summary>
    /// Makes the current selected item to be visible at bottom of scroll viewer
    /// (if any wrapping scroll viewer presents).
    /// Call this method after any PgDown or keyboard down arrow handling.
    /// </summary>
    private void currentItemShouldBeVisibleAtBottom()
    {
        // Notify any ScrollViewer that wraps this control to scroll visible part
        RaiseEvent(ScrollViewer.ContentShouldBeScrolledEvent,
                        new ContentShouldBeScrolledEventArgs(this,
                                                              ScrollViewer.ContentShouldBeScrolledEvent,
                                                              null, null, null,
                                                               Math.Max(0, SelectedItemIndex.Value)));
    }

    /// <summary>
    /// Makes the current selected item to be visible at top of scroll viewer
    /// (if any wrapping scroll viewer presents).
    /// Call this method after any PgUp or keyboard up arrow handling.
    /// </summary>
    private void currentItemShouldBeVisibleAtTop()
    {
        // Notify any ScrollViewer that wraps this control to scroll visible part
        RaiseEvent(ScrollViewer.ContentShouldBeScrolledEvent,
                         new ContentShouldBeScrolledEventArgs(this,
                                                               ScrollViewer.ContentShouldBeScrolledEvent,
                                                               null, null,
                                                               Math.Max(0, SelectedItemIndex.Value),
                                                               null));
    }

    private void pageDownCore(int? pageSize)
    {
        if (allItemsAreDisabled) return;
        int itemIndex = SelectedItemIndex ?? 0;
        if (pageSize == null)
        {
            if (!allItemsAreDisabled && itemIndex != items.Count - 1)
            {
                // Take the last non-disabled item
                int firstEnabledItemIndex = items.Count - 1;
                while (disabledItemsIndexes.Contains(firstEnabledItemIndex))
                {
                    firstEnabledItemIndex--;
                }

                SelectedItemIndex = firstEnabledItemIndex;
            }
        }
        else
        {
            if (itemIndex != items.Count - 1)
            {
                int newIndex = Math.Min(items.Count - 1, itemIndex + pageSize.Value);

                // If it is disabled, take the first non-disabled item after
                while (disabledItemsIndexes.Contains(newIndex)
                        && newIndex < items.Count - 1)
                {
                    newIndex++;
                }

                if (!disabledItemsIndexes.Contains(newIndex))
                {
                    SelectedItemIndex = newIndex;
                }
            }
        }
        currentItemShouldBeVisibleAtBottom();
    }

    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        if (items.Count == 0)
        {
            args.Handled = true;
            return;
        }
        if (args.wVirtualKeyCode == VirtualKeys.PageUp)
        {
            pageUpCore(PageSize);
        }
        if (args.wVirtualKeyCode == VirtualKeys.PageDown)
        {
            pageDownCore(PageSize);
        }
        if (args.wVirtualKeyCode == VirtualKeys.Up)
        {
            if (allItemsAreDisabled) return;
            do
            {
                if (SelectedItemIndex == 0 || SelectedItemIndex == null)
                    SelectedItemIndex = items.Count - 1;
                else
                {
                    SelectedItemIndex--;
                }
            } while (disabledItemsIndexes.Contains(SelectedItemIndex.Value));

            currentItemShouldBeVisibleAtTop();
        }
        if (args.wVirtualKeyCode == VirtualKeys.Down)
        {
            if (allItemsAreDisabled) return;
            do
            {
                SelectedItemIndex ??= 0;
                SelectedItemIndex = (SelectedItemIndex + 1) % items.Count;
            } while (disabledItemsIndexes.Contains(SelectedItemIndex.Value));

            currentItemShouldBeVisibleAtBottom();
        }
        args.Handled = true;
    }

    private bool allItemsAreDisabled
    {
        get { return disabledItemsIndexes.Count == items.Count; }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // If maxLen < availableSize.Width, return maxLen
        // If maxLen > availableSize.Width, return availableSize.Width,
        // and the content of rows that don't fit will be output with ellipsis
        if (items.Count == 0) return new Size(0, 0);
        int maxLen = items.Max(s => s.Length);
        // 1 pixel on the left and 1 on the right
        Size size = new(Math.Min(maxLen + 2, availableSize.Width), items.Count);
        return size;
    }

    public override void Render(RenderingBuffer buffer)
    {
        Attr selectedAttr = ControlTheme.ListSelection;
        Attr attr = ControlTheme.ListNormal;
        Attr disabledAttr = ControlTheme.ListDisabled;
        for (int y = 0; y < ActualHeight; y++)
        {
            string item = y < items.Count ? items[y] : null;

            if (item != null)
            {
                Attr currentAttr = disabledItemsIndexes.Contains(y) ? disabledAttr :
                    (SelectedItemIndex == y ? selectedAttr : attr);

                buffer.SetPixel(0, y, ' ', currentAttr);
                if (ActualWidth > 1)
                {
                    // Minus 2 because we have one empty pixel on the left and right
                    int rendered = RenderString(item, buffer, 1, y, ActualWidth - 2, currentAttr);
                    buffer.FillRectangle(1 + rendered, y, ActualWidth - (1 + rendered), 1, ' ',
                        currentAttr);
                }
            }
            else
            {
                buffer.FillRectangle(0, y, ActualWidth, 1, ' ', attr);
            }
        }
    }
}
