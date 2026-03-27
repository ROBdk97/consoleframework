using ConsoleFramework.Native;
using ConsoleFramework.Xaml;
using System;

namespace ConsoleFramework.Events;

[TypeConverter(typeof(KeyGestureConverter))]
public class KeyGesture
{
    private readonly string _displayString;
    private readonly VirtualKeys _key;
    private static readonly KeyGestureConverter _keyGestureConverter = new();
    private readonly ModifierKeys _modifiers;

    public KeyGesture(VirtualKeys key) : this(key, ModifierKeys.None)
    {
    }

    public KeyGesture(VirtualKeys key, ModifierKeys modifiers) : this(key, modifiers, string.Empty)
    {
    }

    public KeyGesture(VirtualKeys key, ModifierKeys modifiers, string displayString)
    {
        ArgumentNullException.ThrowIfNull(displayString);
        if (!IsValid(key, modifiers))
            throw new InvalidOperationException("KeyGesture is invalid");
        _modifiers = modifiers;
        _key = key;
        _displayString = displayString;
    }

    public string GetDisplayString()
    {
        if (!string.IsNullOrEmpty(_displayString))
        {
            return _displayString;
        }
        return (string)_keyGestureConverter.ConvertTo(this, typeof(string));
    }

    // todo : check incompatible combinations
    internal static bool IsValid(VirtualKeys key, ModifierKeys modifiers)
    {
        if (!Enum.IsDefined(key))
        {
            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(key), (int)key, typeof(VirtualKeys));
        }

        return true;
    }

    public bool Matches(KeyEventArgs args)
    {
        VirtualKeys wVirtualKeyCode = args.wVirtualKeyCode;
        if (Key != wVirtualKeyCode) return false;
        ControlKeyState controlKeyState = args.dwControlKeyState;
        ModifierKeys modifierKeys = Modifiers;

        // Check all possible modifiers in sequence

        if ((modifierKeys & ModifierKeys.Alt) != 0)
        {
            if ((controlKeyState & (ControlKeyState.LEFT_ALT_PRESSED
                                       | ControlKeyState.RIGHT_ALT_PRESSED)) == 0)
            {
                // One of the flags indicating Alt press should be set, but it's not
                return false;
            }
        }
        else
        {
            if ((controlKeyState & (ControlKeyState.LEFT_ALT_PRESSED
                                       | ControlKeyState.RIGHT_ALT_PRESSED)) != 0)
            {
                // None of the flags indicating Alt press should be set,
                // but the flag is actually set
                return false;
            }
        }

        if ((modifierKeys & ModifierKeys.Control) != 0)
        {
            if ((controlKeyState & (ControlKeyState.LEFT_CTRL_PRESSED
                                       | ControlKeyState.RIGHT_CTRL_PRESSED)) == 0)
            {
                return false;
            }
        }
        else
        {
            if ((controlKeyState & (ControlKeyState.LEFT_CTRL_PRESSED
                                       | ControlKeyState.RIGHT_CTRL_PRESSED)) != 0)
            {
                return false;
            }
        }

        if ((modifierKeys & ModifierKeys.Shift) != 0)
        {
            if ((controlKeyState & (ControlKeyState.SHIFT_PRESSED)) == 0)
            {
                return false;
            }
        }
        else
        {
            if ((controlKeyState & (ControlKeyState.SHIFT_PRESSED)) != 0)
            {
                return false;
            }
        }

        return true;
    }

    public string DisplayString
    {
        get { return _displayString; }
    }

    public VirtualKeys Key
    {
        get { return _key; }
    }

    public ModifierKeys Modifiers
    {
        get { return _modifiers; }
    }
}