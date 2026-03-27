using ConsoleFramework.Native;

namespace ConsoleFramework.Core;

public static class Colors
{
    /// <summary>Blends foreground and background colors into one char attributes code.</summary>
    public static Attr Blend(Color foreground, Color background) =>
        (Attr)((ushort)foreground + ((ushort)background << 4));
}
