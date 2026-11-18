using System;
using System.Collections.Generic;
using LABA_6.Core.Models;

namespace LABA_6.Core.Rendering
{
    public static class SoftwareRenderer
    {
        public static void Render(FrameBuffer fb, List<SceneObject> objects, Camera camera,
                                  bool isPerspective, bool doCulling, Point3 customViewDir,
                                  bool showNormals, out int renderedFaces)
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

                var screenPoints = new Point3?[mesh.VertexCount];
                var worldPoints = new Point3[mesh.VertexCount];

                for (int i = 0; i < mesh.VertexCount; i++)
                {
                    worldPoints[i] = modelMatrix * mesh.Vertices[i];
                    var v = Matrix4x4.MultiplyWithW(mvp, mesh.Vertices[i], out float w);

                    if (w <= 0.001f || float.IsNaN(w)) { screenPoints[i] = null; continue; }

                    float sx = (v.X + 1.0f) * 0.5f * fb.Width;
                    float sy = (1.0f - v.Y) * 0.5f * fb.Height;
                    screenPoints[i] = new Point3(sx, sy, v.Z);
                }

                foreach (var face in mesh.Faces)
                {
                    Point3 v0 = worldPoints[face.Indices[0]];
                    Point3 v1 = worldPoints[face.Indices[1]];
                    Point3 v2 = worldPoints[face.Indices[2]];
                    Point3 normal = Point3.Cross(v1 - v0, v2 - v0).Normalized();

                    if (doCulling)
                    {
                        if (isPerspective)
                        {
                            Point3 viewVector = (camera.Position - v0).Normalized();
                            if (Point3.Dot(normal, viewVector) <= 0) continue;
                        }
                        else
                        {
                            if (Point3.Dot(normal, -customViewDir) <= 0) continue;
                        }
                    }

                    renderedFaces++;

                    // Заливка (используем цвет объекта)
                    for (int i = 1; i < face.Count - 1; i++)
                    {
                        var p1 = screenPoints[face.Indices[0]];
                        var p2 = screenPoints[face.Indices[i]];
                        var p3 = screenPoints[face.Indices[i + 1]];

                        if (p1 != null && p2 != null && p3 != null)
                            Rasterizer.DrawTriangle(fb, p1.Value, p2.Value, p3.Value, obj.Color);
                    }

                    // Ребра (белые)
                    for (int i = 0; i < face.Count; i++)
                    {
                        var pa = screenPoints[face.Indices[i]];
                        var pb = screenPoints[face.Indices[(i + 1) % face.Count]];
                        if (pa != null && pb != null)
                        {
                            Rasterizer.DrawLine(fb, (int)pa.Value.X, (int)pa.Value.Y, pa.Value.Z - 0.001f,
                                                    (int)pb.Value.X, (int)pb.Value.Y, pb.Value.Z - 0.001f,
                                                    Rasterizer.ColorWhite);
                        }
                    }

                    // Нормали
                    if (showNormals)
                    {
                        Point3 center = Point3.Zero;
                        for (int i = 0; i < face.Count; i++) center += worldPoints[face.Indices[i]];
                        center /= face.Count;

                        Point3 normalEnd = center + normal * 0.4f;

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