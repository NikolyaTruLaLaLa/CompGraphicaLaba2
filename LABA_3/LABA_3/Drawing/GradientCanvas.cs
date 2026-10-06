using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Задание 3: градиентная заливка треугольника поверх общего холста.
    /// Цвет каждого пикселя — смесь цветов трёх вершин с барицентрическими
    /// весами (затенение Гуро): чем ближе пиксель к вершине, тем больше её доля.
    /// </summary>
    public sealed class GradientCanvas : PixelCanvas
    {
        private const double Epsilon = 1e-9;

        /// <summary>
        /// Заливает треугольник градиентом цветов его вершин.
        /// Порядок вершин любой, вырожденный треугольник игнорируется.
        /// </summary>
        public void RasterizeTriangle(GradientVertex v0, GradientVertex v1, GradientVertex v2)
        {
            double area = SignedArea(v0, v1, v2);
            if (Math.Abs(area) < Epsilon) return;

            // Знак обхода вершин: веса положительны внутри при любом порядке.
            double orientation = area < 0 ? -1.0 : 1.0;
            double totalArea = Math.Abs(area);

            int yTop = ClampY((int)Math.Ceiling(MinY(v0, v1, v2) - 0.5));
            int yBottom = ClampY((int)Math.Floor(MaxY(v0, v1, v2) - 0.5));

            for (int y = yTop; y <= yBottom; y++)
            {
                double py = y + 0.5;

                // Отрезок строки внутри треугольника: пересечения с тремя сторонами.
                double xMin = double.PositiveInfinity;
                double xMax = double.NegativeInfinity;
                AccumulateCrossing(v0.Position, v1.Position, py, ref xMin, ref xMax);
                AccumulateCrossing(v1.Position, v2.Position, py, ref xMin, ref xMax);
                AccumulateCrossing(v2.Position, v0.Position, py, ref xMin, ref xMax);
                if (double.IsInfinity(xMin) || xMin > xMax) continue;

                int xStart = Math.Max(0, (int)Math.Ceiling(xMin - 0.5));
                int xEnd = Math.Min(Width - 1, (int)Math.Floor(xMax - 0.5));

                // Веса в первом пикселе строки (через функции рёбер)...
                double px = xStart + 0.5;
                double w0 = orientation * Edge(v1.Position, v2.Position, px, py);
                double w1 = orientation * Edge(v2.Position, v0.Position, px, py);
                double w2 = orientation * Edge(v0.Position, v1.Position, px, py);

                // ...а дальше веса идут приращениями: сдвиг на пиксель вправо
                // меняет каждую функцию ребра на -(dy стороны).
                double step0 = -orientation * (v2.Position.Y - v1.Position.Y);
                double step1 = -orientation * (v0.Position.Y - v2.Position.Y);
                double step2 = -orientation * (v1.Position.Y - v0.Position.Y);

                for (int x = xStart; x <= xEnd; x++)
                {
                    if (w0 >= -Epsilon && w1 >= -Epsilon && w2 >= -Epsilon)
                        SetPixel(x, y, PackBlend(v0.Color, v1.Color, v2.Color,
                                                 w0 / totalArea, w1 / totalArea, w2 / totalArea));

                    w0 += step0;
                    w1 += step1;
                    w2 += step2;
                }
            }

            UpdateBitmap();
        }

        /// <summary>
        /// Контур недостроенного треугольника (предпросмотр) — отрезки Брезенхема.
        /// </summary>
        public void DrawContour(IReadOnlyList<Point> points, Color color)
        {
            int argb = PackColor(color);

            for (int i = 1; i < points.Count; i++)
                DrawLineBresenham((int)points[i - 1].X, (int)points[i - 1].Y,
                                  (int)points[i].X, (int)points[i].Y, argb);

            UpdateBitmap();
        }

        /// <summary>Функция ребра: ноль на прямой ab, знак — сторона точки p.</summary>
        private static double Edge(Point a, Point b, double px, double py)
            => (b.X - a.X) * (py - a.Y) - (b.Y - a.Y) * (px - a.X);

        /// <summary>Удвоенная ориентированная площадь треугольника.</summary>
        private static double SignedArea(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Edge(v0.Position, v1.Position, v2.Position.X, v2.Position.Y);

        /// <summary>Расширяет отрезок строки точкой пересечения стороны ab с горизонталью py.</summary>
        private static void AccumulateCrossing(Point a, Point b, double py, ref double xMin, ref double xMax)
        {
            double dy = b.Y - a.Y;
            if (Math.Abs(dy) < Epsilon) return; // горизонтальная сторона строку не пересекает

            double px = a.X + (py - a.Y) * (b.X - a.X) / dy;
            if (px < xMin) xMin = px;
            if (px > xMax) xMax = px;
        }

        /// <summary>Смешивает каналы трёх цветов по весам и упаковывает в AARRGGBB.</summary>
        private static int PackBlend(Color c0, Color c1, Color c2, double l0, double l1, double l2)
            => unchecked((int)(0xFF000000u
                | ((uint)MixChannel(l0 * c0.R + l1 * c1.R + l2 * c2.R) << 16)
                | ((uint)MixChannel(l0 * c0.G + l1 * c1.G + l2 * c2.G) << 8)
                | MixChannel(l0 * c0.B + l1 * c1.B + l2 * c2.B)));

        private static byte MixChannel(double value)
        {
            if (value <= 0) return 0;
            if (value >= 255) return 255;
            return (byte)(value + 0.5);
        }

        private static double MinY(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Min(v0.Position.Y, Math.Min(v1.Position.Y, v2.Position.Y));

        private static double MaxY(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Max(v0.Position.Y, Math.Max(v1.Position.Y, v2.Position.Y));

        private static int ClampY(int y) => Math.Clamp(y, 0, Height - 1);
    }
}
