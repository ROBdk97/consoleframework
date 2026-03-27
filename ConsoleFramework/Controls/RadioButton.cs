using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls;

public class RadioButton : CheckBox
{
    public override void Render(RenderingBuffer buffer)
    {
        var captionAttrs = HasFocus
            ? ControlTheme.ToggleFocused
            : ControlTheme.ToggleNormal;

        buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 3);
        buffer.SetPixel(0, 0, pressed ? '<' : '(', captionAttrs);
        buffer.SetPixel(1, 0, Checked ? '●' : ' ', captionAttrs);
        buffer.SetPixel(2, 0, pressed ? '>' : ')', captionAttrs);
        buffer.SetPixel(3, 0, ' ', captionAttrs);

        if (Caption is not null)
            RenderString(Caption, buffer, 4, 0, ActualWidth - 4, captionAttrs);
    }
}
