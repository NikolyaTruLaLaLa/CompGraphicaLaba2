using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.Core.Models
{
    public sealed class Matrix4x4
    {
        private readonly float[,] _m = new float[4, 4];

        public Matrix4x4() { }

        public Matrix4x4(float[,] values)
        {
            if (values.GetLength(0) != 4 || values.GetLength(1) != 4)
                throw new ArgumentException("Матрица должна быть 4x4");
            Array.Copy(values, _m, 16);
        }

        public Matrix4x4(Matrix4x4 other) => Array.Copy(other._m, _m, 16);

        public float this[int row, int col] => _m[row, col];

        private void SetRow(int r, float a, float b, float c, float d)
        { _m[r, 0] = a; _m[r, 1] = b; _m[r, 2] = c; _m[r, 3] = d; }


        public static Matrix4x4 operator *(Matrix4x4 a, Matrix4x4 b)
        {
            var r = new Matrix4x4();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++)
                        sum += a._m[i, k] * b._m[k, j];
                    r._m[i, j] = sum;
                }
            return r;
        }

        public static Point3 operator *(Matrix4x4 m, Point3 p)
        {
            float x = m._m[0, 0] * p.X + m._m[0, 1] * p.Y + m._m[0, 2] * p.Z + m._m[0, 3];
            float y = m._m[1, 0] * p.X + m._m[1, 1] * p.Y + m._m[1, 2] * p.Z + m._m[1, 3];
            float z = m._m[2, 0] * p.X + m._m[2, 1] * p.Y + m._m[2, 2] * p.Z + m._m[2, 3];
            float w = m._m[3, 0] * p.X + m._m[3, 1] * p.Y + m._m[3, 2] * p.Z + m._m[3, 3];
            if (MathF.Abs(w) > 1e-8f && MathF.Abs(w - 1f) > 1e-6f)
                return new Point3(x / w, y / w, z / w);
            return new Point3(x, y, z);
        }
        public static Matrix4x4 Identity
        {
            get
            {
                var m = new Matrix4x4();
                m.SetRow(0, 1, 0, 0, 0);
                m.SetRow(1, 0, 1, 0, 0);
                m.SetRow(2, 0, 0, 1, 0);
                m.SetRow(3, 0, 0, 0, 1);
                return m;
            }
        }

        public static Matrix4x4 Translation(float dx, float dy, float dz)
        {
            var m = Identity;
            m._m[0, 3] = dx;
            m._m[1, 3] = dy;
            m._m[2, 3] = dz;
            return m;
        }
        public static Matrix4x4 Translation(Point3 t) => Translation(t.X, t.Y, t.Z);

        public static Matrix4x4 Scale(float sx, float sy, float sz)
        {
            var m = Identity;
            m._m[0, 0] = sx;
            m._m[1, 1] = sy;
            m._m[2, 2] = sz;
            return m;
        }
        public static Matrix4x4 Scale(float s) => Scale(s, s, s);

        public static Matrix4x4 RotationX(float r)
        {
            float c = MathF.Cos(r), s = MathF.Sin(r);
            var m = Identity;
            m._m[1, 1] = c; m._m[1, 2] = -s;
            m._m[2, 1] = s; m._m[2, 2] = c;
            return m;
        }
        public static Matrix4x4 RotationY(float r)
        {
            float c = MathF.Cos(r), s = MathF.Sin(r);
            var m = Identity;
            m._m[0, 0] = c; m._m[0, 2] = s;
            m._m[2, 0] = -s; m._m[2, 2] = c;
            return m;
        }
        public static Matrix4x4 RotationZ(float r)
        {
            float c = MathF.Cos(r), s = MathF.Sin(r);
            var m = Identity;
            m._m[0, 0] = c; m._m[0, 1] = -s;
            m._m[1, 0] = s; m._m[1, 1] = c;
            return m;
        }

        /// <summary>Формула Родрига — поворот вокруг оси, проходящей через начало координат.</summary>
        public static Matrix4x4 RotationAxis(Point3 axis, float radians)
        {
            var n = axis.Normalized();
            float x = n.X, y = n.Y, z = n.Z;
            float c = MathF.Cos(radians), s = MathF.Sin(radians);
            float t = 1f - c;
            var m = new Matrix4x4();
            m.SetRow(0, t * x * x + c, t * x * y - s * z, t * x * z + s * y, 0);
            m.SetRow(1, t * x * y + s * z, t * y * y + c, t * y * z - s * x, 0);
            m.SetRow(2, t * x * z - s * y, t * y * z + s * x, t * z * z + c, 0);
            m.SetRow(3, 0, 0, 0, 1);
            return m;
        }

        public static Matrix4x4 ReflectionXY() { var m = Identity; m._m[2, 2] = -1; return m; }
        public static Matrix4x4 ReflectionXZ() { var m = Identity; m._m[1, 1] = -1; return m; }
        public static Matrix4x4 ReflectionYZ() { var m = Identity; m._m[0, 0] = -1; return m; }
        public static Matrix4x4 LookAt(Point3 eye, Point3 target, Point3 up)
        {
            Point3 zAxis = (eye - target).Normalized();
            Point3 xAxis = Point3.Cross(up, zAxis).Normalized();
            Point3 yAxis = Point3.Cross(zAxis, xAxis);

            var m = Identity;

            m._m[0, 0] = xAxis.X; m._m[0, 1] = xAxis.Y; m._m[0, 2] = xAxis.Z; m._m[0, 3] = -Point3.Dot(xAxis, eye);
            m._m[1, 0] = yAxis.X; m._m[1, 1] = yAxis.Y; m._m[1, 2] = yAxis.Z; m._m[1, 3] = -Point3.Dot(yAxis, eye);
            m._m[2, 0] = zAxis.X; m._m[2, 1] = zAxis.Y; m._m[2, 2] = zAxis.Z; m._m[2, 3] = -Point3.Dot(zAxis, eye);
            m._m[3, 0] = 0; m._m[3, 1] = 0; m._m[3, 2] = 0; m._m[3, 3] = 1;

            return m;
        }
        public static Matrix4x4 Perspective(float fovRadians, float aspect, float near, float far)
        {
            float f = 1.0f / MathF.Tan(fovRadians / 2.0f);
            var m = new Matrix4x4();

            m._m[0, 0] = f / aspect;
            m._m[1, 1] = f;
            m._m[2, 2] = (far + near) / (near - far);
            m._m[2, 3] = (2.0f * far * near) / (near - far);
            m._m[3, 2] = -1.0f;
            m._m[3, 3] = 0.0f;

            return m;
        }
    }
}
