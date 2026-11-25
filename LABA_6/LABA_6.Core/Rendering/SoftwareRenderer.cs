using System;
using System.Collections.Generic;
using LABA_6.Core.Models;

namespace LABA_6.Core.Rendering
{
    public static class SoftwareRenderer
    {
        public static void Render(FrameBuffer fb, List<SceneObject> objects, Camera camera,
                                  bool isPerspective, bool doCulling, Point3 customViewDir,
                                  bool showNormals, Texture? texture, Light light,
                                  ShadingMode shadingMode, int objectColor, out int renderedFaces)
        {
            renderedFaces = 0;
            fb.Clear(Rasterizer.ColorBlack);

            float aspect = (float)fb.Width / fb.Height;
            Matrix4x4 view = camera.GetViewMatrix();
            Matrix4x4 proj = isPerspective ? camera.GetProjectionMatrix(aspect) : Matrix4x4.Scale(0.4f, 0.4f, 0.4f);
            Matrix4x4 viewProj = proj * view;

            foreach (var obj in objects)
            {
                Mesh mesh = obj.Mesh;
                Matrix4x4 modelMatrix = obj.ModelMatrix;
                Matrix4x4 mvp = viewProj * modelMatrix;

                if (mesh.Normals.Count == 0)
                {
                    CalculateVertexNormals(mesh);
                }

                var screenPoints = new Point3?[mesh.VertexCount];
                var screenWs = new float[mesh.VertexCount];
                var worldPoints = new Point3[mesh.VertexCount];
                var worldNormals = new Point3[mesh.VertexCount];

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    worldPoints[i] = modelMatrix * mesh.Vertices[i];

                    var n = mesh.Normals[i];
                    worldNormals[i] = (modelMatrix * new Point3(n.X, n.Y, n.Z)).Normalized();

                    var v = Matrix4x4.MultiplyWithW(mvp, mesh.Vertices[i], out float w);

                    if (w <= 0.001f || float.IsNaN(w)) { screenPoints[i] = null; continue; }

                    float sx = (v.X + 1.0f) * 0.5f * fb.Width;
                    float sy = (1.0f - v.Y) * 0.5f * fb.Height;
                    screenPoints[i] = new Point3(sx, sy, v.Z);
                    screenWs[i] = w;
                }

                foreach (var face in mesh.Faces)
                {
                    Point3 v0 = worldPoints[face.Indices[0]];
                    Point3 v1 = worldPoints[face.Indices[1]];
                    Point3 v2 = worldPoints[face.Indices[2]];
                    Point3 faceNormal = Point3.Cross(v1 - v0, v2 - v0).Normalized();

                    if (doCulling)
                    {
                        if (isPerspective)
                        {
                            if (Point3.Dot(faceNormal, (camera.Position - v0).Normalized()) <= 0) continue;
                        }
                        else
                        {
                            if (Point3.Dot(faceNormal, -customViewDir) <= 0) continue;
                        }
                    }

                    renderedFaces++;

                    float[] vertexIntensities = new float[face.Count];
                    int[] vertexColors = new int[face.Count];

                    if (shadingMode == ShadingMode.Gouraud)
                    {
                        for (int i = 0; i < face.Count; i++)
                        {
                            int vIdx = face.Indices[i];
                            int nIdx = (face.NormalIndices != null && i < face.NormalIndices.Length) ? face.NormalIndices[i] : vIdx;

                            Point3 L = (light.Position - worldPoints[vIdx]).Normalized();
                            float dot = Point3.Dot(worldNormals[nIdx], L);
                            float intensity = Math.Clamp(light.Ambient + light.Intensity * Math.Max(0, dot), 0f, 1f);

                            vertexIntensities[i] = intensity;
                            vertexColors[i] = ApplyIntensity(objectColor, intensity);
                        }
                    }

                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        var p1 = screenPoints[face.Indices[0]];
                        var p2 = screenPoints[face.Indices[i]];
                        var p3 = screenPoints[face.Indices[i + 1]];

                        if (p1 != null && p2 != null && p3 != null)
                        {
                            bool hasTexture = texture != null && face.TexIndices != null && mesh.TexCoords.Count > 0;

                            if (shadingMode == ShadingMode.Phong || shadingMode == ShadingMode.Toon)
                            {
                                // Берем нормали для текущих вершин грани
                                int nIdx0 = (face.NormalIndices != null) ? face.NormalIndices[0] : face.Indices[0];
                                int nIdx1 = (face.NormalIndices != null) ? face.NormalIndices[i] : face.Indices[i];
                                int nIdx2 = (face.NormalIndices != null) ? face.NormalIndices[i + 1] : face.Indices[i + 1];

                                if (hasTexture)
                                {
                                    // ИСПОЛЬЗУЕМ ТЕКСТУРНЫЙ ФОНГ/ТУН
                                    var uv1 = mesh.TexCoords[face.TexIndices![0]];
                                    var uv2 = mesh.TexCoords[face.TexIndices[i]];
                                    var uv3 = mesh.TexCoords[face.TexIndices[i + 1]];

                                    Rasterizer.DrawTriangleTexturedPhong(fb,
                                        p1.Value, uv1, screenWs[face.Indices[0]], worldNormals[nIdx0], worldPoints[face.Indices[0]],
                                        p2.Value, uv2, screenWs[face.Indices[i]], worldNormals[nIdx1], worldPoints[face.Indices[i]],
                                        p3.Value, uv3, screenWs[face.Indices[i + 1]], worldNormals[nIdx2], worldPoints[face.Indices[i + 1]],
                                        light.Position, light.Ambient, light.Intensity, texture!, shadingMode == ShadingMode.Toon);
                                }
                                else
                                {
                                    // ОБЫЧНЫЙ ФОНГ/ТУН БЕЗ ТЕКСТУРЫ
                                    Rasterizer.DrawTrianglePhong(fb,
                                        p1.Value, worldNormals[nIdx0], worldPoints[face.Indices[0]],
                                        p2.Value, worldNormals[nIdx1], worldPoints[face.Indices[i]],
                                        p3.Value, worldNormals[nIdx2], worldPoints[face.Indices[i + 1]],
                                        light.Position, light.Ambient, light.Intensity,
                                        objectColor, shadingMode == ShadingMode.Toon);
                                }
                            }
                            else if (hasTexture)
                            {
                                var uv1 = mesh.TexCoords[face.TexIndices![0]];
                                var uv2 = mesh.TexCoords[face.TexIndices[i]];
                                var uv3 = mesh.TexCoords[face.TexIndices[i + 1]];

                                if (shadingMode == ShadingMode.Gouraud)
                                {
                                    Rasterizer.DrawTriangleTexturedGouraud(fb,
                                        p1.Value, uv1, screenWs[face.Indices[0]], vertexIntensities[0],
                                        p2.Value, uv2, screenWs[face.Indices[i]], vertexIntensities[i],
                                        p3.Value, uv3, screenWs[face.Indices[i + 1]], vertexIntensities[i + 1], texture!);
                                }
                                else
                                {
                                    Rasterizer.DrawTriangleTextured(fb, p1.Value, uv1, screenWs[face.Indices[0]],
                                        p2.Value, uv2, screenWs[face.Indices[i]], p3.Value, uv3, screenWs[face.Indices[i + 1]], texture!);
                                }
                            }
                            else if (shadingMode == ShadingMode.Gouraud)
                            {
                                Rasterizer.DrawTriangleGouraud(fb,
                                    p1.Value, vertexColors[0], p2.Value, vertexColors[i], p3.Value, vertexColors[i + 1]);
                            }
                            else
                            {
                                Rasterizer.DrawTriangle(fb, p1.Value, p2.Value, p3.Value, objectColor);
                            }
                        }
                    }

                    // Отрисовка нормалей (Красным цветом)
                    if (showNormals)
                    {
                        Point3 center = Point3.Zero;
                        for (int j = 0; j < face.Count; j++) center += worldPoints[face.Indices[j]];
                        center /= face.Count;

                        Point3 normalEnd = center + faceNormal * 0.4f;

                        var screenCenter = ProjectPoint(center, viewProj, fb.Width, fb.Height);
                        var screenNormal = ProjectPoint(normalEnd, viewProj, fb.Width, fb.Height);

                        if (screenCenter != null && screenNormal != null)
                        {
                            Rasterizer.DrawLine(fb, (int)screenCenter.Value.X, (int)screenCenter.Value.Y, screenCenter.Value.Z,
                                                    (int)screenNormal.Value.X, (int)screenNormal.Value.Y, screenNormal.Value.Z,
                                                    Rasterizer.ColorRed);
                        }
                    }
                }
            }
            DrawAxes(fb, viewProj);
        }

        private static void CalculateVertexNormals(Mesh mesh)
        {
            mesh.Normals.Clear();
            for (int i = 0; i < mesh.VertexCount; i++) mesh.Normals.Add(Point3.Zero);
            foreach (var face in mesh.Faces)
            {
                Point3 v0 = mesh.Vertices[face.Indices[0]];
                Point3 v1 = mesh.Vertices[face.Indices[1]];
                Point3 v2 = mesh.Vertices[face.Indices[2]];
                Point3 fn = Point3.Cross(v1 - v0, v2 - v0).Normalized();
                foreach (var idx in face.Indices) mesh.Normals[idx] += fn;
            }
            for (int i = 0; i < mesh.Normals.Count; i++) mesh.Normals[i] = mesh.Normals[i].Normalized();
        }

        private static int ApplyIntensity(int color, float intensity)
        {
            int a = (color >> 24) & 0xFF;
            int r = (int)(((color >> 16) & 0xFF) * intensity);
            int g = (int)(((color >> 8) & 0xFF) * intensity);
            int b = (int)((color & 0xFF) * intensity);
            return (a << 24) | (Math.Clamp(r, 0, 255) << 16) | (Math.Clamp(g, 0, 255) << 8) | Math.Clamp(b, 0, 255);
        }

        private static void DrawAxes(FrameBuffer fb, Matrix4x4 viewProj)
        {
            float len = 2.5f;
            var o = ProjectPoint(new Point3(0, 0, 0), viewProj, fb.Width, fb.Height);
            var x = ProjectPoint(new Point3(len, 0, 0), viewProj, fb.Width, fb.Height);
            var y = ProjectPoint(new Point3(0, len, 0), viewProj, fb.Width, fb.Height);
            var z = ProjectPoint(new Point3(0, 0, len), viewProj, fb.Width, fb.Height);

            if (o.HasValue && x.HasValue) Rasterizer.DrawLine(fb, (int)o.Value.X, (int)o.Value.Y, o.Value.Z, (int)x.Value.X, (int)x.Value.Y, x.Value.Z, Rasterizer.ColorRed);
            if (o.HasValue && y.HasValue) Rasterizer.DrawLine(fb, (int)o.Value.X, (int)o.Value.Y, o.Value.Z, (int)y.Value.X, (int)y.Value.Y, y.Value.Z, Rasterizer.ColorLime);
            if (o.HasValue && z.HasValue) Rasterizer.DrawLine(fb, (int)o.Value.X, (int)o.Value.Y, o.Value.Z, (int)z.Value.X, (int)z.Value.Y, z.Value.Z, Rasterizer.ColorBlue);
        }

        private static Point3? ProjectPoint(Point3 p, Matrix4x4 mvp, int width, int height)
        {
            var v = Matrix4x4.MultiplyWithW(mvp, p, out float w);
            if (w <= 0.001f || float.IsNaN(w)) return null;
            float sx = (v.X + 1.0f) * 0.5f * width;
            float sy = (1.0f - v.Y) * 0.5f * height;
            return new Point3(sx, sy, v.Z);
        }
    }
}