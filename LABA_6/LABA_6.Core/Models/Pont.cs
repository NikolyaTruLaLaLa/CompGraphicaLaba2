using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.Core.Models
{

    public struct Point3 : IEquatable<Point3>
    {
        public float X;
        public float Y;
        public float Z;

        public Point3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public static readonly Point3 Zero = new(0, 0, 0);
        public static readonly Point3 UnitX = new(1, 0, 0);
        public static readonly Point3 UnitY = new(0, 1, 0);
        public static readonly Point3 UnitZ = new(0, 0, 1);

        public static Point3 operator +(Point3 a, Point3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3 operator -(Point3 a, Point3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Point3 operator -(Point3 a) => new(-a.X, -a.Y, -a.Z);
        public static Point3 operator *(Point3 a, float s) => new(a.X * s, a.Y * s, a.Z * s);
        public static Point3 operator *(float s, Point3 a) => a * s;
        public static Point3 operator /(Point3 a, float s) => new(a.X / s, a.Y / s, a.Z / s);

        public static float Dot(Point3 a, Point3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Point3 Cross(Point3 a, Point3 b) => new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        public float Length() => MathF.Sqrt(LengthSquared());
        public float LengthSquared() => X * X + Y * Y + Z * Z;

        public Point3 Normalized()
        {
            float len = Length();
            return len > 1e-8f ? this / len : Zero;
        }

        public void Normalize() => this = Normalized();

        public bool Equals(Point3 other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object? obj) => obj is Point3 p && Equals(p);
        public override int GetHashCode() => HashCode.Combine(X, Y, Z);
        public override string ToString() => $"({X:F3}, {Y:F3}, {Z:F3})";
    }
}
