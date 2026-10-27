using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LABA_6.Core.Models
{
    public sealed class Mesh
    {
        public List<Point3> Vertices { get; }
        public List<Face> Faces { get; }

        public Mesh() { Vertices = new(); Faces = new(); }
        public Mesh(IEnumerable<Point3> vertices, IEnumerable<Face> faces)
        {
            Vertices = new List<Point3>(vertices);
            Faces = new List<Face>(faces);
        }

        public int VertexCount => Vertices.Count;
        public int FaceCount => Faces.Count;

        public Point3 Center
        {
            get
            {
                if (Vertices.Count == 0) return Point3.Zero;
                Point3 sum = Point3.Zero;
                foreach (var v in Vertices) sum += v;
                return sum / Vertices.Count;
            }
        }

        public Mesh ApplyTransform(Matrix4x4 m)
        {
            for (int i = 0; i < Vertices.Count; i++)
                Vertices[i] = m * Vertices[i];
            return this;
        }

        public Mesh Clone()
        {
            var copy = new Mesh();
            copy.Vertices.AddRange(Vertices);
            foreach (var f in Faces) copy.Faces.Add(new Face(f.Indices));
            return copy;
        }

        public Point3 FaceNormal(Face f) => f.Normal(Vertices);
        public Point3 FaceCentroid(Face f) => f.Centroid(Vertices);
    }
}
