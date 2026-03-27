using System;
using System.Globalization;

namespace Binding.Converters;

/// <summary>
/// Converter between String and Integer.
/// </summary>
public class StringToIntegerConverter : IBindingConverter
{
    public Type FirstType => typeof(string);
    public Type SecondType => typeof(int);

    public ConversionResult Convert(object s)
    {
        if (s is not string str) return new ConversionResult(false, "String is null");
        return int.TryParse(str, out int value)
            ? new ConversionResult(value)
            : new ConversionResult(false, "Incorrect number");
    }

    public ConversionResult ConvertBack(object integer) =>
        new(((int)integer).ToString(CultureInfo.InvariantCulture));
}
