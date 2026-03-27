using System;

namespace ConsoleFramework.Core;

public struct Vector
{
    internal int x;
    internal int y;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public double Length => Math.Sqrt(x * x + y * y);
    public double LengthSquared => x * x + y * y;

    public Vector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Vector v1, Vector v2) => v1.X == v2.X && v1.Y == v2.Y;
    public static bool operator !=(Vector v1, Vector v2) => !(v1 == v2);

    public static bool Equals(Vector v1, Vector v2) => v1.X.Equals(v2.X) && v1.Y.Equals(v2.Y);

    public override bool Equals(object? o) =>
        o is Vector vector && Equals(this, vector);

    public bool Equals(Vector value) => Equals(this, value);

    public override int GetHashCode() => HashCode.Combine(X, Y);

    public void Negate()
    {
        x = -x;
        y = -y;
    }

    public static double CrossProduct(Vector v1, Vector v2) => v1.x * v2.y - v1.y * v2.x;
    public static double AngleBetween(Vector v1, Vector v2) =>
        Math.Atan2(v1.x * v2.y - v2.x * v1.y, v1.x * v2.x + v1.y * v2.y) * 57.295779513082323;
    public static double Determinant(Vector v1, Vector v2) => v1.x * v2.y - v1.y * v2.x;

    public static Vector operator -(Vector v) => new(-v.x, -v.y);
    public static Vector operator +(Vector v1, Vector v2) => new(v1.x + v2.x, v1.y + v2.y);
    public static Vector Add(Vector v1, Vector v2) => new(v1.x + v2.x, v1.y + v2.y);
    public static Vector operator -(Vector v1, Vector v2) => new(v1.x - v2.x, v1.y - v2.y);
    public static Vector Subtract(Vector v1, Vector v2) => new(v1.x - v2.x, v1.y - v2.y);
    public static Point operator +(Vector vector, Point point) => new(point.x + vector.x, point.y + vector.y);
    public static Point Add(Vector vector, Point point) => new(point.x + vector.x, point.y + vector.y);
    public static Vector operator *(Vector vector, int scalar) => new(vector.x * scalar, vector.y * scalar);
    public static Vector Multiply(Vector vector, int scalar) => new(vector.x * scalar, vector.y * scalar);
    public static Vector operator *(int scalar, Vector vector) => new(vector.x * scalar, vector.y * scalar);
    public static Vector Multiply(int scalar, Vector vector) => new(vector.x * scalar, vector.y * scalar);
    public static double operator *(Vector v1, Vector v2) => v1.x * v2.x + v1.y * v2.y;
    public static double Multiply(Vector v1, Vector v2) => v1.x * v2.x + v1.y * v2.y;
    public static explicit operator Point(Vector vector) => new(vector.x, vector.y);

    public override string ToString() => $"{x};{y}";
}