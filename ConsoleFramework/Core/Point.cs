using System;

namespace ConsoleFramework.Core;

public struct Point(int x, int y)
{
    internal int x = x;
    internal int y = y;

    public static bool operator ==(Point point1, Point point2) =>
        point1.X == point2.X && point1.Y == point2.Y;

    public static bool operator !=(Point point1, Point point2) => !(point1 == point2);

    public static bool Equals(Point point1, Point point2) =>
        point1.X.Equals(point2.X) && point1.Y.Equals(point2.Y);

    public override bool Equals(object? o) =>
        o is Point point && Equals(this, point);

    public bool Equals(Point value) => Equals(this, value);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public void Offset(int offsetX, int offsetY)
    {
        x += offsetX;
        y += offsetY;
    }

    public static Point operator +(Point point, Vector vector) => new(point.x + vector.x, point.y + vector.y);
    public static Point Add(Point point, Vector vector) => new(point.x + vector.x, point.y + vector.y);
    public static Point operator -(Point point, Vector vector) => new(point.x - vector.x, point.y - vector.y);
    public static Point Subtract(Point point, Vector vector) => new(point.x - vector.x, point.y - vector.y);
    public static Vector operator -(Point point1, Point point2) => new(point1.x - point2.x, point1.y - point2.y);
    public static Vector Subtract(Point point1, Point point2) => new(point1.x - point2.x, point1.y - point2.y);
    public static explicit operator Vector(Point point) => new(point.x, point.y);

    public override string ToString() => $"{x};{y}";
}