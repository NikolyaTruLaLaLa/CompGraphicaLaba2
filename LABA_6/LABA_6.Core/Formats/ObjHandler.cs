using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using LABA_6.Core.Models;

namespace LABA_6.Core.Formats
{
    public static class ObjHandler
    {
        public static Mesh Load(string filePath)
        {
            var vertices = new List<Point3>();
            var faces = new List<Face>();
            
            var culture = CultureInfo.InvariantCulture;

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                if (parts[0] == "v") 
                {
                    if (parts.Length >= 4)
                    {
                        float x = float.Parse(parts[1], culture);
                        float y = float.Parse(parts[2], culture);
                        float z = float.Parse(parts[3], culture);
                        vertices.Add(new Point3(x, y, z));
                    }
                }
                else if (parts[0] == "f") 
                {
                    var faceIndices = new List<int>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string vertexStr = parts[i].Split('/')[0];

                        // В OBJ индексы начинаются с 1, а у нас в C# с 0
                        int index = int.Parse(vertexStr) - 1;
                        faceIndices.Add(index);
                    }

                    if (faceIndices.Count >= 3)
                    {
                        faces.Add(new Face(faceIndices.ToArray()));
                    }
                }
            }

            return new Mesh(vertices, faces);
        }

        public static void Save(Mesh mesh, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Exported from LABA_6 Graphics Engine");
            sb.AppendLine("o CustomMesh");

            var culture = CultureInfo.InvariantCulture;

            foreach (var v in mesh.Vertices)
            {
                sb.AppendLine($"v {v.X.ToString(culture)} {v.Y.ToString(culture)} {v.Z.ToString(culture)}");
            }

            foreach (var f in mesh.Faces)
            {
                sb.Append("f ");
                foreach (var index in f.Indices)
                {
                    // Прибавляем 1, так как формат OBJ 1-based
                    sb.Append($"{index + 1} ");
                }
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}