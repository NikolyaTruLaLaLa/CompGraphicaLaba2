using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Градиентная растеризация треугольника поверх общего пиксельного холста
    /// <see cref="PixelCanvas"/>. Цвет каждого залитого пикселя — линейная
    /// комбинация цветов вершин с барицентрическими весами (затенение Гуро).
    /// </summary>
    public sealed class GradientCanvas : PixelCanvas
    {
        /// <summary>Допуск сравнения вещественных величин (площадь, совпадение координат).</summary>
        private const double Epsilon = 1e-9;

        /// <summary>Промежуточный буфер заливки: упакованный цвет пикселя или маркер «не залито».</summary>
        private readonly int[] _gradient = new int[PixelCount];

        /// <summary>Сколько пикселей залила последняя растеризация (для строки состояния).</summary>
        public int FilledPixelCount { get; private set; }

        /// <inheritdoc/>
        public override void Clear()
        {
            base.Clear();
            FilledPixelCount = 0;
        }

        /// <summary>Ломаная по заданным точкам — предпросмотр недостроенного треугольника.</summary>
        public void DrawPolyline(IReadOnlyList<Point> points, Color color)
        {
            int argb = PackColor(color);

            for (int i = 1; i < points.Count; i++)
                DrawLineBresenham((int)points[i - 1].X, (int)points[i - 1].Y, (int)points[i].X, (int)points[i].Y, argb);

            foreach (Point point in points)
                DrawMarker((int)point.X, (int)point.Y, argb);

            UpdateBitmap();
        }

        /// <summary>
        /// Заливает треугольник градиентом цветов его вершин.
        /// </summary>
        /// <param name="showOverlay">Рисовать поверх заливки контур и маркеры вершин.</param>
        public void RasterizeTriangle(GradientVertex v0, GradientVertex v1, GradientVertex v2,
                                      TriangleRasterizer rasterizer, bool showOverlay)
        {
            FilledPixelCount = 0;
            Fill(v0, v1, v2, rasterizer, _gradient);

            for (int i = 0; i < PixelCount; i++)
            {
                if (_gradient[i] == NoPixel) continue;

                FilledPixelCount++;
                int argb = _gradient[i];
                int index = i * 4;
                Pixels[index] = (byte)(argb & 0xFF);
                Pixels[index + 1] = (byte)((argb >> 8) & 0xFF);
                Pixels[index + 2] = (byte)((argb >> 16) & 0xFF);
                Pixels[index + 3] = (byte)((argb >> 24) & 0xFF);
            }

            if (showOverlay) DrawOverlay(v0, v1, v2);

            UpdateBitmap();
        }

        /// <summary>
        /// Прогоняет оба алгоритма на временных буферах и считает расхождения.
        /// Ноль означает, что способы растеризации дали идентичный результат.
        /// </summary>
        public int CompareRasterizers(GradientVertex v0, GradientVertex v1, GradientVertex v2)
        {
            int[] byScanLine = new int[PixelCount];
            int[] byEdges = new int[PixelCount];

            Fill(v0, v1, v2, TriangleRasterizer.ScanLineBarycentric, byScanLine);
            Fill(v0, v1, v2, TriangleRasterizer.EdgeFunctions, byEdges);

            int differences = 0;
            for (int i = 0; i < PixelCount; i++)
                if (byScanLine[i] != byEdges[i]) differences++;

            return differences;
        }

        /// <summary>Выбирает нужный алгоритм растеризации и заливает им переданный буфер.</summary>
        private static void Fill(GradientVertex v0, GradientVertex v1, GradientVertex v2,
                                 TriangleRasterizer rasterizer, int[] target)
        {
            if (rasterizer == TriangleRasterizer.EdgeFunctions)
                FillEdgeFunctions(v0, v1, v2, target);
            else
                FillScanLine(v0, v1, v2, target);
        }

        /// <summary>
        /// Построчное сканирование. Для каждой строки y берётся отрезок
        /// пересечения с тремя сторонами, затем барицентрические веса идут
        /// по строке приращениями (без пересчёта площадей для каждого пикселя).
        /// </summary>
        private static void FillScanLine(GradientVertex v0, GradientVertex v1, GradientVertex v2, int[] target)
        {
            Array.Fill(target, NoPixel);

            double area = SignedArea(v0, v1, v2);
            if (Math.Abs(area) < Epsilon) return; // вырожденный треугольник

            double orientation = area < 0 ? -1.0 : 1.0;
            double totalArea = Math.Abs(area);

            int yTop = ClampY((int)Math.Ceiling(MinY(v0, v1, v2) - 0.5));
            int yBottom = ClampY((int)Math.Floor(MaxY(v0, v1, v2) - 0.5));

            for (int y = yTop; y <= yBottom; y++)
            {
                double py = y + 0.5;

                double xMin = double.PositiveInfinity;
                double xMax = double.NegativeInfinity;
                AccumulateCrossing(v0.Position, v1.Position, py, ref xMin, ref xMax);
                AccumulateCrossing(v1.Position, v2.Position, py, ref xMin, ref xMax);
                AccumulateCrossing(v2.Position, v0.Position, py, ref xMin, ref xMax);
                if (double.IsInfinity(xMin) || xMin > xMax) continue;

                int xStart = Math.Max(0, (int)Math.Ceiling(xMin - 0.5));
                int xEnd = Math.Min(Width - 1, (int)Math.Floor(xMax - 0.5));

                // барицентрические веса (через edge functions) в крайнем левом пикселе строки
                double px = xStart + 0.5;
                double w0 = orientation * Edge(v1.Position, v2.Position, px, py);
                double w1 = orientation * Edge(v2.Position, v0.Position, px, py);
                double w2 = orientation * Edge(v0.Position, v1.Position, px, py);

                // приращения весов при сдвиге на один пиксель вправо
                double step0 = -orientation * (v2.Position.Y - v1.Position.Y);
                double step1 = -orientation * (v0.Position.Y - v2.Position.Y);
                double step2 = -orientation * (v1.Position.Y - v0.Position.Y);

                for (int x = xStart; x <= xEnd; x++)
                {
                    if (w0 >= -Epsilon && w1 >= -Epsilon && w2 >= -Epsilon)
                        target[y * Width + x] = PackBlend(v0.Color, v1.Color, v2.Color,
                                                           w0 / totalArea, w1 / totalArea, w2 / totalArea);

                    w0 += step0;
                    w1 += step1;
                    w2 += step2;
                }
            }
        }

        /// <summary>
        /// Перебор ограничивающего прямоугольника: пиксель внутри, если он
        /// лежит во всех трёх полуплоскостях сторон треугольника.
        /// </summary>
        private static void FillEdgeFunctions(GradientVertex v0, GradientVertex v1, GradientVertex v2, int[] target)
        {
            Array.Fill(target, NoPixel);

            double area = SignedArea(v0, v1, v2);
            if (Math.Abs(area) < Epsilon) return; // вырожденный треугольник

            double orientation = area < 0 ? -1.0 : 1.0;
            double totalArea = Math.Abs(area);

            int xLeft = Math.Max(0, (int)Math.Floor(MinX(v0, v1, v2) - 0.5));
            int xRight = Math.Min(Width - 1, (int)Math.Ceiling(MaxX(v0, v1, v2) - 0.5));
            int yTop = Math.Max(0, (int)Math.Floor(MinY(v0, v1, v2) - 0.5));
            int yBottom = Math.Min(Height - 1, (int)Math.Ceiling(MaxY(v0, v1, v2) - 0.5));

            for (int y = yTop; y <= yBottom; y++)
            {
                double py = y + 0.5;

                for (int x = xLeft; x <= xRight; x++)
                {
                    double px = x + 0.5;

                    double w0 = orientation * Edge(v1.Position, v2.Position, px, py);
                    if (w0 < -Epsilon) continue;

                    double w1 = orientation * Edge(v2.Position, v0.Position, px, py);
                    if (w1 < -Epsilon) continue;

                    double w2 = orientation * Edge(v0.Position, v1.Position, px, py);
                    if (w2 < -Epsilon) continue;

                    target[y * Width + x] = PackBlend(v0.Color, v1.Color, v2.Color,
                                                       w0 / totalArea, w1 / totalArea, w2 / totalArea);
                }
            }
        }

        /// <summary>
        /// Функция-полуплоскость для стороны ab и точки p.
        /// Все три функции дают ноль на сторонах и положительны внутри
        /// (после нормализации по знаку обхода вершин).
        /// </summary>
        private static double Edge(Point a, Point b, double px, double py)
            => (b.X - a.X) * (py - a.Y) - (b.Y - a.Y) * (px - a.X);

        /// <summary>Удвоенная ориентированная площадь треугольника.</summary>
        private static double SignedArea(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Edge(v0.Position, v1.Position, v2.Position.X, v2.Position.Y);

        /// <summary>Координата x, где сторона ab пересекает горизонталь py.</summary>
        private static void AccumulateCrossing(Point a, Point b, double py, ref double xMin, ref double xMax)
        {
            double dy = b.Y - a.Y;
            if (Math.Abs(dy) < Epsilon) return; // горизонтальная сторона строку не пересекает

            double px = a.X + (py - a.Y) * (b.X - a.X) / dy;
            if (px < xMin) xMin = px;
            if (px > xMax) xMax = px;
        }

        /// <summary>
        /// Смешивает цвета трёх вершин по барицентрическим весам (затенение Гуро)
        /// и упаковывает результат в AARRGGBB. Канал = l0·канал0 + l1·канал1 + l2·канал2.
        /// </summary>
        private static int PackBlend(Color c0, Color c1, Color c2, double l0, double l1, double l2)
            => unchecked((int)(0xFF000000u
                | ((uint)MixChannel(l0 * c0.R + l1 * c1.R + l2 * c2.R) << 16)
                | ((uint)MixChannel(l0 * c0.G + l1 * c1.G + l2 * c2.G) << 8)
                | MixChannel(l0 * c0.B + l1 * c1.B + l2 * c2.B)));

        /// <summary>Округляет долю цвета до байта 0..255 (с защитным отсечением краёв).</summary>
        private static byte MixChannel(double value)
        {
            if (value <= 0) return 0;
            if (value >= 255) return 255;
            return (byte)(value + 0.5);
        }

        /// <summary>Минимальная X среди трёх вершин — левая граница ограничивающего прямоугольника.</summary>
        private static double MinX(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Min(v0.Position.X, Math.Min(v1.Position.X, v2.Position.X));

        /// <summary>Максимальная X среди трёх вершин — правая граница ограничивающего прямоугольника.</summary>
        private static double MaxX(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Max(v0.Position.X, Math.Max(v1.Position.X, v2.Position.X));

        /// <summary>Минимальная Y среди трёх вершин — верхняя граница ограничивающего прямоугольника.</summary>
        private static double MinY(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Min(v0.Position.Y, Math.Min(v1.Position.Y, v2.Position.Y));

        /// <summary>Максимальная Y среди трёх вершин — нижняя граница ограничивающего прямоугольника.</summary>
        private static double MaxY(GradientVertex v0, GradientVertex v1, GradientVertex v2)
            => Math.Max(v0.Position.Y, Math.Max(v1.Position.Y, v2.Position.Y));

        /// <summary>Ограничивает номер строки диапазоном холста (вершины могли выйти за край).</summary>
        private static int ClampY(int y) => Math.Clamp(y, 0, Height - 1);

        /// <summary>Рисует поверх заливки чёрный контур треугольника и цветные маркеры вершин.</summary>
        private void DrawOverlay(GradientVertex v0, GradientVertex v1, GradientVertex v2)
        {
            int outline = PackColor(Colors.Black);

            DrawLineBresenham((int)v0.Position.X, (int)v0.Position.Y, (int)v1.Position.X, (int)v1.Position.Y, outline);
            DrawLineBresenham((int)v1.Position.X, (int)v1.Position.Y, (int)v2.Position.X, (int)v2.Position.Y, outline);
            DrawLineBresenham((int)v2.Position.X, (int)v2.Position.Y, (int)v0.Position.X, (int)v0.Position.Y, outline);

            DrawVertexMarker(v0);
            DrawVertexMarker(v1);
            DrawVertexMarker(v2);
        }

        /// <summary>Маркер вершины: белый квадрат 7×7 с квадратом 3×3 цвета этой вершины в центре.</summary>
        private void DrawVertexMarker(GradientVertex vertex)
        {
            int x = (int)vertex.Position.X;
            int y = (int)vertex.Position.Y;
            int white = PackColor(Colors.White);
            int tint = PackColor(vertex.Color);

            for (int dy = -3; dy <= 3; dy++)
                for (int dx = -3; dx <= 3; dx++)
                    SetPixel(x + dx, y + dy, white);

            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                    SetPixel(x + dx, y + dy, tint);
        }

        /// <summary>Маркер точки предпросмотра: квадрат 7×7 заданным цветом с белым центром.</summary>
        private void DrawMarker(int x, int y, int argb)
        {
            for (int dy = -3; dy <= 3; dy++)
                for (int dx = -3; dx <= 3; dx++)
                    SetPixel(x + dx, y + dy, argb);

            SetPixel(x, y, PackColor(Colors.White));
        }
    }
}
