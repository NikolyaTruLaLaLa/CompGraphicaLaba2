using System;
using System.Collections.Generic;
using LABA_6.Core.Models;

namespace LABA_6.Core.Factory
{
    public static class RevolutionFactory
    {
        public static Mesh Create(List<Point3> profile, int divisions, TransformFactory.CoordinateAxis axis)
        {
            if (profile == null || profile.Count < 2)
                throw new ArgumentException("Образующая должна содержать минимум 2 точки.");
            if (divisions < 3)
                throw new ArgumentException("Количество разбиений должно быть не менее 3.");

            var vertices = new List<Point3>();
            var faces = new List<Face>();

            int profileCount = profile.Count;

            for (int i = 0; i < divisions; i++)
            {
                float angle = (float)(2 * Math.PI * i / divisions);
                var rotationMatrix = Matrix4x4.RotationY(angle);

                foreach (var p in profile)
                {
                    vertices.Add(rotationMatrix * p);
                }
            }

            for (int i = 0; i < divisions; i++)
            {
                int next_i = (i + 1) % divisions; 

                for (int j = 0; j < profileCount - 1; j++)
                {
                    int p0 = i * profileCount + j;
                    int p1 = i * profileCount + j + 1;
                    int p2 = next_i * profileCount + j + 1;
                    int p3 = next_i * profileCount + j;

                    faces.Add(new Face(p0, p1, p2, p3));
                }
            }

            var mesh = new Mesh(vertices, faces);

            if (axis == TransformFactory.CoordinateAxis.X)
            {
                mesh.ApplyTransform(Matrix4x4.RotationZ((float)(Math.PI / 2)));
            }
            else if (axis == TransformFactory.CoordinateAxis.Z)
            {
                mesh.ApplyTransform(Matrix4x4.RotationX((float)(-Math.PI / 2)));
            }

            return mesh;
        }
    }
}