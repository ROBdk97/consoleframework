using ConsoleFramework.Binding.Converters;
using System;

namespace ConsoleFramework.XamlIntegration;

public class NotBooleanConverter : IBindingConverter
{
    public Type FirstType => typeof(bool);
    public Type SecondType => typeof(bool);

    public ConversionResult Convert(object first) => new(!(bool)first);
    public ConversionResult ConvertBack(object second) => new(!(bool)second);
}
