using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.Core
{
    [TypeConverter(typeof(ColorPairConverter))]
    public class ColorPair(Color foreground, Color background) : Tuple<Color, Color>(foreground, background)
    {
        public Color ForegroundColor => Item1;
        public Color BackgroundColor => Item2;
    }
}
