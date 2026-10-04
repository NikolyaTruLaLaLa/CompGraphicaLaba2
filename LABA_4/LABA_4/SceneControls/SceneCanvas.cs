using LABA_4.Core;
using LABA_4.Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LABA_4.UI.SceneControls
{
    public class SceneCanvas : FrameworkElement
    {
        private Scene _scene;
        private SceneRender _renderer;
        private Point? _mousePos;
        public SceneEditor Editor { get; set; }
        public Scene Scene
        {
            get => _scene;
            set
            {
                _scene = value;
                _renderer = _scene != null ? new SceneRender(_scene) : null;
                InvalidateVisual();
            }
        }

        public SceneCanvas()
        {
            Focusable = true;
            ClipToBounds = true;
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.White, null,
                new Rect(0, 0, ActualWidth, ActualHeight));

            if (_renderer == null) return;

            _renderer.Render(dc);

            if (Editor != null && Editor.Pending.Count > 0)
                _renderer.RenderPreview(dc, Editor.Pending);

            if (Editor != null && Editor.IntersectionPoints.Count > 0 && Editor.ShowIntersections)
                _renderer.RenderIntersections(dc, Editor.IntersectionPoints);

            if (Editor != null && Editor.SelectedEdgeForCheck != null && Editor.SelectedPolygonForCheck != null && Editor.SelectedEdgeIndex >= 0)
                _renderer.RenderSelectedPolygonEdge(dc, Editor.SelectedPolygonForCheck, Editor.SelectedEdgeIndex);

            if (Editor != null && Editor.SelectedEdgeForCheck != null && Editor.SelectedPolygonForCheck == null)
            {
                var edge = Editor.SelectedEdgeForCheck;
                _renderer.RenderEdgeDirectionArrow(dc, edge.first, edge.second);
            }

            if (Editor != null && Editor.TestPoints.Count > 0)
            {
                foreach (var tp in Editor.TestPoints)
                    _renderer.RenderPoint(dc, tp, Brushes.Purple, 6);
            }

            if (Editor != null && Editor.FirstPointForEdgeCheck != null)
                _renderer.RenderPoint(dc, Editor.FirstPointForEdgeCheck, Brushes.Purple, 8);

            DrawCursorCoords(dc);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _mousePos = e.GetPosition(this);
            InvalidateVisual();        
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _mousePos = null;          
            InvalidateVisual();
            base.OnMouseLeave(e);
        }
        
        private void DrawCursorCoords(DrawingContext dc)
        {
            if (_mousePos == null) return;

            var text = string.Format(CultureInfo.InvariantCulture,
                "X: {0:F1}   Y: {1:F1}", _mousePos.Value.X, _mousePos.Value.Y);

            var formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                13,
                Brushes.DimGray,
                96);   

            var pos = new Point(8, ActualHeight - formatted.Height - 8);

            var bgRect = new Rect(pos.X - 4, pos.Y - 2,
                                  formatted.Width + 8,
                                  formatted.Height + 4);

            dc.DrawRectangle(
                new SolidColorBrush(Color.FromArgb(180, 255, 255, 255)),
                new Pen(Brushes.LightGray, 1),
                bgRect);

            dc.DrawText(formatted, pos);
        }

        public void Redraw() => InvalidateVisual();

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (Editor == null) return;

            Focus();
            var pos = e.GetPosition(this);
            bool shiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

            if (e.ChangedButton == MouseButton.Left)
            {
                if (Editor.IsDrawing)
                {
                    Editor.AddPoint(pos.X, pos.Y);
                }
                else if (Editor.IsPointInPolygonMode)
                {
                    if (shiftPressed)
                    {
                        // Shift+LMB: select polygon for checking
                        Editor.HandlePointInPolygonSelect(pos.X, pos.Y);
                        Editor.ClearTestPoint(); // Clear test points when selecting new polygon
                    }
                    else
                    {
                        // LMB: test point against selected polygon
                        Editor.HandlePointInPolygonTest(pos.X, pos.Y);
                    }
                }
                else if (Editor.IsPointRelativeToEdgeMode)
                {
                    if (shiftPressed)
                    {
                        // Shift+LMB: select edge for checking
                        Editor.HandlePointEdgeSelect(pos.X, pos.Y);
                        Editor.ClearTestPoint(); // Clear test points when selecting new edge
                    }
                    else
                    {
                        // LMB: test point against selected edge
                        Editor.HandlePointEdgeTest(pos.X, pos.Y);
                    }
                }
                else
                {
                    Editor.Select(pos.X, pos.Y);
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (Editor.IsDrawing)
                    Editor.Finish();
            }

            InvalidateVisual();
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Editor == null) return;

            if (e.Key == Key.Escape)
            {
                Editor.Cancel();
                InvalidateVisual();
            }
        }

    }
}