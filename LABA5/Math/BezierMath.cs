using System.Collections.Generic;
using System.Windows;

namespace BezierSpline.Math
{
    /// <summary>
    /// Чистая математика по кубическим кривым Безье.
    /// Никакого UI — можно тестировать юнит-тестами.
    /// </summary>
    public static class BezierMath
    {
        /// <summary>
        /// Точка на кубической кривой Безье в параметре t ∈ [0,1]:
        /// B(t) = (1-t)^3·P0 + 3(1-t)^2·t·P1 + 3(1-t)·t^2·P2 + t^3·P3
        /// </summary>
        public static Point Cubic(Point p0, Point p1, Point p2, Point p3, double t)
        {
            double u = 1.0 - t;
            double u2 = u * u;
            double t2 = t * t;

            double b0 = u2 * u;
            double b1 = 3 * u2 * t;
            double b2 = 3 * u * t2;
            double b3 = t2 * t;

            return new Point(
                b0 * p0.X + b1 * p1.X + b2 * p2.X + b3 * p3.X,
                b0 * p0.Y + b1 * p1.Y + b2 * p2.Y + b3 * p3.Y);
        }

        /// <summary>
        /// Сэмплирование сегмента: возвращает (steps + 1) точку вдоль кривой.
        /// </summary>
        public static IEnumerable<Point> SampleCubic(
            Point p0, Point p1, Point p2, Point p3, int steps)
        {
            if (steps < 1) steps = 1;
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                yield return Cubic(p0, p1, p2, p3, t);
            }
        }

        /// <summary>
        /// Сколько полных кубических сегментов даёт N опорных точек.
        /// Сегмент s использует точки: P[3s], P[3s+1], P[3s+2], P[3s+3].
        /// </summary>
        public static int SegmentCount(int pointCount)
        {
            return pointCount >= 4 ? (pointCount - 1) / 3 : 0;
        }
    }
}