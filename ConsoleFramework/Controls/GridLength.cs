using ConsoleFramework.Controls.Converter;
using ConsoleFramework.Xaml;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Represents the length of elements that explicitly support Star unit types.
    /// </summary>
    [TypeConverter(typeof(GridLengthTypeConverter))]
    public struct GridLength(GridUnitType unitType, int value)
    {
        private readonly GridUnitType gridUnitType = unitType;
        private readonly int value = value;

        public GridUnitType GridUnitType
        {
            get { return gridUnitType; }
        }

        public int Value
        {
            get { return value; }
        }
    }
}
