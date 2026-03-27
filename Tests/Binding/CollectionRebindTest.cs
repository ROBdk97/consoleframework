using System;
using System.ComponentModel;
using System.Collections.Generic;
using ConsoleFramework.Binding;
using Xunit;
using ConsoleFramework.Binding.Observables;

namespace Tests.Binding;
    public class CollectionRebindTest
    {
        class TargetClass
        {
            public TargetClass()
            {
                Items = [];
            }

            public List<string> Items
            {
                get;
                set;
            }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass()
            {
                SourceItems = new ObservableList(new List<string>());
                // Rebind collection after first change
                // This first change should not affect target list
                SourceItems.ListChanged += (sender, args) =>
                {
                    SourceItems = new ObservableList(new List<string>());
                    raisePropertyChanged(nameof(SourceItems));
                };
            }

            public ObservableList SourceItems { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        [Fact]
        public void TestListRebind()
        {
            SourceClass source = new();
            TargetClass target = new();
        BindingBase binding = new(target, "Items", source, "SourceItems", BindingMode.OneWay);
            binding.Bind();
            source.SourceItems.Add("1");
            // First change should not affect target list
            Assert.True(target.Items.Count == 0);
            source.SourceItems.Add("1");
            Assert.True(target.Items[0] == "1");
            source.SourceItems.Remove("1");
            Assert.True(target.Items.Count == 0);
        }
    }
