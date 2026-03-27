using System;
using Xaml;

namespace ConsoleFramework.Core;

/// <summary>
/// WPF Thickness analog but using integers instead doubles.
/// </summary>
[TypeConverter(typeof(ThicknessConverter))]
public struct Thickness : IEquatable<Thickness>
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public Thickness(int uniformLength)
    {
        Left = Top = Right = Bottom = uniformLength;
    }

    public Thickness(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    internal bool IsZero() => Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
    internal bool IsUniform() => Left == Top && Left == Right && Left == Bottom;

    public static bool operator ==(Thickness t1, Thickness t2) =>
        t1.Left == t2.Left && t1.Top == t2.Top && t1.Right == t2.Right && t1.Bottom == t2.Bottom;

    public static bool operator !=(Thickness t1, Thickness t2) => !(t1 == t2);

    public bool Equals(Thickness other) =>
        other.Left == Left && other.Top == Top && other.Right == Right && other.Bottom == Bottom;

    public override bool Equals(object? obj) =>
        obj is Thickness thickness && Equals(thickness);

    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    public override string ToString() => $"{Left},{Top},{Right},{Bottom}";
}
