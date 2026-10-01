using LABA_4.Core.Models;
using System;

namespace LABA_4.Core.Heometric
{
    public static class HeometricHelpers
    {
        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1, dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double DistanceToSegment(double px, double py,
            double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len2 = dx * dx + dy * dy;

            if (len2 < 1e-9)                          
                return Distance(px, py, x1, y1);

            double t = ((px - x1) * dx + (py - y1) * dy) / len2;
            t = Math.Max(0, Math.Min(1, t));          

            return Distance(px, py, x1 + t * dx, y1 + t * dy);
        }

        public static bool IsNearBorder(double x, double y, Pol poly, double threshold)
        {
            int n = poly.Count;
            if (n < 2) return false;

            int last = poly.IsClosed ? n : n - 1;
            for (int i = 0; i < last; i++)
            {
                var a = poly[i];
                var b = poly[(i + 1) % n];           
                if (DistanceToSegment(x, y, a.X, a.Y, b.X, b.Y) <= threshold)
                    return true;
            }
            return false;
        }

        public static bool IsInside(double x, double y, Pol poly)
        {
            if (!poly.IsClosed || poly.Count < 3) return false;

            bool inside = false;
            int n = poly.Count;

            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = poly[i].X, yi = poly[i].Y;
                double xj = poly[j].X, yj = poly[j].Y;

                bool intersect = ((yi > y) != (yj > y))
                    && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);

                if (intersect) inside = !inside;       
            }
            return inside;
        }
    }
}