using ConsoleFramework.Core;
using ConsoleFramework.Events;
using ConsoleFramework.Native;
using ConsoleFramework.Rendering;
using System;

namespace ConsoleFramework.Controls;

/// <summary>
/// todo : add text selection handling
/// </summary>
public class TextBox : Control
{
    public TextBox()
    {
        KeyDown += TextBox_KeyDown;
        MouseDown += OnMouseDown;
        CursorVisible = true;
        CursorPosition = new Point(1, 0);
        Focusable = true;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs args)
    {
        Point point = args.GetPosition(this);
        if (point.X > 0 && point.X - 1 < getSize())
        {
            int x = point.X - 1;
            if (!string.IsNullOrEmpty(text))
            {
                if (x <= text.Length)
                    cursorPosition = x;
                else
                {
                    cursorPosition = text.Length;
                }
                CursorPosition = new Point(cursorPosition + 1, 0);
            }
            args.Handled = true;
        }
    }

    private void TextBox_KeyDown(object sender, KeyEventArgs args)
    {
        ConsoleKeyInfo keyInfo = new(args.UnicodeChar,
            (ConsoleKey)args.wVirtualKeyCode,
            (args.dwControlKeyState & ControlKeyState.SHIFT_PRESSED) == ControlKeyState.SHIFT_PRESSED,
            (args.dwControlKeyState & ControlKeyState.LEFT_ALT_PRESSED) == ControlKeyState.LEFT_ALT_PRESSED
                || (args.dwControlKeyState & ControlKeyState.RIGHT_ALT_PRESSED) == ControlKeyState.RIGHT_ALT_PRESSED,
            (args.dwControlKeyState & ControlKeyState.LEFT_CTRL_PRESSED) == ControlKeyState.LEFT_CTRL_PRESSED
                || (args.dwControlKeyState & ControlKeyState.RIGHT_CTRL_PRESSED) == ControlKeyState.RIGHT_CTRL_PRESSED
        );
        if (!char.IsControl(keyInfo.KeyChar))
        {
            // insert keychar into a text string according to cursorPosition and offset
            if (text != null)
            {
                string leftPart = text[..(cursorPosition + displayOffset)];
                string rightPart = text[(cursorPosition + displayOffset)..];
                Text = leftPart + keyInfo.KeyChar + rightPart;
            }
            else
            {
                Text = keyInfo.KeyChar.ToString();
            }
            if (cursorPosition + 1 < ActualWidth - 2)
            {
                cursorPosition++;
                CursorPosition = new Point(cursorPosition + 1, 0);
            }
            else
            {
                displayOffset++;
            }
        }
        else
        {
            if (keyInfo.Key == ConsoleKey.Delete)
            {
                if (!string.IsNullOrEmpty(text) && displayOffset + cursorPosition < text.Length)
                {
                    string leftPart = text[..(cursorPosition + displayOffset)];
                    string rightPart = text[(cursorPosition + displayOffset + 1)..];
                    Text = leftPart + rightPart;
                    //
                }
                else
                {
                    Console.Beep();
                }
            }
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (!string.IsNullOrEmpty(text) && (displayOffset != 0 || cursorPosition != 0))
                {
                    string leftPart = text[..(cursorPosition + displayOffset - 1)];
                    string rightPart = text[(cursorPosition + displayOffset)..];
                    Text = leftPart + rightPart;
                    if (displayOffset > 0)
                        displayOffset--;
                    else
                    {
                        if (cursorPosition > 0)
                        {
                            cursorPosition--;
                            CursorPosition = new Point(cursorPosition + 1, 0);
                        }
                    }
                }
                else
                {
                    Console.Beep();
                }
            }
            if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    // todo :
                }
                if (!string.IsNullOrEmpty(text) && (displayOffset != 0 || cursorPosition != 0))
                {
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        CursorPosition = new Point(cursorPosition + 1, 0);
                    }
                    else
                    {
                        if (displayOffset > 0)
                        {
                            displayOffset--;
                            Invalidate();
                        }
                    }
                }
                else
                {
                    Console.Beep();
                }
            }
            if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (!string.IsNullOrEmpty(text) && displayOffset + cursorPosition < text.Length)
                {
                    if (cursorPosition + 1 < ActualWidth - 2)
                    {
                        cursorPosition++;
                        CursorPosition = new Point(cursorPosition + 1, 0);
                    }
                    else
                    {
                        if (displayOffset + cursorPosition < text.Length)
                        {
                            displayOffset++;
                            Invalidate();
                        }
                    }
                }
                else
                {
                    Console.Beep();
                }
            }
            if (keyInfo.Key == ConsoleKey.Home)
            {
                if (displayOffset != 0 || cursorPosition != 0)
                {
                    displayOffset = 0;
                    cursorPosition = 0;
                    CursorPosition = new Point(cursorPosition + 1, 0);
                    Invalidate();
                }
                else
                {
                    Console.Beep();
                }
            }
            if (keyInfo.Key == ConsoleKey.End)
            {
                if (!string.IsNullOrEmpty(text) && cursorPosition + displayOffset < ActualWidth - 2)
                {
                    displayOffset = text.Length >= ActualWidth - 2 ? text.Length - (ActualWidth - 2) + 1 : 0;
                    cursorPosition = text.Length >= ActualWidth - 2 ? ActualWidth - 2 - 1 : text.Length;
                    CursorPosition = new Point(cursorPosition + 1, 0);
                    Invalidate();
                }
                else
                {
                    Console.Beep();
                }
            }
        }
        //Debugger.Log(0, "", String.Format("cursorPos : {0} offset {1}\n", cursorPosition, displayOffset));
    }

    private string text;

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
                RaisePropertyChanged(nameof(Text));
            }
        }
    }

    public int MaxLength
    {
        get;
        set;
    }

    public int? Size
    {
        get;
        set;
    }

    public char? PasswordChar { get; set; }

    private int getSize()
    {
        if (Size.HasValue) return Size.Value;
        return text != null ? text.Length + 1 : 1;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Size desired = new(getSize() + 2, 1);
        return new Size(
            Math.Min(desired.Width, availableSize.Width),
            Math.Min(desired.Height, availableSize.Height)
        );
    }

    // this fields describe the whole state of textbox
    private int displayOffset;
    private int cursorPosition;

    public override void Render(RenderingBuffer buffer)
    {
        Attr attr = HasFocus ? ControlTheme.InputFocused : ControlTheme.InputNormal;
        buffer.FillRectangle(0, 0, ActualWidth, ActualHeight, ' ', attr);
        if (null != text)
        {
            for (int i = displayOffset; i < text.Length; i++)
            {
                if (i - displayOffset < ActualWidth - 2 && i - displayOffset >= 0)
                {
                    buffer.SetPixel(1 + i - displayOffset, 0, PasswordChar ?? text[i]);
                }
            }
        }
        Attr arrowsAttr = HasFocus ? ControlTheme.InputIndicatorFocused : ControlTheme.InputIndicator;
        if (displayOffset > 0)
            buffer.SetPixel(0, 0, '<', arrowsAttr);
        if (!string.IsNullOrEmpty(text) && ActualWidth - 2 + displayOffset < text.Length)
            buffer.SetPixel(ActualWidth - 1, 0, '>', arrowsAttr);
    }
}