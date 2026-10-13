using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BezierSpline.Math;

namespace BezierSpline.Views
{
    public partial class BezierSplineView : UserControl
    {
        private readonly List<Point> points = new List<Point>();
        private int draggingIndex = -1;

        private const double HitRadius = 10.0;
        private const int CurveSteps = 120;

        public BezierSplineView()
        {
            InitializeComponent();
            SeedDemoPoints();
            Redraw();
        }

        private void SeedDemoPoints()
        {
            points.Add(new Point(100, 350));
            points.Add(new Point(200, 120));
            points.Add(new Point(320, 480));
            points.Add(new Point(430, 200));
            points.Add(new Point(540, 420));
            points.Add(new Point(650, 150));
            points.Add(new Point(760, 380));
        }

        // ---------- Отрисовка ----------

        private void Redraw()
        {
            canvas.Children.Clear();
            DrawControlPolygon();
            DrawBezierSegments();
            DrawPoints();
        }

        private void DrawControlPolygon()
        {
            for (int i = 0; i + 1 < points.Count; i++)
            {
                canvas.Children.Add(new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 3 }
                });
            }
        }

        private void DrawBezierSegments()
        {
            int segments = BezierMath.SegmentCount(points.Count);

            for (int s = 0; s < segments; s++)
            {
                int i0 = s * 3;
                var poly = new Polyline
                {
                    Stroke = Brushes.RoyalBlue,
                    StrokeThickness = 2.5,
                    StrokeLineJoin = PenLineJoin.Round
                };

                foreach (var p in BezierMath.SampleCubic(
                             points[i0], points[i0 + 1],
                             points[i0 + 2], points[i0 + 3],
                             CurveSteps))
                {
                    poly.Points.Add(p);
                }
                canvas.Children.Add(poly);
            }
        }

        private void DrawPoints()
        {
            for (int i = 0; i < points.Count; i++)
            {
                var dot = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = Brushes.Tomato,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1.5
                };
                Canvas.SetLeft(dot, points[i].X - 6);
                Canvas.SetTop(dot, points[i].Y - 6);
                canvas.Children.Add(dot);

                var label = new TextBlock
                {
                    Text = i.ToString(),
                    FontSize = 10,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(label, points[i].X + 8);
                Canvas.SetTop(label, points[i].Y - 16);
                canvas.Children.Add(label);
            }
        }

        // ---------- Мышь ----------

        private int HitTest(Point p)
        {
            for (int i = points.Count - 1; i >= 0; i--)
            {
                if ((points[i] - p).Length <= HitRadius) return i;
            }
            return -1;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(canvas);
            int idx = HitTest(pos);
            if (idx < 0) return;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                points.RemoveAt(idx);
                Redraw();
                return;
            }

            draggingIndex = idx;
            canvas.CaptureMouse();
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggingIndex >= 0)
            {
                draggingIndex = -1;
                canvas.ReleaseMouseCapture();
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (draggingIndex < 0) return;
            points[draggingIndex] = e.GetPosition(canvas);
            Redraw();
        }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(canvas);
            int idx = HitTest(pos);

            if (idx >= 0) points.RemoveAt(idx);
            else points.Add(pos);

            Redraw();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            Redraw();
        }
    }
}