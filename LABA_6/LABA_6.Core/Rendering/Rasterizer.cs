using System;
using LABA_6.Core.Models;

namespace LABA_6.Core.Rendering
{
    public static class Rasterizer
    {
        public const int ColorBlack = unchecked((int)0xFF000000);
        public const int ColorCyan = unchecked((int)0xFF00FFFF);
        public const int ColorRed = unchecked((int)0xFFFF0000);
        public const int ColorLime = unchecked((int)0xFF00FF00);
        public const int ColorBlue = unchecked((int)0xFF0000FF);
        public const int ColorWhite = unchecked((int)0xFFFFFFFF);

        public static void DrawLine(FrameBuffer fb, int x0, int y0, float z0, int x1, int y1, float z1, int color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            int steps = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
            if (steps == 0) { fb.SetPixel(x0, y0, z0, color); return; }

            float dz = (z1 - z0) / steps;
            float currentZ = z0;

            while (true)
            {
                fb.SetPixel(x0, y0, currentZ, color);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
                currentZ += dz;
            }
        }
        public static void DrawTriangle(FrameBuffer fb, Point3 v1, Point3 v2, Point3 v3, int color)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));


            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return; 

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0); 

                    // барицентрические координаты
                    float w0 = EdgeFunction(v2, v3, p);
                    float w1 = EdgeFunction(v3, v1, p);
                    float w2 = EdgeFunction(v1, v2, p);

                    if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0))
                    {
                        w0 /= area;
                        w1 /= area;
                        w2 /= area;
                        float z = w0 * v1.Z + w1 * v2.Z + w2 * v3.Z;
                        fb.SetPixel(x, y, z, color);
                    }
                }
            }
        }
        private static float EdgeFunction(Point3 a, Point3 b, Point3 c)
        {
            return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
        }
    }
}