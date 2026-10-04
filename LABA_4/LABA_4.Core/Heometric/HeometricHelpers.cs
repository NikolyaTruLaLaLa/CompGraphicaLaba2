using LABA_4.Core.Models;
using System;
using System.Collections.Generic;

namespace LABA_4.Core.Heometric
{
    public enum PointPosition
    {
        Inside,
        Outside,
        OnBorder
    }

    public enum RelativePosition
    {
        Left,
        Right,
        OnLine
    }

    public enum SegmentPosition
    {
        Left,
        Right,
        OnLineExtended,
        OnSegment
    }

    public struct SegmentIntersectionResult
    {
        public enum Type { None, Point, Segment }
        public Type IntersectionType;
        public PointPol Point1;
        public PointPol Point2;
        public bool IsParallel;
        public bool IsCollinear;

        public static SegmentIntersectionResult NoIntersection(bool parallel, bool collinear)
            => new() { IntersectionType = Type.None, IsParallel = parallel, IsCollinear = collinear };

        public static SegmentIntersectionResult SinglePoint(PointPol p)
            => new() { IntersectionType = Type.Point, Point1 = p };

        public static SegmentIntersectionResult OverlappingSegment(PointPol p1, PointPol p2)
            => new() { IntersectionType = Type.Segment, Point1 = p1, Point2 = p2 };
    }

    public static class HeometricHelpers
    {
        private const double Eps = 1e-9;

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1, dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double DistanceToSegment(double px, double py, double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len2 = dx * dx + dy * dy;

            if (len2 < Eps)                          
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

        public static PointPosition PointInPolygon(double x, double y, Pol poly, double borderThreshold = 5.0)
        {
            if (!poly.IsClosed || poly.Count < 3) return PointPosition.Outside;

            if (IsNearBorder(x, y, poly, borderThreshold))
                return PointPosition.OnBorder;

            return IsInside(x, y, poly) ? PointPosition.Inside : PointPosition.Outside;
        }

        public static double Cross(double ax, double ay, double bx, double by) => ax * by - ay * bx;

        public static RelativePosition PointRelativeToEdge(double px, double py, double x1, double y1, double x2, double y2)
        {
            double cross = Cross(x2 - x1, y2 - y1, px - x1, py - y1);
            
            if (Math.Abs(cross) < Eps)
                return RelativePosition.OnLine;
            
            return cross > 0 ? RelativePosition.Right : RelativePosition.Left;
        }

        public static RelativePosition PointRelativeToEdge(PointPol p, Edge edge) 
            => PointRelativeToEdge(p.X, p.Y, edge.first.X, edge.first.Y, edge.second.X, edge.second.Y);

        public static RelativePosition PointRelativeToEdge(PointPol p, PointPol a, PointPol b)
            => PointRelativeToEdge(p.X, p.Y, a.X, a.Y, b.X, b.Y);

        public static SegmentPosition PointRelativeToSegment(double px, double py, double x1, double y1, double x2, double y2, double threshold = 5.0)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len2 = dx * dx + dy * dy;

            if (len2 < Eps)
            {
                double dist = Distance(px, py, x1, y1);
                return dist <= threshold ? SegmentPosition.OnSegment : SegmentPosition.OnLineExtended;
            }

            double cross = Cross(dx, dy, px - x1, py - y1);
            double dot = (px - x1) * dx + (py - y1) * dy;

            if (Math.Abs(cross) <= threshold * Math.Sqrt(len2))
            {
                if (dot >= -threshold * Math.Sqrt(len2) && dot <= len2 + threshold * Math.Sqrt(len2))
                    return SegmentPosition.OnSegment;
                return SegmentPosition.OnLineExtended;
            }

            return cross > 0 ? SegmentPosition.Right : SegmentPosition.Left;
        }

        public static SegmentPosition PointRelativeToSegment(PointPol p, Edge edge, double threshold = 5.0)
            => PointRelativeToSegment(p.X, p.Y, edge.first.X, edge.first.Y, edge.second.X, edge.second.Y, threshold);

        public static SegmentPosition PointRelativeToSegment(PointPol p, PointPol a, PointPol b, double threshold = 5.0)
            => PointRelativeToSegment(p.X, p.Y, a.X, a.Y, b.X, b.Y, threshold);

        public static SegmentIntersectionResult IntersectSegments(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            double dx1 = x2 - x1, dy1 = y2 - y1;
            double dx2 = x4 - x3, dy2 = y4 - y3;
            
            double cross = Cross(dx1, dy1, dx2, dy2);
            
            if (Math.Abs(cross) < Eps)
            {
                double cross2 = Cross(x3 - x1, y3 - y1, dx1, dy1);
                
                if (Math.Abs(cross2) < Eps)
                {
                    var result = CollinearOverlap(x1, y1, x2, y2, x3, y3, x4, y4);
                    return result with { IsParallel = true, IsCollinear = true };
                }
                
                return SegmentIntersectionResult.NoIntersection(parallel: true, collinear: false);
            }
            
            double t = Cross(x3 - x1, y3 - y1, dx2, dy2) / cross;
            double u = Cross(x3 - x1, y3 - y1, dx1, dy1) / cross;
            
            if (t >= -Eps && t <= 1 + Eps && u >= -Eps && u <= 1 + Eps)
            {
                double ix = x1 + t * dx1;
                double iy = y1 + t * dy1;
                return SegmentIntersectionResult.SinglePoint(new PointPol(ix, iy));
            }
            
            return SegmentIntersectionResult.NoIntersection(parallel: false, collinear: false);
        }

        public static SegmentIntersectionResult IntersectSegments(Edge e1, Edge e2)
            => IntersectSegments(e1.first.X, e1.first.Y, e1.second.X, e1.second.Y,
                                 e2.first.X, e2.first.Y, e2.second.X, e2.second.Y);

        public static SegmentIntersectionResult IntersectSegments(PointPol a1, PointPol a2, PointPol b1, PointPol b2)
            => IntersectSegments(a1.X, a1.Y, a2.X, a2.Y, b1.X, b1.Y, b2.X, b2.Y);

        private static SegmentIntersectionResult CollinearOverlap(
            double x1, double y1, double x2, double y2,
            double x3, double y3, double x4, double y4)
        {
            double dx = x2 - x1, dy = y2 - y1;
            double len2 = dx * dx + dy * dy;
            
            if (len2 < Eps)
            {
                if (Distance(x1, y1, x3, y3) < Eps)
                    return SegmentIntersectionResult.SinglePoint(new PointPol(x1, y1));
                return SegmentIntersectionResult.NoIntersection(parallel: true, collinear: true);
            }
            
            double t0 = ((x3 - x1) * dx + (y3 - y1) * dy) / len2;
            double t1 = ((x4 - x1) * dx + (y4 - y1) * dy) / len2;
            
            double tMin = Math.Max(0, Math.Min(t0, t1));
            double tMax = Math.Min(1, Math.Max(t0, t1));
            
            if (tMin > tMax + Eps)
                return SegmentIntersectionResult.NoIntersection(parallel: true, collinear: true);
            
            if (Math.Abs(tMin - tMax) < Eps)
            {
                double ix = x1 + tMin * dx;
                double iy = y1 + tMin * dy;
                return SegmentIntersectionResult.SinglePoint(new PointPol(ix, iy));
            }
            
            var p1 = new PointPol(x1 + tMin * dx, y1 + tMin * dy);
            var p2 = new PointPol(x1 + tMax * dx, y1 + tMax * dy);
            return SegmentIntersectionResult.OverlappingSegment(p1, p2);
        }

        public static List<PointPol> FindAllIntersections(Scene scene)
        {
            var intersections = new List<PointPol>();
            var shapes = scene.Shapes;
            
            var edges = new List<(PointPol a, PointPol b)>();
            
            foreach (var shape in shapes)
            {
                switch (shape)
                {
                    case Edge e:
                        edges.Add((e.first, e.second));
                        break;
                    case Pol poly:
                        int last = poly.IsClosed ? poly.Count : poly.Count - 1;
                        for (int i = 0; i < last; i++)
                            edges.Add((poly[i], poly[(i + 1) % poly.Count]));
                        break;
                }
            }
            
            for (int i = 0; i < edges.Count; i++)
            {
                for (int j = i + 1; j < edges.Count; j++)
                {
                    var result = IntersectSegments(edges[i].a, edges[i].b, edges[j].a, edges[j].b);
                    
                    if (result.IntersectionType == SegmentIntersectionResult.Type.Point)
                    {
                        intersections.Add(result.Point1);
                    }
                    else if (result.IntersectionType == SegmentIntersectionResult.Type.Segment)
                    {
                        intersections.Add(result.Point1);
                        intersections.Add(result.Point2);
                    }
                }
            }
            
            return intersections;
        }
    }
}