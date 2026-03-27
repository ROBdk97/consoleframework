using System;

namespace ConsoleFramework.Binding.Converters;

public class ReversedConverter(IBindingConverter converter) : IBindingConverter
{
    private readonly IBindingConverter converter = converter;

    public Type FirstType => converter.SecondType;
    public Type SecondType => converter.FirstType;

    public ConversionResult Convert(object tFirst) => converter.ConvertBack(tFirst);
    public ConversionResult ConvertBack(object tSecond) => converter.Convert(tSecond);
}