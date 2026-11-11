using System;
using System.Collections.Generic;
using LABA_6.Core.Models;

namespace LABA_6.Core.Factory
{
    public static class FunctionGraphFactory
    {
        public static Mesh Create(Func<float, float, float> func,
                                  float xMin, float xMax,
                                  float yMin, float yMax,
                                  int divisions)
        {
            if (divisions < 1) throw new ArgumentException("Разбиений должно быть >= 1");

            var vertices = new List<Point3>();
            var faces = new List<Face>();

            float stepX = (xMax - xMin) / divisions;
            float stepY = (yMax - yMin) / divisions;

            for (int i = 0; i <= divisions; i++)
            {
                float x = xMin + i * stepX;
                for (int j = 0; j <= divisions; j++)
                {
                    float y = yMin + j * stepY;
                    float z = func(x, y);
                    vertices.Add(new Point3(x, y, z));
                }
            }

            for (int i = 0; i < divisions; i++)
            {
                for (int j = 0; j < divisions; j++)
                {
                    int row1 = i * (divisions + 1);
                    int row2 = (i + 1) * (divisions + 1);

                    int p0 = row1 + j;
                    int p1 = row1 + j + 1;
                    int p2 = row2 + j + 1;
                    int p3 = row2 + j;

                    faces.Add(new Face(p0, p1, p2, p3));
                }
            }

            return new Mesh(vertices, faces);
        }
    }
}