using System.Collections.Generic;
using System.Windows;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Обход границы связной области по алгоритму Мура.
    /// Возвращает список точек в порядке обхода.
    /// </summary>
    public static class MooreBoundary
    {
        // 8 соседей по часовой стрелке: 0=NW, 1=N, 2=NE, 3=E, 4=SE, 5=S, 6=SW, 7=W
        private static readonly int[] DX = { -1, 0, 1, 1, 1, 0, -1, -1 };
        private static readonly int[] DY = { -1, -1, -1, 0, 1, 1, 1, 0 };

        /// <summary>Ищет ближайший пиксель границы заданного цвета вокруг (cx, cy).</summary>
        public static bool FindNearest(PixelBuffer buf, int cx, int cy, int boundaryColor,
                                       int maxRadius, out int bx, out int by)
        {
            bx = by = -1;
            if (IsBoundary(buf, cx, cy, boundaryColor)) { bx = cx; by = cy; return true; }

            for (int r = 1; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    if (IsBoundary(buf, cx + dx, cy - r, boundaryColor)) { bx = cx + dx; by = cy - r; return true; }
                    if (IsBoundary(buf, cx + dx, cy + r, boundaryColor)) { bx = cx + dx; by = cy + r; return true; }
                }
                for (int dy = -r + 1; dy <= r - 1; dy++)
                {
                    if (IsBoundary(buf, cx - r, cy + dy, boundaryColor)) { bx = cx - r; by = cy + dy; return true; }
                    if (IsBoundary(buf, cx + r, cy + dy, boundaryColor)) { bx = cx + r; by = cy + dy; return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// Обход контура, начиная с (startX, startY), который уже является пикселем границы.
        /// </summary>
        public static List<Point> Trace(PixelBuffer buf, int startX, int startY, int boundaryColor)
        {
            List<Point> contour = new List<Point>();

            int x = startX, y = startY;
            int dir = 7; // пришли как будто с запада

            long maxSteps = (long)buf.Width * buf.Height * 4L;
            long steps = 0;

            do
            {
                contour.Add(new Point(x, y));

                bool found = false;
                for (int i = 1; i <= 8; i++)
                {
                    int nd = (dir + i) % 8;
                    int nx = x + DX[nd];
                    int ny = y + DY[nd];

                    if (IsBoundary(buf, nx, ny, boundaryColor))
                    {
                        dir = (nd + 4) % 8; // обратное направление
                        x = nx;
                        y = ny;
                        found = true;
                        break;
                    }
                }

                if (!found) break; // изолированный пиксель
                steps++;
                if (steps > maxSteps) break;
            }
            while (!(x == startX && y == startY && contour.Count > 1));

            return contour;
        }

        private static bool IsBoundary(PixelBuffer buf, int x, int y, int color)
        {
            if (!buf.InBounds(x, y)) return false;
            return buf.Pixels[y * buf.Width + x] == color;
        }
    }
}