using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.Core.Models
{
    public class Camera
    {
        public Point3 Position { get; set; }
        public Point3 Target { get; set; }
        public Point3 Up { get; set; }

        public float FovDegrees { get; set; } = 45f;
        public float NearPlane { get; set; } = 0.1f;
        public float FarPlane { get; set; } = 100f;

        public Camera(Point3 position, Point3 target, Point3 up)
        {
            Position = position;
            Target = target;
            Up = up;
        }

        public Matrix4x4 GetViewMatrix()
        {
            return Matrix4x4.LookAt(Position, Target, Up);
        }

        public Matrix4x4 GetProjectionMatrix(float aspect)
        {
            return Matrix4x4.Perspective(FovDegrees * MathF.PI / 180f, aspect, NearPlane, FarPlane);
        }
    }
}
