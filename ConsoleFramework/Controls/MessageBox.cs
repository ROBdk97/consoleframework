using ConsoleFramework.Controls.Events;
using ConsoleFramework.Core;
using ConsoleFramework.Events;
using System;

namespace ConsoleFramework.Controls;

public class MessageBox : Window
{
    private readonly TextBlock textBlock;

    public MessageBox()
    {
        Panel panel = new();
        textBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(1)
        };
        Button button = new()
        {
            Margin = new Thickness(4, 0, 4, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            Caption = "OK"
        };
        button.OnClick += CloseButtonOnClicked;
        panel.Children.Add(textBlock);
        panel.Children.Add(button);
        panel.HorizontalAlignment = HorizontalAlignment.Center;
        panel.VerticalAlignment = VerticalAlignment.Bottom;
        Content = panel;
    }

    protected virtual void CloseButtonOnClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    public string Text
    {
        get { return textBlock.Text; }
        set { textBlock.Text = value; }
    }

    public static void Show(string title, string text, MessageBoxClosedEventHandler onClosed)
    {
        Control rootControl = ConsoleApplication.Instance.RootControl;
        if (rootControl is not WindowsHost)
            throw new InvalidOperationException("Default windows host not found, create MessageBox manually");
        WindowsHost windowsHost = (WindowsHost)rootControl;
        MessageBox messageBox = new()
        {
            Title = title,
            Text = text
        };
        messageBox.AddHandler(ClosedEvent, new EventHandler((sender, args) =>
        {
            onClosed?.Invoke(MessageBoxResult.Button1);
        }));
        //messageBox.X =
        windowsHost.ShowModal(messageBox);
    }
}

