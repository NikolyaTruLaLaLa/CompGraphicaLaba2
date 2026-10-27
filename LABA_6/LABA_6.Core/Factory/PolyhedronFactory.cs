using LABA_6.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LABA_6.UI.Factory
{
    public static class PolyhedronFactory
    {
        public static Mesh CreateTetrahedron()
        {
            float s = 1f / MathF.Sqrt(3f);
            var v = new List<Point3>
        {
            new( s,  s,  s),
            new( s, -s, -s),
            new(-s,  s, -s),
            new(-s, -s,  s),
        };
            var f = new List<Face>
        {
            new(0, 1, 2), new(0, 3, 1), new(0, 2, 3), new(1, 3, 2),
        };
            return new Mesh(v, f);
        }

        public static Mesh CreateCube()
        {
            float s = 1f / MathF.Sqrt(3f); 
            var v = new List<Point3>
        {
            new(-s, -s, -s), new( s, -s, -s), new( s,  s, -s), new(-s,  s, -s),
            new(-s, -s,  s), new( s, -s,  s), new( s,  s,  s), new(-s,  s,  s),
        };
            var f = new List<Face>
        {
            new(0, 3, 2, 1), // z = -1
            new(4, 5, 6, 7), // z = +1
            new(0, 4, 7, 3), // x = -1
            new(1, 2, 6, 5), // x = +1
            new(0, 1, 5, 4), // y = -1
            new(3, 7, 6, 2), // y = +1
        };
            return new Mesh(v, f);
        }

        public static Mesh CreateOctahedron()
        {
            var v = new List<Point3>
        {
            new( 1, 0, 0), new(-1, 0, 0),
            new( 0, 1, 0), new( 0,-1, 0),
            new( 0, 0, 1), new( 0, 0,-1),
        };
            var f = new List<Face>
        {
            new(0, 2, 4), new(2, 1, 4), new(1, 3, 4), new(3, 0, 4),
            new(2, 0, 5), new(1, 2, 5), new(3, 1, 5), new(0, 3, 5),
        };
            return new Mesh(v, f);
        }


        public static Mesh CreateIcosahedron()
        {
            float phi = (1f + MathF.Sqrt(5f)) / 2f;
            float k = 1f / MathF.Sqrt(1f + phi * phi);

            var v = new List<Point3>
        {
            new(-1,  phi, 0), new( 1,  phi, 0), new(-1, -phi, 0), new( 1, -phi, 0),
            new( 0, -1,  phi), new( 0,  1,  phi), new( 0, -1, -phi), new( 0,  1, -phi),
            new( phi, 0, -1), new( phi, 0,  1), new(-phi, 0, -1), new(-phi, 0,  1),
        };
            for (int i = 0; i < v.Count; i++) v[i] = v[i] * k;

            var f = new List<Face>
        {
            new(0,11,5), new(0,5,1),  new(0,1,7),  new(0,7,10), new(0,10,11),
            new(1,5,9),  new(5,11,4), new(11,10,2),new(10,7,6), new(7,1,8),
            new(3,9,4),  new(3,4,2),  new(3,2,6),  new(3,6,8),  new(3,8,9),
            new(4,9,5),  new(2,4,11), new(6,2,10), new(8,6,7),  new(9,8,1),
        };
            return new Mesh(v, f);
        }

        public static Mesh CreateDodecahedron()
        {
            float phi = (1f + MathF.Sqrt(5f)) / 2f;
            float k = 1f / MathF.Sqrt(3f);

            var vertices = new List<Point3>
            {
                new( k,  k,  k), new( k,  k, -k), new( k, -k,  k), new( k, -k, -k),
                new(-k,  k,  k), new(-k,  k, -k), new(-k, -k,  k), new(-k, -k, -k),
                
                new(0,  k / phi,  k * phi), new(0,  k / phi, -k * phi),
                new(0, -k / phi,  k * phi), new(0, -k / phi, -k * phi),

                new( k / phi,  k * phi, 0), new( k / phi, -k * phi, 0),
                new(-k / phi,  k * phi, 0), new(-k / phi, -k * phi, 0),

                new( k * phi, 0,  k / phi), new( k * phi, 0, -k / phi),
                new(-k * phi, 0,  k / phi), new(-k * phi, 0, -k / phi)
            };

            var faces = new List<Face>
            {
                new(8, 10, 2, 16, 0),
                new(16, 17, 1, 12, 0),
                new(12, 14, 4, 8, 0),
                new(18, 6, 10, 8, 4),
                new(14, 5, 19, 18, 4),
                new(9, 5, 14, 12, 1),
                new(17, 3, 11, 9, 1),
                new(13, 3, 17, 16, 2),
                new(10, 6, 15, 13, 2),
                new(13, 15, 7, 11, 3),
                new(9, 11, 7, 19, 5),
                new(18, 19, 7, 15, 6)
            };

            return new Mesh(vertices, faces);
        }
    }
}
