using ConsoleFramework.Binding;
using System;
using System.ComponentModel;
using Xunit;

namespace Tests.Binding;

public class SimplePropertiesTest
{
    class TargetClass
    {
        public string Title { get; set; }
        public int TargetInt { get; set; }
        public string TargetStr { get; set; }
    }

    class SourceClass : INotifyPropertyChanged
    {
        public string Text
        {
            get { return text; }
            set
            {
                if (value != text)
                {
                    text = value;
                    raisePropertyChanged(nameof(Text));
                }
            }
        }

        public string SourceStr
        {
            get { return sourceStr; }
            set
            {
                if (value != sourceStr)
                {
                    sourceStr = value;
                    raisePropertyChanged(nameof(SourceStr));
                }
            }
        }

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

        private string text;
        private string sourceStr;
        private int sourceInt;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void raisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    [Fact]
    public void TestString()
    {
        SourceClass source = new();
        TargetClass target = new();
        BindingBase binding = new(target, "Title", source, "Text", BindingMode.OneWay);
        binding.Bind();
        source.Text = "Text!";
        Assert.Equal(target.Title, source.Text);
    }

    [Fact]
    public void TestConversion()
    {
        SourceClass source = new();
        TargetClass target = new();
        BindingBase binding = new(target, "TargetInt", source, "SourceStr", BindingMode.OneWay);
        BindingBase binding2 = new(target, "TargetStr", source, "SourceInt", BindingMode.OneWay);
        binding.Bind();
        binding2.Bind();
        source.SourceInt = 5;
        source.SourceStr = "4";
        Assert.Equal(4, target.TargetInt);
        Assert.Equal("5", target.TargetStr);
    }

    [Fact]
    public void TestValidation()
    {
        SourceClass source = new();
        TargetClass target = new();
        BindingBase binding = new(target, "TargetStr", source, "SourceInt", BindingMode.OneWay)
        {
            UpdateSourceIfBindingFails = false
        };
        binding.Bind();
        target.TargetInt = 1;
        source.SourceStr = "invalid int";
        Assert.Equal(1, target.TargetInt);
    }
}
