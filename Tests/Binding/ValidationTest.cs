using ConsoleFramework.Binding;
using System;
using System.ComponentModel;
using Xunit;

namespace Tests.Binding;
    public class ValidationTest
    {
        class TargetClass : INotifyPropertyChanged
        {
            public string TargetStr
            {
                get { return targetStr; }
                set
                {
                    if (targetStr != value)
                    {
                        targetStr = value;
                        raisePropertyChanged(nameof(TargetStr));
                    }
                }
            }

            private string targetStr;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public int SourceInt
            {
                get { return sourceInt; }
                set
                {
                    if (value != sourceInt)
                    {
                        sourceInt = value;
                        raisePropertyChanged(nameof(SourceInt));
                    }
                }
            }

            private int sourceInt;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        [Fact]
        public void TestMethod1()
        {
            SourceClass source = new();
            TargetClass target = new();
        BindingBase binding = new(target, "TargetStr", source, "SourceInt");
        BindingResult lastResult = null;
            binding.OnBinding += result =>
            {
                lastResult = result;
            };
            binding.Bind();
            target.TargetStr = "5";
            Assert.True(source.SourceInt == 5);
            target.TargetStr = "invalid int";
            Assert.True(source.SourceInt == 0);
            Assert.True(lastResult.hasConversionError);
        }
    }
