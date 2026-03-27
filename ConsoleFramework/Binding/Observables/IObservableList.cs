using System;
using System.Collections.Generic;

namespace Binding.Observables;

/// <summary>
/// Marks IList / IList&lt;T&gt; with notifications support.
/// Not derived from IList/IList&lt;T&gt; to allow both generic and non-generic implementations.
/// </summary>
public interface IObservableList
{
    event ListChangedHandler ListChanged;
}

public delegate void ListChangedHandler(object sender, ListChangedEventArgs args);

public enum ListChangedEventType { ItemsInserted, ItemsRemoved, ItemReplaced }

public class ListChangedEventArgs : EventArgs
{
    public ListChangedEventType Type { get; }
    public int Index { get; }
    public int Count { get; }
    public List<object> RemovedItems { get; }

    public ListChangedEventArgs(ListChangedEventType type, int index, int count, List<object> removedItems)
    {
        Type = type;
        Index = index;
        Count = count;
        RemovedItems = removedItems;
    }
}