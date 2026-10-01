
using LABA_4.Core.Models;
using LABA_4.Core;
using System.Windows;             
using System.Windows.Media;

namespace LABA_4.UI.SceneControls
{
    public class SceneRender
    {
        private readonly Scene _scene;

        public Brush PointBrush { get; set; } = Brushes.Black;
        public Brush EdgeBrush { get; set; } = Brushes.SteelBlue;
        public Brush PolygonBrush { get; set; } = Brushes.DarkGreen;
        public Brush SelectedBrush { get; set; } = Brushes.OrangeRed;
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

        private void DrawPoint(DrawingContext dc, PointPol p,
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

        // рисование не законченной фигуры
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

    }
}
