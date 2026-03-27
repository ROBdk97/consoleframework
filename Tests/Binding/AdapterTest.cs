using ConsoleFramework.Binding;
using ConsoleFramework.Binding.Adapters;
using System;
using System.ComponentModel;
using Xunit;

namespace Tests.Binding;
    /// <summary>
    /// Sample to show how write adapter for class that doesn't implement
    /// INotifyPropertyChanged interface.
    /// </summary>
    public class AdapterTest
    {
        class TargetClass
        {
            private string s;
            public void SetTargetStr(string str)
            {
                s = str;
                raiseSChanged();
            }

            public string GetTargetStr()
            {
                return s;
            }

            public event EventHandler SChanged;

            protected virtual void raiseSChanged()
            {
            SChanged?.Invoke(this, EventArgs.Empty);
        }
        }

        class SourceClass : INotifyPropertyChanged
        {
            private string str;
            public string Str
            {
                get { return str; }
                set
                {
                    if (str != value)
                    {
                        str = value;
                        raisePropertyChanged(nameof(Str));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        class MyAdapter : IBindingAdapter
    {
            public Type TargetType
            {
                get { return typeof(TargetClass); }
            }

            public Type GetTargetPropertyClazz(string targetProperty)
            {
                if ("S" == targetProperty) return typeof(string);
                throw new InvalidOperationException("Unsupported property");
            }

            public void SetValue(object target, string targetProperty, object value)
            {
                if ("S" == targetProperty)
                {
                    ((TargetClass)target).SetTargetStr((string)value);
                    return;
                }
                throw new InvalidOperationException("Unsupported property");
            }

            public object GetValue(object target, string targetProperty)
            {
                if ("S" == targetProperty)
                {
                    return ((TargetClass)target).GetTargetStr();
                }
                throw new InvalidOperationException("Unsupported property");
            }

            public object AddPropertyChangedListener(object target, PropertyChangedEventHandler listener)
            {
                EventHandler changedHandler = (sender, args) =>
                {
                    listener.Invoke(this, new PropertyChangedEventArgs("S"));
                };
                ((TargetClass)target).SChanged += changedHandler;
                return changedHandler;
            }

            public void RemovePropertyChangedListener(object target, object listenerWrapper)
            {
                ((TargetClass)target).SChanged -= (EventHandler)listenerWrapper;
            }

            public BindingMode DefaultMode
            {
                get { return BindingMode.TwoWay; }
            }
        }

        [Fact]
        public void TestMethod1()
        {
            SourceClass source = new();
            TargetClass target = new();
        BindingBase binding = new(target, "S", source, "Str")
        {
            Adapter = new MyAdapter()
        };
        binding.Bind();
            source.Str = "123";
            Assert.True(target.GetTargetStr() == "123");
            target.SetTargetStr("456");
            Assert.True(source.Str == "456");
            binding.Unbind();
            source.Str = "123";
            Assert.True(target.GetTargetStr() == "456");
        }
    }
