using System;
using System.Collections.Generic;

namespace ConsoleFramework.Binding.Observables
{
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
}