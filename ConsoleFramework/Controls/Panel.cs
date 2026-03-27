using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.Controls;

/// <summary>
/// A control that can consist of other controls.
/// Positions the controls contained in it in accordance with the internal behavior of the panel and
/// the specified properties of the child controls.
/// Like all controls, it is associated with a virtual canvas.
/// Can be the first control of the program (a window cannot, for example, it can exist
/// only within a windows host).
/// </summary>
[ContentProperty("Children")]
public class Panel : Control
{
    public Panel()
    {
        children = new UIElementCollection(this);
    }

    public Attr Background
    {
        get;
        set;
    }

    private Orientation orientation = Orientation.Vertical;

    public Orientation Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            if (orientation != value)
            {
                orientation = value;
                Invalidate();
            }
        }
    }

    private readonly UIElementCollection children;
    public new UIElementCollection Children
    {
        get { return children; }
    }

    /// <summary>
    /// Positions elements vertically in the simplest way.
    /// </summary>
    /// <param name="availableSize"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (orientation == Orientation.Vertical)
        {
            int totalHeight = 0;
            int maxWidth = 0;
            foreach (Control child in base.Children)
            {
                child.Measure(availableSize);
                totalHeight += child.DesiredSize.Height;
                if (child.DesiredSize.Width > maxWidth)
                {
                    maxWidth = child.DesiredSize.Width;
                }
            }
            foreach (Control child in base.Children)
            {
                child.Measure(new Size(maxWidth, child.DesiredSize.Height));
            }
            return new Size(maxWidth, totalHeight);
        }
        else
        {
            int totalWidth = 0;
            int maxHeight = 0;
            foreach (Control child in base.Children)
            {
                child.Measure(availableSize);
                totalWidth += child.DesiredSize.Width;
                if (child.DesiredSize.Height > maxHeight)
                    maxHeight = child.DesiredSize.Height;
            }
            foreach (Control child in base.Children)
                child.Measure(new Size(child.DesiredSize.Width, maxHeight));
            return new Size(totalWidth, maxHeight);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (orientation == Orientation.Vertical)
        {
            int totalHeight = 0;
            int maxWidth = 0;
            foreach (Control child in base.Children)
            {
                if (child.DesiredSize.Width > maxWidth)
                    maxWidth = child.DesiredSize.Width;
            }
            maxWidth = Math.Max(maxWidth, finalSize.Width);
            foreach (Control child in base.Children)
            {
                int y = totalHeight;
                int height = child.DesiredSize.Height;
                child.Arrange(new Rect(0, y, maxWidth, height));
                totalHeight += height;
            }
            return finalSize;
        }
        else
        {
            int totalWidth = 0;
            int maxHeight = 0;
            foreach (Control child in base.Children)
            {
                if (child.DesiredSize.Height > maxHeight)
                    maxHeight = child.DesiredSize.Height;
            }
            maxHeight = Math.Max(maxHeight, finalSize.Height);
            foreach (Control child in base.Children)
            {
                int x = totalWidth;
                int width = child.DesiredSize.Width;
                child.Arrange(new Rect(x, 0, width, maxHeight));
                totalWidth += width;
            }
            return finalSize;
        }
    }

    /// <summary>
    /// Draws only itself - just a background.
    /// </summary>
    /// <param name="buffer"></param>
    public override void Render(RenderingBuffer buffer)
    {
        Attr backgroundAttr = ControlTheme.PopupBackground;
        for (int x = 0; x < ActualWidth; ++x)
        {
            for (int y = 0; y < ActualHeight; ++y)
            {
                buffer.SetPixel(x, y, ' ', backgroundAttr);
                buffer.SetOpacity(x, y, 4);
            }
        }
    }
}
