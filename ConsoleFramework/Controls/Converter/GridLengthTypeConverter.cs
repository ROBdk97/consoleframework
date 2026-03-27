using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.Controls.Converter
{
    public class GridLengthTypeConverter : ITypeConverter
    {
        public bool CanConvertFrom(Type sourceType)
        {
            return Type.GetTypeCode(sourceType) switch
            {
                TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal or TypeCode.String => true,
                _ => false,
            };
        }

        public bool CanConvertTo(Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public object ConvertFrom(object value)
        {
            if (value is string s)
            {
                if (s == "Auto")
                {
                    return new GridLength(GridUnitType.Auto, 0);
                }
                else if (s.EndsWith('*'))
                {
                    if (s == "*")
                        return new GridLength(GridUnitType.Star, 1);

                    int num = int.Parse(s[..^1]);
                    return new GridLength(GridUnitType.Star, num);
                }
                else
                {
                    return new GridLength(GridUnitType.Pixel, int.Parse(s));
                }
            }
            else
            {
                int num = Convert.ToInt32(value);
                return new GridLength(GridUnitType.Pixel, num);
            }
        }

        public object ConvertTo(object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                GridLength gl = (GridLength)value;
                switch (gl.GridUnitType)
                {
                    case GridUnitType.Auto:
                        return "Auto";

                    case GridUnitType.Star:
                        if (gl.Value == 1)
                        {
                            return "*";
                        }
                        return Convert.ToString(gl.Value) + "*";
                }
                return Convert.ToString(gl.Value);
            }
            throw new NotSupportedException();
        }
    }
}
