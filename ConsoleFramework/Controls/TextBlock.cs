using ConsoleFramework.Core;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using ConsoleFramework.Xaml;

namespace ConsoleFramework.Controls;

[ContentProperty("Text")]
public class TextBlock : Control
{
    private string text;

    private static void Initialize()
    {
    }

    public TextBlock()
    {
        Initialize();
    }

    private Color color = Color.Gray;

    public Color Color
    {
        get { return color; }
        set
        {
            if (color != value)
            {
                color = value;
                Invalidate();
            }
        }
    }

    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            if (text != value)
            {
                text = value;
                Invalidate();
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (null != text)
            return new Size(text.Length, 1);
        return new Size(0, 0);
    }

    public override void Render(RenderingBuffer buffer)
    {
        Attr attr = Colors.Blend(color, Color.Black);
        buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
        for (int x = 0; x < ActualWidth; ++x)
        {
            for (int y = 0; y < ActualHeight; ++y)
            {
                if (y == 0 && x < text.Length)
                {
                    buffer.SetPixel(x, y, text[x], attr);
                }
            }
        }
        buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 3);
    }

    public override string ToString()
    {
        return "TextBlock";
    }
}
