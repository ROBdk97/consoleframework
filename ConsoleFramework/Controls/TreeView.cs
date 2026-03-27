using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ConsoleFramework.Controls;

[ContentProperty("Items")]
public class TreeView : Control
{
    private readonly ObservableList<TreeItem> items = new(
        []);

    public IList<TreeItem> Items
    {
        get { return items; }
    }

    public IItemsSource? ItemsSource { get; set; }

    private readonly ListBox listBox;

    public TreeItem? SelectedItem
    {
        get
        {
            if (treeItemsFlat.Count == 0) return null;
            if (listBox.SelectedItemIndex == null) return null;
            return treeItemsFlat[listBox.SelectedItemIndex.Value];
        }
    }

    public TreeView()
    {
        listBox = new ListBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        // Stretch by default too
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        AddChild(listBox);
        items.ListChanged += ItemsOnListChanged;

        listBox.AddHandler(MouseDownEvent, new MouseEventHandler((sender, args) =>
        {
            if (!args.Handled)
            {
                if (listBox.SelectedItemIndex.HasValue)
                    ExpandCollapse(treeItemsFlat[listBox.SelectedItemIndex.Value]);
            }
        }), true);

        listBox.SelectedItemIndexChanged += (sender, args) =>
        {
            RaisePropertyChanged(nameof(SelectedItem));
        };
    }

    private void SubscribeToItem(TreeItem item, ListChangedHandler handler)
    {
        item.items.ListChanged += handler;
        item.PropertyChanged += ItemOnPropertyChanged;
        foreach (TreeItem child in item.items)
        {
            SubscribeToItem(child, handler);
        }
    }

    private void UnsubscribeFromItem(TreeItem item, ListChangedHandler handler)
    {
        item.items.ListChanged -= handler;
        item.PropertyChanged -= ItemOnPropertyChanged;
        foreach (TreeItem child in item.items)
        {
            UnsubscribeFromItem(child, handler);
        }
    }

    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (sender is not TreeItem senderItem)
        {
            return;
        }

        if (args.PropertyName == "DisplayTitle")
        {
            if (senderItem.Position >= 0)
            {
                listBox.Items[senderItem.Position] = senderItem.DisplayTitle;
            }
        }
        if (args.PropertyName == "Disabled")
        {
            if (senderItem.Position >= 0)
            {
                if (senderItem.Disabled)
                    listBox.DisabledItemsIndexes.Add(senderItem.Position);
                else
                    listBox.DisabledItemsIndexes.Remove(senderItem.Position);
            }
        }
        if (args.PropertyName == "Expanded")
        {
            if (senderItem.Position >= 0)
            {
                if (senderItem.Expanded)
                    Expand(senderItem);
                else
                    Collapse(senderItem);
            }
        }
    }

    private void EnsureFlatListIsCorrect()
    {
        for (int i = 0; i < treeItemsFlat.Count; i++)
        {
            Assert(treeItemsFlat[i].Position == i);
        }
    }

    /// <summary>
    /// Maintains the correct order of items in flat list.
    /// </summary>
    private void OnItemInserted(int pos)
    {
        TreeItem treeItem = items[pos];
        TreeItem? prevItem = null;
        if (pos > 0)
            prevItem = items[pos - 1];
        treeItem.Position = prevItem != null ? prevItem.Position + 1 : items.Count - 1;
        for (int j = treeItem.Position; j < treeItemsFlat.Count; j++)
        {
            treeItemsFlat[j].Position++;
        }
        treeItemsFlat.Insert(treeItem.Position, treeItem);
        listBox.Items.Insert(treeItem.Position, treeItem.DisplayTitle);
        if (treeItem.Disabled)
            listBox.DisabledItemsIndexes.Add(treeItem.Position);

        // Handle modification of inner list recursively
        SubscribeToItem(treeItem, ItemsOnListChanged);
        if (treeItem.Position <= listBox.SelectedItemIndex)
            RaisePropertyChanged(nameof(SelectedItem));

        EnsureFlatListIsCorrect();
    }

    private void OnItemRemoved(TreeItem treeItem)
    {
        if (treeItem.Expanded) Collapse(treeItem);
        treeItemsFlat.RemoveAt(treeItem.Position);
        listBox.Items.RemoveAt(treeItem.Position);
        for (int j = treeItem.Position; j < treeItemsFlat.Count; j++)
            treeItemsFlat[j].Position--;

        // Cleanup event handler recursively
        UnsubscribeFromItem(treeItem, ItemsOnListChanged);

        if (listBox.SelectedItemIndex >= treeItem.Position)
            RaisePropertyChanged(nameof(SelectedItem));

        EnsureFlatListIsCorrect();
    }

    private void ItemsOnListChanged(object? sender, Binding.Observables.ListChangedEventArgs args)
    {
        switch (args.Type)
        {
            case ListChangedEventType.ItemsInserted:
                {
                    for (int i = 0; i < args.Count; i++)
                        OnItemInserted(i + args.Index);
                    break;
                }
            case ListChangedEventType.ItemsRemoved:
                {
                    if (args.RemovedItems is null)
                    {
                        break;
                    }

                    foreach (TreeItem treeItem in args.RemovedItems.OfType<TreeItem>())
                        OnItemRemoved(treeItem);
                    break;
                }
            case ListChangedEventType.ItemReplaced:
                {
                    if (args.RemovedItems is { Count: > 0 } && args.RemovedItems[0] is TreeItem removedItem)
                    {
                        OnItemRemoved(removedItem);
                    }
                    OnItemInserted(args.Index);
                    break;
                }
        }
    }

    /// <summary>
    /// Flat list of tree items in order corresponding to actual listbox content.
    /// </summary>
    private readonly List<TreeItem> treeItemsFlat = [];

    private void Expand(TreeItem item)
    {
        int index = treeItemsFlat.IndexOf(item);
        for (int i = 0; i < item.Items.Count; i++)
        {
            TreeItem child = item.Items[i];
            treeItemsFlat.Insert(i + index + 1, child);
            child.Position = i + index + 1;
            child.Level = item.Level + 1;

            // Take nesting level into account in title
            listBox.Items.Insert(i + index + 1, child.DisplayTitle);
            if (child.Disabled) listBox.DisabledItemsIndexes.Add(i + index + 1);
        }
        for (int k = index + 1 + item.Items.Count; k < treeItemsFlat.Count; k++)
        {
            treeItemsFlat[k].Position += item.Items.Count;
        }

        // Children are expanded too according to their Expanded stored state
        foreach (TreeItem child in item.Items.Where(child => child.Expanded))
        {
            Expand(child);
        }

        EnsureFlatListIsCorrect();
    }

    private void Collapse(TreeItem item)
    {
        // Children are collapsed but with Expanded state saved
        foreach (TreeItem child in item.Items.Where(child => child.Expanded))
        {
            Collapse(child);
        }

        int index = treeItemsFlat.IndexOf(item);
        foreach (TreeItem child in item.Items)
        {
            treeItemsFlat.RemoveAt(index + 1);
            if (child.Disabled) listBox.DisabledItemsIndexes.Remove(index + 1);
            listBox.Items.RemoveAt(index + 1);
            child.Position = -1;
        }
        for (int k = index + 1; k < treeItemsFlat.Count; k++)
        {
            treeItemsFlat[k].Position -= item.Items.Count;
        }

        EnsureFlatListIsCorrect();
    }

    private void ExpandCollapse(TreeItem item)
    {
        int index = treeItemsFlat.IndexOf(item);
        if (item.Expanded)
        {
            Collapse(item);
            item.expanded = false;
            // Need to update item string (because Expanded status has been changed)
            listBox.Items[index] = item.DisplayTitle;
        }
        else
        {
            Expand(item);
            item.expanded = true;
            // Need to update item string (because Expanded status has been changed)
            listBox.Items[index] = item.DisplayTitle;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        listBox.Measure(availableSize);
        return listBox.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        listBox.Arrange(new Rect(finalSize));
        return finalSize;
    }
}
