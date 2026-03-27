using ConsoleFramework.Binding;
using ConsoleFramework.Binding.Observables;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Xunit;

namespace Tests.Binding;
    public class CollectionsTest
    {
        class TargetClass
        {
            public TargetClass()
            {
                Items = [];
            }

            public List<string> Items { get; set; }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public SourceClass()
            {
                SourceItems = new ObservableList<string>([]);
            }

            public ObservableList<string> SourceItems { get; private set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        [Fact]
        public void TestListBinding()
        {
            SourceClass source = new();
            TargetClass target = new();
        BindingBase binding = new(target, "Items", source, "SourceItems", BindingMode.OneWay);
            binding.Bind();
            source.SourceItems.Add("1");
            Assert.True(target.Items[0] == "1");
            source.SourceItems.Add("2");
            Assert.True(target.Items[0] == "1");
            Assert.True(target.Items[1] == "2");
            source.SourceItems.Remove("1");
            Assert.True(target.Items.Count == 1);
            Assert.True(target.Items[0] == "2");
        }

        [Fact]
        public void TestListBinding2()
        {
            SourceClass source = new();
            TargetClass target = new();
        BindingBase binding = new(target, "Items", source, "SourceItems", BindingMode.OneWay);
            source.SourceItems.Add("1");
            binding.Bind();
            Assert.True(target.Items[0] == "1");
            source.SourceItems.Remove("1");
            Assert.True(target.Items.Count == 0);
        }
    }