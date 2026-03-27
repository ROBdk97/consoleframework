using ConsoleFramework.Binding;
using ConsoleFramework.Binding.Converters;
using System;
using System.ComponentModel;
using System.Globalization;
using Xunit;

namespace Tests.Binding;
    public class ExplicitConverterTest
    {
        class TargetClass : INotifyPropertyChanged
        {
            private string text;
            public string Text
            {
                get { return text; }
                set
                {
                    if (text != value)
                    {
                        text = value;
                        raisePropertyChanged(nameof(Text));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        class SourceClass : INotifyPropertyChanged
        {
            public double Val
            {
                get { return sourceInt; }
                set
                {
                    if (!Equals(value, sourceInt))
                    {
                        sourceInt = value;
                        raisePropertyChanged(nameof(Val));
                    }
                }
            }

            private double sourceInt;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void raisePropertyChanged(string propertyName)
            {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        }

        class DoubleToStringConverter : IBindingConverter
    {
            public Type FirstType
            {
                get { return typeof(double); }
            }

            public Type SecondType
            {
                get { return typeof(string); }
            }

            public ConversionResult Convert(object first)
            {
                return new ConversionResult(((double)first).ToString(CultureInfo.InvariantCulture));
            }

            public ConversionResult ConvertBack(object second)
            {
            string s = (string)second;
                if (string.IsNullOrEmpty(s))
                    return new ConversionResult(false, "String is null or empty");
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return new ConversionResult(result);
            }
            return new ConversionResult(false, "Conversion failed");
            }
        }

        [Fact]
        public void TestMethod1()
        {
            TargetClass target = new();
            SourceClass source = new();
        BindingBase binding = new(target, "Text", source, "Val")
        {
            Converter = new DoubleToStringConverter()
        };
        binding.Bind();
            source.Val = 3.0f;
            Assert.True(target.Text == "3");
            target.Text = "0.5";
            Assert.True(source.Val == 0.5);
        }
    }
