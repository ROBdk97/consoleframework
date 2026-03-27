using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;

namespace ConsoleFramework.Controls;

public class RadioGroup : Panel
{
    private int? selectedItemIndex;
    public int? SelectedItemIndex
    {
        get => selectedItemIndex;
        set
        {
            if (selectedItemIndex == value) return;
            selectedItemIndex = value;
            RaisePropertyChanged("SelectedItemIndex");
            RaisePropertyChanged("SelectedItem");
        }
    }

    public RadioButton? SelectedItem =>
        selectedItemIndex.HasValue ? (RadioButton)((Control)this).Children[selectedItemIndex.Value] : null;

    public RadioGroup()
    {
        Children.ControlAdded += OnControlAdded;
        Children.ControlRemoved -= OnControlRemoved;
    }

    private void OnControlRemoved(Control control)
    {
        if (control is not RadioButton radioButton) return;
        radioButton.OnClick -= RadioButton_OnClick;
    }

    private void OnControlAdded(Control control)
    {
        if (control is not RadioButton radioButton) return;
        radioButton.OnClick += RadioButton_OnClick;
        int index = ((Control)this).Children.IndexOf(radioButton);
        radioButton.Checked = selectedItemIndex is not null && selectedItemIndex == index;
    }

    private void RadioButton_OnClick(object sender, RoutedEventArgs args)
    {
        foreach (var child in Children)
        {
            if (child is RadioButton rb && child != sender)
                rb.Checked = false;
        }
        ((RadioButton)sender).Checked = true;
        SelectedItemIndex = ((Control)this).Children.IndexOf((Control)sender);
    }
}

public class RadioButton : CheckBox
{
    public override void Render(RenderingBuffer buffer)
    {
        var captionAttrs = HasFocus
            ? Colors.Blend(Color.White, Color.DarkGreen)
            : Colors.Blend(Color.Black, Color.DarkGreen);

        buffer.SetOpacityRect(0, 0, ActualWidth, ActualHeight, 3);
        buffer.SetPixel(0, 0, pressed ? '<' : '(', captionAttrs);
        buffer.SetPixel(1, 0, Checked ? 'X' : ' ', captionAttrs);
        buffer.SetPixel(2, 0, pressed ? '>' : ')', captionAttrs);
        buffer.SetPixel(3, 0, ' ', captionAttrs);

        if (Caption is not null)
            RenderString(Caption, buffer, 4, 0, ActualWidth - 4, captionAttrs);
    }
}
