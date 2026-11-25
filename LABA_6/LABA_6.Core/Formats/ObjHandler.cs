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
            var texCoords = new List<Point2>();
            var faces = new List<Face>();
            var normals = new List<Point3>();

            var culture = CultureInfo.InvariantCulture;
            string? mtlFileName = null;

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                if (parts[0] == "v")
                {
                    float x = float.Parse(parts[1], culture);
                    float y = float.Parse(parts[2], culture);
                    float z = float.Parse(parts[3], culture);
                    vertices.Add(new Point3(x, y, z));
                }
                else if (parts[0] == "vt")
                {
                    float u = float.Parse(parts[1], culture);
                    float v = parts.Length > 2 ? float.Parse(parts[2], culture) : 0;
                    texCoords.Add(new Point2(u, v));
                }
                else if (parts[0] == "mtllib")
                {
                    // Читаем всю строку после mtllib (с учетом возможных пробелов)
                    mtlFileName = line.Substring(line.IndexOf("mtllib") + 6).Trim();
                }
                else if (parts[0] == "f")
                {
                    var faceIndices = new List<int>();
                    var faceTexIndices = new List<int>();
                    var faceNormalIndices = new List<int>();

                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] indices = parts[i].Split('/');
                        faceIndices.Add(int.Parse(indices[0]) - 1);

                        if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]))
                            faceTexIndices.Add(int.Parse(indices[1]) - 1);

                        if (indices.Length > 2 && !string.IsNullOrEmpty(indices[2]))
                            faceNormalIndices.Add(int.Parse(indices[2]) - 1);
                    }

                    if (faceIndices.Count >= 3)
                    {
                        var texIdxArray = faceTexIndices.Count == faceIndices.Count ? faceTexIndices.ToArray() : null;
                        var normIdxArray = faceNormalIndices.Count == faceIndices.Count ? faceNormalIndices.ToArray() : null;
                        faces.Add(new Face(faceIndices.ToArray(), texIdxArray, normIdxArray));
                    }
                }
                else if (parts[0] == "vn")
                {
                    float x = float.Parse(parts[1], culture);
                    float y = float.Parse(parts[2], culture);
                    float z = float.Parse(parts[3], culture);
                    normals.Add(new Point3(x, y, z));
                }
            }

            var mesh = new Mesh(vertices, faces);
            mesh.TexCoords.AddRange(texCoords);
            mesh.Normals.AddRange(normals);

            if (mtlFileName != null)
            {
                string mtlPath = Path.Combine(Path.GetDirectoryName(filePath)!, mtlFileName);
                mesh.TexturePath = GetTexturePathFromMtl(mtlPath);
            }

            return mesh;
        }

        private static string? GetTexturePathFromMtl(string mtlPath)
        {
            if (!File.Exists(mtlPath)) return null;

            foreach (var line in File.ReadLines(mtlPath))
            {
                if (line.TrimStart().StartsWith("map_Kd"))
                {
                    // Читаем всю строку после map_Kd (включая пробелы)
                    string relativePath = line.Substring(line.IndexOf("map_Kd") + 6).Trim();
                    return Path.Combine(Path.GetDirectoryName(mtlPath)!, relativePath);
                }
            }
            return null;
        }

        public static void Save(Mesh mesh, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Exported from LABA_6 Graphics Engine");
            var culture = CultureInfo.InvariantCulture;

            foreach (var v in mesh.Vertices)
                sb.AppendLine($"v {v.X.ToString(culture)} {v.Y.ToString(culture)} {v.Z.ToString(culture)}");

            foreach (var f in mesh.Faces)
            {
                sb.Append("f ");
                foreach (var index in f.Indices) sb.Append($"{index + 1} ");
                sb.AppendLine();
            }
            File.WriteAllText(filePath, sb.ToString());
        }
    }
}