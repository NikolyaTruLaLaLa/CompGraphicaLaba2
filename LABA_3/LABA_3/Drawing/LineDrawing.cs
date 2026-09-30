using System;

namespace LABA_3.Drawing
{
    /// <summary>Растровые линии. Брезенхэм с квадратной кистью thickness×thickness.</summary>
    public static class LineDrawing
    {
        public static void DrawLine(PixelBuffer buf, int x0, int y0, int x1, int y1, int color, int thickness)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int r = thickness / 2;

            int minX = Math.Min(x0, x1) - r;
            int minY = Math.Min(y0, y1) - r;
            int maxX = Math.Max(x0, x1) + r;
            int maxY = Math.Max(y0, y1) + r;

            while (true)
            {
                for (int oy = -r; oy <= r; oy++)
                    for (int ox = -r; ox <= r; ox++)
                        SetPixel(buf, x0 + ox, y0 + oy, color);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }

            buf.CommitRegion(minX, minY, maxX, maxY);
        }

        public static void SetPixel(PixelBuffer buf, int x, int y, int color)
        {
            if (!buf.InBounds(x, y)) return;
            buf.Pixels[y * buf.Width + x] = color;
        }
    }
}