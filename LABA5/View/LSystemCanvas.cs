using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using BezierSpline.Math;

namespace BezierSpline.Views
{
    /// <summary>
    /// Холст для отрисовки отрезков L-системы.
    /// В режиме дерева толщина и цвет зависят от глубины ветвления:
    /// от толстого коричневого ствола к тонким зелёным ветвям.
    /// </summary>
    public sealed class LSystemCanvas : FrameworkElement
    {
        private static readonly Color TreeTrunk = Color.FromRgb(0x8B, 0x45, 0x13);
        private static readonly Color TreeLeaf = Color.FromRgb(0x22, 0x8B, 0x22);
        private const double BaseThickness = 8.0;
        private const double ThicknessDecay = 0.7;

        private readonly Dictionary<int, Pen> _pens = new Dictionary<int, Pen>();
        private List<LSystemSegment> _segments = new List<LSystemSegment>();
        private bool _treeMode;
        private int _maxDepth;

        public void SetSegments(IEnumerable<LSystemSegment> segments, bool treeMode)
        {
            _segments = new List<LSystemSegment>(segments);
            _treeMode = treeMode;
            _maxDepth = 0;
            foreach (var s in _segments)
                _maxDepth = System.Math.Max(_maxDepth, s.Depth);
            _pens.Clear();
            InvalidateVisual();
        }

        public void Clear()
        {
            _segments.Clear();
            _pens.Clear();
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, ActualWidth, ActualHeight));

            foreach (var s in _segments)
                dc.DrawLine(GetPen(s.Depth), new Point(s.X1, s.Y1), new Point(s.X2, s.Y2));
        }

        private Pen GetPen(int depth)
        {
            if (!_pens.TryGetValue(depth, out var pen))
            {
                double thickness = 1.0;
                Brush brush = Brushes.DarkSlateGray;
                if (_treeMode)
                {
                    double t = _maxDepth > 0 ? (double)depth / _maxDepth : 0;
                    brush = new SolidColorBrush(Lerp(TreeTrunk, TreeLeaf, t));
                    thickness = System.Math.Max(1.0, BaseThickness * System.Math.Pow(ThicknessDecay, depth));
                }

                pen = new Pen(brush, thickness)
                {
                    LineJoin = PenLineJoin.Round,
                    StartLineCap = PenLineCap.Round,
                    EndLineCap = PenLineCap.Round
                };
                pen.Freeze();
                _pens[depth] = pen;
            }
            return pen;
        }

        private static Color Lerp(Color a, Color b, double t)
        {
            t = System.Math.Max(0.0, System.Math.Min(1.0, t));
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t));
        }
    }
}