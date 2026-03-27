using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFramework.Binding.Observables;

/// <summary>
/// Generic implementation of <see cref="IObservableList"/>.
/// Non-generic IList is implemented to enforce compatibility with
/// Collection&lt;T&gt; and List&lt;T&gt;.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableList<T>(IList<T> list) : IObservableList, IList<T>, IList
{
    private readonly IList<T> list = list;

    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        int index = list.Count;
        list.Add(item);
        RaiseListElementsAdded(index, 1);
    }

    int IList.Add(object? value)
    {
        VerifyValueType(value);
        int count = Count;
        Add((T)value!);
        return count;
    }

    bool IList.Contains(object? value)
    {
        return IsCompatibleObject(value) && Contains((T)value!);
    }

    public void Clear()
    {
        int count = list.Count;
        List<object?> removedItems = [.. list.Cast<object?>()];
        list.Clear();

        RaiseListElementsRemoved(0, count, removedItems);
    }

    int IList.IndexOf(object? value)
    {
        return IsCompatibleObject(value) ? IndexOf((T)value!) : -1;
    }

    void IList.Insert(int index, object? value)
    {
        VerifyValueType(value);
        Insert(index, (T)value!);
    }

    void IList.Remove(object? value)
    {
        if (IsCompatibleObject(value))
        {
            Remove((T)value!);
        }
    }

    public bool Contains(T item)
    {
        return list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        int index = list.IndexOf(item);
        list.Remove(item);
        if (-1 != index)
        {
            RaiseListElementsRemoved(index, 1, [item]);
            return true;
        }
        return false;
    }

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)list).CopyTo(array, index);
    }

    public int Count
    {
        get
        {
            return list.Count;
        }
    }

    object ICollection.SyncRoot { get { return ((ICollection)list).SyncRoot; } }

    bool ICollection.IsSynchronized { get { return ((ICollection)list).IsSynchronized; } }

    public bool IsReadOnly
    {
        get
        {
            return list.IsReadOnly;
        }
    }

    bool IList.IsFixedSize { get { return ((IList)list).IsFixedSize; } }

    public int IndexOf(T item)
    {
        return list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        RaiseListElementsAdded(index, 1);
    }

    public void RemoveAt(int index)
    {
        T removedItem = list[index];
        list.RemoveAt(index);
        RaiseListElementsRemoved(index, 1, [removedItem]);
    }

    object? IList.this[int index]
    {
        get { return this[index]; }
        set
        {
            VerifyValueType(value);
            this[index] = (T)value!;
        }
    }

    public T this[int index]
    {
        get
        {
            return list[index];
        }
        set
        {
            T removedItem = list[index];
            list[index] = value;
            RaiseListElementReplaced(index, [removedItem]);
        }
    }

    private void RaiseListElementsAdded(int index, int length)
    {
        ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsInserted, index, length, null));
    }

    private void RaiseListElementsRemoved(int index, int length, List<object?> removedItems)
    {
        ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemsRemoved, index, length, removedItems));
    }

    private void RaiseListElementReplaced(int index, List<object?> removedItems)
    {
        ListChanged?.Invoke(this, new ListChangedEventArgs(ListChangedEventType.ItemReplaced, index, 1, removedItems));
    }

    private static bool IsCompatibleObject(object? value)
    {
        if (value is T)
        {
            return true;
        }

        return value is null && default(T) is null;
    }

    private static void VerifyValueType(object? value)
    {
        if (!IsCompatibleObject(value))
        {
            throw new ArgumentException($"Value must be assignable to {typeof(T).FullName}.", nameof(value));
        }
    }

    public event ListChangedHandler? ListChanged;
}