
using LABA_4.Core.Models;
using LABA_4.Core;
using System.Windows;             
using System.Windows.Media;
using System.Collections.Generic;
using System;

namespace LABA_4.UI.SceneControls
{
    public class SceneRender
    {
        private readonly Scene _scene;

        public Brush PointBrush { get; set; } = Brushes.Black;
        public Brush EdgeBrush { get; set; } = Brushes.SteelBlue;
        public Brush PolygonBrush { get; set; } = Brushes.DarkGreen;
        public Brush SelectedBrush { get; set; } = Brushes.OrangeRed;
        public Brush IntersectionBrush { get; set; } = Brushes.Red;
        public double LineWidth { get; set; } = 2;
        public double PointRadius { get; set; } = 4;

        public SceneRender(Scene scene) { _scene = scene; }

        public void Render(DrawingContext dc)
        {
            foreach (var shape in _scene.Shapes)
            {
                switch (shape)
                {
                    case PointPol p: DrawPoint(dc, p); break;
                    case Edge e: DrawEdge(dc, e); break;
                    case Pol poly: DrawPolygon(dc, poly); break;
                }
            }
        }

        public void DrawPoint(DrawingContext dc, PointPol p,
                               double? radius = null, Brush overrideBrush = null)
        {
            var brush = overrideBrush ?? (p.IsSelected ? SelectedBrush : PointBrush);
            double r = radius ?? PointRadius;
            dc.DrawEllipse(brush, null, new Point(p.X, p.Y), r, r);
        }

        private void DrawEdge(DrawingContext dc, Edge e)
        {
            var brush = e.IsSelected ? SelectedBrush : EdgeBrush;
            var pen = new Pen(brush, LineWidth);

            dc.DrawLine(pen, new Point(e.first.X, e.first.Y),
                             new Point(e.second.X, e.second.Y));

            double r = PointRadius;
            DrawPoint(dc, e.first, r, brush);
            DrawPoint(dc, e.second, r, brush);
        }

        private void DrawPolygon(DrawingContext dc, Pol poly)
        {
            if (poly.Count == 0) return;
            if (poly.Count == 1) { DrawPoint(dc, poly[0]); return; }

            var brush = poly.IsSelected ? SelectedBrush : PolygonBrush;
            var pen = new Pen(brush, LineWidth);

            int last = poly.IsClosed ? poly.Count : poly.Count - 1;
            for (int i = 0; i < last; i++)
            {
                var a = poly[i];
                var b = poly[(i + 1) % poly.Count];
                dc.DrawLine(pen, new Point(a.X, a.Y), new Point(b.X, b.Y));
            }

            double r = PointRadius;
            for (int i = 0; i < poly.Count; i++)
                DrawPoint(dc, poly[i], r, brush);
        }

        public void RenderPreview(DrawingContext dc, IEnumerable<PointPol> points)
        {
            var previewBrush = Brushes.Gray;
            var previewPen = new Pen(previewBrush, LineWidth) { DashStyle = DashStyles.Dash };

            var list = points.ToList();
            if (list.Count == 0) return;

            foreach (var p in list)
                DrawPoint(dc, p, PointRadius, previewBrush);

            for (int i = 0; i < list.Count - 1; i++)
                dc.DrawLine(previewPen,
                    new Point(list[i].X, list[i].Y),
                    new Point(list[i + 1].X, list[i + 1].Y));
        }

        public void RenderIntersections(DrawingContext dc, IEnumerable<PointPol> points)
        {
            var brush = IntersectionBrush;
            var pen = new Pen(brush, 3);
            double r = PointRadius + 2;

            foreach (var p in points)
            {
                dc.DrawEllipse(Brushes.Yellow, pen, new Point(p.X, p.Y), r, r);
                DrawPoint(dc, p, r - 1, brush);
            }
        }

        public void RenderPoint(DrawingContext dc, PointPol p, Brush brush, double radius)
        {
            var pen = new Pen(brush, 2);
            dc.DrawEllipse(Brushes.White, pen, new Point(p.X, p.Y), radius, radius);
            DrawPoint(dc, p, radius - 1, brush);
        }

        public void RenderSelectedPolygonEdge(DrawingContext dc, Pol poly, int edgeIndex)
        {
            if (poly.Count == 0 || edgeIndex < 0) return;
            
            int last = poly.IsClosed ? poly.Count : poly.Count - 1;
            if (edgeIndex >= last) return;

            var a = poly[edgeIndex];
            var b = poly[(edgeIndex + 1) % poly.Count];

            var highlightPen = new Pen(Brushes.OrangeRed, LineWidth + 2);
            dc.DrawLine(highlightPen, new Point(a.X, a.Y), new Point(b.X, b.Y));

            double r = PointRadius + 2;
            DrawPoint(dc, a, r, Brushes.OrangeRed);
            DrawPoint(dc, b, r, Brushes.OrangeRed);

            RenderEdgeDirectionArrow(dc, a, b);
        }

        public void RenderEdgeDirectionArrow(DrawingContext dc, PointPol a, PointPol b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            
            if (len < 1) return;

            // Normalize
            double ux = dx / len;
            double uy = dy / len;

            // Midpoint
            double mx = (a.X + b.X) / 2;
            double my = (a.Y + b.Y) / 2;

            // Arrow size
            double arrowSize = 12;
            double arrowWidth = 6;

            // Arrow tip at midpoint, pointing along the edge
            // Base points perpendicular to direction
            double bx1 = mx - ux * arrowSize + uy * arrowWidth;
            double by1 = my - uy * arrowSize - ux * arrowWidth;
            double bx2 = mx - ux * arrowSize - uy * arrowWidth;
            double by2 = my - uy * arrowSize + ux * arrowWidth;

            var arrowBrush = Brushes.OrangeRed;
            var arrowPen = new Pen(arrowBrush, 2);

            // Draw filled triangle arrow
            var geometry = new System.Windows.Media.StreamGeometry();
            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(new Point(mx, my), true, true);
                ctx.LineTo(new Point(bx1, by1), true, false);
                ctx.LineTo(new Point(bx2, by2), true, false);
            }
            geometry.Freeze();

            dc.DrawGeometry(arrowBrush, arrowPen, geometry);
        }
    }
}
