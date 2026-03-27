using ConsoleFramework.Events;
using System;

namespace ConsoleFramework.Controls.Events
{
    /// <summary>
    /// Event args containing info for ScrollViewer - how to display inner content.
    /// </summary>
    public class ContentShouldBeScrolledEventArgs : RoutedEventArgs
    {
        private readonly int? mostLeftVisibleX;
        private readonly int? mostRightVisibleX;
        private readonly int? mostTopVisibleY;
        private readonly int? mostBottomVisibleY;

        public ContentShouldBeScrolledEventArgs(object source, RoutedEvent routedEvent,
            int? mostLeftVisibleX, int? mostRightVisibleX,
            int? mostTopVisibleY, int? mostBottomVisibleY)
            : base(source, routedEvent)
        {
            if (mostLeftVisibleX.HasValue && mostRightVisibleX.HasValue)
                throw new ArgumentException("Only one of X values can be specified");
            if (mostTopVisibleY.HasValue && mostBottomVisibleY.HasValue)
                throw new ArgumentException("Only one of Y values can be specified");
            this.mostLeftVisibleX = mostLeftVisibleX;
            this.mostRightVisibleX = mostRightVisibleX;
            this.mostTopVisibleY = mostTopVisibleY;
            this.mostBottomVisibleY = mostBottomVisibleY;
        }

        public int? MostLeftVisibleX
        {
            get { return mostLeftVisibleX; }
        }

        public int? MostRightVisibleX
        {
            get { return mostRightVisibleX; }
        }

        public int? MostTopVisibleY
        {
            get { return mostTopVisibleY; }
        }

        public int? MostBottomVisibleY
        {
            get { return mostBottomVisibleY; }
        }
    }
}
