using System;
using ConsoleFramework.Xaml;
using Xaml;

namespace ConsoleFramework.Core;

/// <summary>
/// Converter for XAML. Supports only String → Thickness conversion.
/// </summary>
public class ThicknessConverter : ITypeConverter
{
    public bool CanConvertFrom(Type sourceType) => sourceType == typeof(string);
    public bool CanConvertTo(Type destinationType) => false;

    public object ConvertFrom(object value)
    {
        if (value is not string str) throw new NotSupportedException();

        var parts = str.Split(',');
        return parts.Length switch
        {
            1 => new Thickness(int.Parse(str)),
            2 => new Thickness(int.Parse(parts[0]), int.Parse(parts[1]),
                               int.Parse(parts[0]), int.Parse(parts[1])),
            4 => new Thickness(int.Parse(parts[0]), int.Parse(parts[1]),
                               int.Parse(parts[2]), int.Parse(parts[3])),
            _ => throw new NotSupportedException()
        };
    }

    public object ConvertTo(object value, Type destinationType) =>
        throw new NotSupportedException();
}
