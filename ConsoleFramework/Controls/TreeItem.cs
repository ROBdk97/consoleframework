using ConsoleFramework.Binding.Observables;
using ConsoleFramework.Core;
using ConsoleFramework.Xaml;
using System.Collections.Generic;
using System.ComponentModel;

namespace ConsoleFramework.Controls
{
    [ContentProperty("Items")]
    public class TreeItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Pos in TreeView listbox.
        /// </summary>
        internal int Position;

        internal int Level;

        internal string DisplayTitle
        {
            get
            {
                if (Items.Count != 0)
                    return string.Format("{0}{1} {2}", new string(' ', Level * 2),
                        Expanded ? UnicodeTable.ArrowDown : UnicodeTable.ArrowRight, Title);
                return string.Format("{0}{1}", new string(' ', (Level + 1) * 2), Title);
            }
        }

        // todo : call listBox.Invalidate() if item is visible now
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                if (title != value)
                {
                    title = value;
                    raisePropertyChanged(nameof(Title));
                    raisePropertyChanged(nameof(DisplayTitle));
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
                    raisePropertyChanged(nameof(Disabled));
                }
            }
        }

        internal readonly ObservableList<TreeItem> items = new([]);

        public IList<TreeItem> Items { get { return items; } }

        public bool HasChildren
        {
            get { return items.Count != 0; }
        }

        public IItemsSource ItemsSource { get; set; }

        internal bool expanded;
        public bool Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                if (expanded != value)
                {
                    expanded = value;
                    raisePropertyChanged(nameof(Expanded));
                    raisePropertyChanged(nameof(DisplayTitle));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void raisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
