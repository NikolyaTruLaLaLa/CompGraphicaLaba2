using LABA_6.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.Core.Factory
{
    public static class TransformFactory
    {
        public static Matrix4x4 Translation(float dx, float dy, float dz)
            => Matrix4x4.Translation(dx, dy, dz);

        public static Matrix4x4 RotationX(float radians) => Matrix4x4.RotationX(radians);
        public static Matrix4x4 RotationY(float radians) => Matrix4x4.RotationY(radians);
        public static Matrix4x4 RotationZ(float radians) => Matrix4x4.RotationZ(radians);

        public static Matrix4x4 Scale(float sx, float sy, float sz)
            => Matrix4x4.Scale(sx, sy, sz);

        public enum CoordinatePlane { XY, XZ, YZ }

        public static Matrix4x4 Reflection(CoordinatePlane plane) => plane switch
        {
            CoordinatePlane.XY => Matrix4x4.ReflectionXY(),
            CoordinatePlane.XZ => Matrix4x4.ReflectionXZ(),
            CoordinatePlane.YZ => Matrix4x4.ReflectionYZ(),
            _ => throw new ArgumentOutOfRangeException(nameof(plane)),
        };

        public static Matrix4x4 ScaleAboutPoint(Point3 point, float sx, float sy, float sz)
            => Matrix4x4.Translation(point)
             * Matrix4x4.Scale(sx, sy, sz)
             * Matrix4x4.Translation(-point);

        public static Matrix4x4 ScaleAboutCenter(Mesh mesh, float sx, float sy, float sz)
            => ScaleAboutPoint(mesh.Center, sx, sy, sz);

        public static Matrix4x4 RotationAboutAxisThroughPoint(Point3 point, Point3 axis, float radians)
            => Matrix4x4.Translation(point)
             * Matrix4x4.RotationAxis(axis, radians)
             * Matrix4x4.Translation(-point);

        public enum CoordinateAxis { X, Y, Z }

        public static Matrix4x4 RotationAboutCenterParallelToAxis(
            Mesh mesh, CoordinateAxis axis, float radians)
        {
            var dir = axis switch
            {
                CoordinateAxis.X => Point3.UnitX,
                CoordinateAxis.Y => Point3.UnitY,
                CoordinateAxis.Z => Point3.UnitZ,
                _ => throw new ArgumentOutOfRangeException(nameof(axis)),
            };
            return RotationAboutAxisThroughPoint(mesh.Center, dir, radians);
        }

        public static Matrix4x4 RotationAboutLine(Point3 p1, Point3 p2, float radians)
        {
            var axis = p2 - p1;
            if (axis.Length() < 1e-8f)
                throw new ArgumentException("Точки прямой совпадают");
            return RotationAboutAxisThroughPoint(p1, axis, radians);
        }

        public static float DegToRad(float degrees) => degrees * MathF.PI / 180f;
    }

}
