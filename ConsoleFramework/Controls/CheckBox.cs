using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls;

/// <summary>
/// Represents a control that a user can select and clear.
/// </summary>
public class CheckBox : ButtonBase
{
    public CheckBox()
    {
        OnClick += (_, _) => Checked = !Checked;
    }

    private string? caption;
    public string? Caption
    {
        get => caption;
        set
        {
            if (caption == value) return;
            caption = value;
            Invalidate();
        }
    }

    private bool isChecked;
    public bool Checked
    {
        get => isChecked;
        set
        {
            if (isChecked == value) return;
            isChecked = value;
            RaisePropertyChanged("Checked");
            Invalidate();
        }
    }

    protected override Size MeasureOverride(Size availableSize) =>
        string.IsNullOrEmpty(caption) ? new Size(8, 1) : new Size(caption.Length + 4, 1);

    public override void Render(RenderingBuffer buffer)
    {
        var captionAttrs = HasFocus
            ? Colors.Blend(Color.White, Color.DarkGreen)
            : Colors.Blend(Color.Black, Color.DarkGreen);

        buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 3);
        buffer.SetPixel(0, 0, pressed ? '<' : '[', captionAttrs);
        buffer.SetPixel(1, 0, Checked ? 'X' : ' ', captionAttrs);
        buffer.SetPixel(2, 0, pressed ? '>' : ']', captionAttrs);
        buffer.SetPixel(3, 0, ' ', captionAttrs);

        if (caption is not null)
            RenderString(caption, buffer, 4, 0, ActualWidth - 4, captionAttrs);
    }
}
