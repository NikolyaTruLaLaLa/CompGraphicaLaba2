using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.Core.Models
{
    using System;

    public sealed class Face
    {
        public int[] Indices { get; }
        public int[]? TexIndices { get; }
        public int[]? NormalIndices { get; }

        // Конструктор для обычных граней (как было раньше)
        public Face(params int[] indices)
        {
            if (indices == null || indices.Length < 3)
                throw new ArgumentException("Грань должна содержать минимум 3 вершины");
            Indices = (int[])indices.Clone();
            TexIndices = null;
        }

        // Конструктор для граней с текстурами
        public Face(int[] indices, int[]? texIndices, int[]? normalIndices = null)
        {
            if (indices == null || indices.Length < 3)
                throw new ArgumentException("Грань должна содержать минимум 3 вершины");
            Indices = (int[])indices.Clone();
            TexIndices = texIndices != null ? (int[])texIndices.Clone() : null;
            NormalIndices = normalIndices != null ? (int[])normalIndices.Clone() : null;
        }

        public int Count => Indices.Length;

        public Point3 Centroid(IReadOnlyList<Point3> v)
        {
            Point3 sum = Point3.Zero;
            foreach (var i in Indices) sum += v[i];
            return sum / Count;
        }

        public Point3 Normal(IReadOnlyList<Point3> v)
        {
            var e1 = v[Indices[1]] - v[Indices[0]];
            var e2 = v[Indices[2]] - v[Indices[0]];
            return Point3.Cross(e1, e2).Normalized();
        }
    }
}
