using System;

namespace ConsoleFramework.Core;

public struct Rect : IFormattable
{
    internal int x;
    internal int y;
    internal int width;
    internal int height;
    private static readonly Rect s_empty;

    static Rect() => s_empty = CreateEmptyRect();

    public static bool operator ==(Rect r1, Rect r2) =>
        r1.X == r2.X && r1.Y == r2.Y && r1.Width == r2.Width && r1.Height == r2.Height;

    public static bool operator !=(Rect r1, Rect r2) => !(r1 == r2);

    public static bool Equals(Rect r1, Rect r2)
    {
        if (r1.IsEmpty) return r2.IsEmpty;
        return r1.X.Equals(r2.X) && r1.Y.Equals(r2.Y) && r1.Width.Equals(r2.Width) && r1.Height.Equals(r2.Height);
    }

    public override bool Equals(object? o) =>
        o is Rect rect && Equals(this, rect);

    public bool Equals(Rect value) => Equals(this, value);

    public override int GetHashCode() =>
        IsEmpty ? 0 : HashCode.Combine(X, Y, Width, Height);

    public override string ToString() => ConvertToString(null, null);
    public string ToString(IFormatProvider provider) => ConvertToString(null, provider);
    string IFormattable.ToString(string? format, IFormatProvider? provider) => ConvertToString(format, provider);

    internal string ConvertToString(string? format, IFormatProvider? provider)
    {
        if (IsEmpty) return "Empty";
        const char sep = ',';
        return string.Format(provider,
            "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}",
            sep, x, y, width, height);
    }

    public Rect(Rect copy) { x = copy.x; y = copy.y; width = copy.width; height = copy.height; }

    public Rect(Point location, Size size)
    {
        if (size.IsEmpty) { this = s_empty; return; }
        x = location.x; y = location.y; width = size.width; height = size.height;
    }

    public Rect(int x, int y, int width, int height)
    {
        if (width < 0 || height < 0) throw new ArgumentException("Size_WidthAndHeightCannotBeNegative");
        this.x = x; this.y = y; this.width = width; this.height = height;
    }

    public Rect(Point point1, Point point2)
    {
        x = Math.Min(point1.x, point2.x);
        y = Math.Min(point1.y, point2.y);
        width = Math.Max(Math.Max(point1.x, point2.x) - x, 0);
        height = Math.Max(Math.Max(point1.y, point2.y) - y, 0);
    }

    public Rect(Point point, Vector vector) : this(point, point + vector) { }

    public Rect(Size size)
    {
        if (size.IsEmpty) { this = s_empty; return; }
        x = y = 0; width = size.Width; height = size.Height;
    }

    public static Rect Empty => s_empty;
    public bool IsEmpty => width == 0 || height == 0;

    public Point Location
    {
        get => new(x, y);
        set
        {
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            x = value.x; y = value.y;
        }
    }

    public Size Size
    {
        get => IsEmpty ? Size.Empty : new(width, height);
        set
        {
            if (value.IsEmpty) { this = s_empty; return; }
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            width = value.width; height = value.height;
        }
    }

    public int X
    {
        get => x;
        set
        {
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            x = value;
        }
    }

    public int Y
    {
        get => y;
        set
        {
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            y = value;
        }
    }

    public int Width
    {
        get => width;
        set
        {
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            if (value < 0) throw new ArgumentException("Size_WidthCannotBeNegative");
            width = value;
        }
    }

    public int Height
    {
        get => height;
        set
        {
            if (IsEmpty) throw new InvalidOperationException("Rect_CannotModifyEmptyRect");
            if (value < 0) throw new ArgumentException("Size_HeightCannotBeNegative");
            height = value;
        }
    }

    public int Left => x;
    public int Top => y;
    public int Right => IsEmpty ? 0 : x + width;
    public int Bottom => IsEmpty ? 0 : y + height;
    public Point TopLeft => new(Left, Top);
    public Point TopRight => new(Right, Top);
    public Point BottomLeft => new(Left, Bottom);
    public Point BottomRight => new(Right, Bottom);

    public bool Contains(Point point) => Contains(point.x, point.y);

    public bool Contains(int _x, int _y) =>
        !IsEmpty && ContainsInternal(_x, _y);

    public bool Contains(Rect rect) =>
        !IsEmpty && !rect.IsEmpty &&
        x <= rect.x && y <= rect.y &&
        x + width >= rect.x + rect.width &&
        y + height >= rect.y + rect.height;

    public bool IntersectsWith(Rect rect) =>
        !IsEmpty && !rect.IsEmpty &&
        rect.Left <= Right && rect.Right >= Left &&
        rect.Top <= Bottom && rect.Bottom >= Top;

    public void Intersect(Rect rect)
    {
        if (!IntersectsWith(rect)) { this = Empty; return; }
        int left = Math.Max(Left, rect.Left);
        int top = Math.Max(Top, rect.Top);
        width = Math.Max(Math.Min(Right, rect.Right) - left, 0);
        height = Math.Max(Math.Min(Bottom, rect.Bottom) - top, 0);
        x = left; y = top;
    }

    public static Rect Intersect(Rect rect1, Rect rect2) { rect1.Intersect(rect2); return rect1; }

    public void Union(Rect rect)
    {
        if (IsEmpty) { this = rect; return; }
        if (rect.IsEmpty) return;
        int left = Math.Min(Left, rect.Left);
        int top = Math.Min(Top, rect.Top);
        width = rect.Width == int.MaxValue || Width == int.MaxValue
            ? int.MaxValue
            : Math.Max(Math.Max(Right, rect.Right) - left, 0);
        height = rect.Height == int.MaxValue || Height == int.MaxValue
            ? int.MaxValue
            : Math.Max(Math.Max(Bottom, rect.Bottom) - top, 0);
        x = left; y = top;
    }

    public static Rect Union(Rect rect1, Rect rect2) { rect1.Union(rect2); return rect1; }
    public void Union(Point point) => Union(new Rect(point, point));
    public static Rect Union(Rect rect, Point point) { rect.Union(new Rect(point, point)); return rect; }

    public void Offset(Vector offsetVector)
    {
        if (IsEmpty) throw new InvalidOperationException("Rect_CannotCallMethod");
        x += offsetVector.x; y += offsetVector.y;
    }

    public void Offset(int offsetX, int offsetY)
    {
        if (IsEmpty) throw new InvalidOperationException("Rect_CannotCallMethod");
        x += offsetX; y += offsetY;
    }

    public static Rect Offset(Rect rect, Vector offsetVector) { rect.Offset(offsetVector.X, offsetVector.Y); return rect; }
    public static Rect Offset(Rect rect, int offsetX, int offsetY) { rect.Offset(offsetX, offsetY); return rect; }

    public void Inflate(Size size) => Inflate(size.width, size.height);

    public void Inflate(int _width, int _height)
    {
        if (IsEmpty) throw new InvalidOperationException("Rect_CannotCallMethod");
        x -= _width; y -= _height;
        width += _width * 2;
        height += _height * 2;
        if (width < 0 || height < 0) this = s_empty;
    }

    public static Rect Inflate(Rect rect, Size size) { rect.Inflate(size.width, size.height); return rect; }
    public static Rect Inflate(Rect rect, int width, int height) { rect.Inflate(width, height); return rect; }

    private bool ContainsInternal(int _x, int _y) =>
        _x >= x && _x - width < x && _y >= y && _y - height < y;

    private static Rect CreateEmptyRect() => new() { x = 0, y = 0, width = 0, height = 0 };
}
