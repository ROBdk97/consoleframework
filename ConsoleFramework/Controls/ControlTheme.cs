using ConsoleFramework.Core;
using ConsoleFramework.Native;

namespace ConsoleFramework.Controls;

internal static class ControlTheme
{
    internal static readonly Attr WindowBorderInactive = Colors.Blend(Color.DarkGray, Color.Black);
    internal static readonly Attr WindowBorderActive = Colors.Blend(Color.White, Color.DarkGray);
    internal static readonly Attr WindowBorderInteraction = Colors.Blend(Color.White, Color.DarkCyan);
    internal static readonly Attr WindowCloseButton = Colors.Blend(Color.Cyan, Color.Black);

    internal static readonly Attr ButtonNormal = Colors.Blend(Color.White, Color.DarkGray);
    internal static readonly Attr ButtonFocused = Colors.Blend(Color.White, Color.DarkCyan);
    internal static readonly Attr ButtonDisabled = Colors.Blend(Color.DarkGray, Color.Black);

    internal static readonly Attr InputNormal = Colors.Blend(Color.Gray, Color.Black);
    internal static readonly Attr InputFocused = Colors.Blend(Color.White, Color.DarkGray);
    internal static readonly Attr InputIndicator = Colors.Blend(Color.Cyan, Color.Black);
    internal static readonly Attr InputIndicatorFocused = Colors.Blend(Color.Cyan, Color.DarkGray);

    internal static readonly Attr ListNormal = Colors.Blend(Color.Gray, Color.Black);
    internal static readonly Attr ListSelection = Colors.Blend(Color.White, Color.DarkCyan);
    internal static readonly Attr ListDisabled = Colors.Blend(Color.DarkGray, Color.Black);

    internal static readonly Attr ToggleNormal = Colors.Blend(Color.Gray, Color.Black);
    internal static readonly Attr ToggleFocused = Colors.Blend(Color.White, Color.DarkCyan);

    internal static readonly Attr GroupBorder = Colors.Blend(Color.DarkGray, Color.Black);
    internal static readonly Attr PopupBackground = Colors.Blend(Color.Gray, Color.Black);
}
