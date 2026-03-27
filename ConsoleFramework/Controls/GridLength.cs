using ConsoleFramework.Controls.Converter;
using ConsoleFramework.Xaml;

namespace ConsoleFramework.Controls
{
    /// <summary>
    /// Represents the length of elements that explicitly support Star unit types.
    /// </summary>
    [TypeConverter(typeof(GridLengthTypeConverter))]
    public struct GridLength
    {
        private readonly GridUnitType gridUnitType;
        private readonly int value;

        public GridLength(GridUnitType unitType, int value)
        {
            gridUnitType = unitType;
            this.value = value;
        }

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
