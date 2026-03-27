using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.Core
{

    /// <summary>
    /// Converter for color pair. string value example: "Blue on Gray".
    /// </summary>
    public class ColorPairConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type sourceType) => Type.GetTypeCode(sourceType) == TypeCode.String;
        public bool CanConvertTo(Type destinationType) => destinationType == typeof(string);

        public object ConvertFrom(object value)
        {
            var parts = ((string)value).Split(["on"], StringSplitOptions.RemoveEmptyEntries);
            return new ColorPair(
                Enum.Parse<Color>(parts[0].Trim()),
                Enum.Parse<Color>(parts[1].Trim()));
        }

        public object ConvertTo(object value, Type destinationType)
        {
            var colorPair = (ColorPair)value;
            return $"{colorPair.ForegroundColor}:{colorPair.BackgroundColor}";
        }
    }
}
