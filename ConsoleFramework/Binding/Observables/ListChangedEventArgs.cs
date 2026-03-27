using System;
using System.Collections.Generic;

namespace ConsoleFramework.Binding.Observables
{
    public class ListChangedEventArgs(ListChangedEventType type, int index, int count, List<object?>? removedItems) : EventArgs
    {
        public ListChangedEventType Type { get; } = type;
        public int Index { get; } = index;
        public int Count { get; } = count;
        public List<object?>? RemovedItems { get; } = removedItems;
    }
}