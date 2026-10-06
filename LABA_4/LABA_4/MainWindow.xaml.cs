using LABA_4.Core;
using LABA_4.Core.Heometric;
using LABA_4.Core.Matrix;
using LABA_4.Core.Models;
using LABA_4.UI.SceneControls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static LABA_4.Core.Heometric.HeometricHelpers;

namespace LABA_4
{
    public partial class MainWindow : Window
    {
        private readonly Scene _scene = new();

        public MainWindow()
        {
            InitializeComponent();
            SceneView.Scene = _scene;
            SceneView.Editor = new SceneEditor(_scene);
            SceneView.CheckStateChanged += EvaluateChecks;
            UpdateTransformFieldsVisibility();
            UpdateStatusText();
            SceneView.Redraw();
        }

        private void DrawMode_Changed(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;

            bool isDrawing = btnDrawMode.IsChecked == true;
            SceneView.Editor.SetMode(isDrawing ? EditorMode.Draw : EditorMode.None);

            if (!isDrawing)
                SceneView.Editor.Cancel();

            UpdateStatusText();
            SceneView.Redraw();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _scene.Clear();
            SceneView.Editor.ClearAll();
            tbPointInPolyResult.Text = "";
            tbIntersectionCount.Text = "";
            tbPointEdgeResult.Text = "";
            SceneView.Redraw();
        }

        private void Transform_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => UpdateTransformFieldsVisibility();

        private void UpdateTransformFieldsVisibility()
        {
            if (cmbTransform == null || pDx == null || pDy == null || pAngle == null
                || pSx == null || pSy == null || pPx == null || pPy == null)
                return;

            int mode = cmbTransform.SelectedIndex;
            bool showShift = mode == 0;
            bool showAngle = mode == 1 || mode == 2;
            bool showScale = mode == 3 || mode == 4;
            bool showPivot = mode == 1 || mode == 3;

            pDx.Visibility = pDy.Visibility = showShift ? Visibility.Visible : Visibility.Collapsed;
            pAngle.Visibility = showAngle ? Visibility.Visible : Visibility.Collapsed;
            pSx.Visibility = pSy.Visibility = showScale ? Visibility.Visible : Visibility.Collapsed;
            pPx.Visibility = pPy.Visibility = showPivot ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool TryParseParam(TextBox tb, string name, out double value)
        {
            if (double.TryParse(tb.Text, out value))
                return true;

            MessageBox.Show($"Некорректный параметр «{name}».", "Преобразование",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void ApplyTransform_Click(object sender, RoutedEventArgs e)
        {
            var selected = SceneView.Editor?.Selected;
            if (selected == null)
            {
                MessageBox.Show("Сначала выделите фигуру.", "Преобразование",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            switch (cmbTransform.SelectedIndex)
            {
                case 0: // Смещение на dx, dy
                    if (!TryParseParam(tbDx, "dx", out double dx)
                        || !TryParseParam(tbDy, "dy", out double dy))
                        return;
                    selected.Translate(dx, dy);
                    break;

                case 1: // Поворот вокруг заданной точки
                    if (!TryParseParam(tbAngle, "угол", out double angleDeg)
                        || !TryParseParam(tbPx, "px", out double rpx)
                        || !TryParseParam(tbPy, "py", out double rpy))
                        return;
                    selected.RotateDeg(angleDeg, rpx, rpy);
                    break;

                case 2: // Поворот вокруг своего центра
                    if (!TryParseParam(tbAngle, "угол", out double centerAngleDeg))
                        return;
                    selected.RotateAroundCenterDeg(centerAngleDeg);
                    break;

                case 3: // Масштабирование относительно заданной точки
                    if (!TryParseParam(tbSx, "sx", out double sx)
                        || !TryParseParam(tbSy, "sy", out double sy)
                        || !TryParseParam(tbPx, "px", out double spx)
                        || !TryParseParam(tbPy, "py", out double spy))
                        return;
                    selected.ScaleAround(sx, sy, spx, spy);
                    break;

                case 4: // Масштабирование относительно своего центра
                    if (!TryParseParam(tbSx, "sx", out double csx)
                        || !TryParseParam(tbSy, "sy", out double csy))
                        return;
                    selected.ScaleAroundCenter(csx, csy);
                    break;

                default:
                    return;
            }

            SceneView.Redraw();
        }

        private void PointInPolyMode_Changed(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;

            bool enabled = btnPointInPolyMode.IsChecked == true;
            SceneView.Editor.SetMode(enabled ? EditorMode.PointInPolygon : EditorMode.None);
            
            if (!enabled)
            {
                SceneView.Editor.ResetPointInPolygonCheck();
                tbPointInPolyResult.Text = "";
                UpdateStatusText();
            }
            else
            {
                tbPointInPolyResult.Text = "";
                tbStatusText.Text = "Режим: Точка в полигоне  |  Shift+ЛКМ — выбрать полигон, ЛКМ — проверить точку";
            }
            
            SceneView.Redraw();
        }

        private void FindIntersections_Click(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;

            var intersections = FindAllIntersections(_scene);
            SceneView.Editor.IntersectionPoints.Clear();
            SceneView.Editor.IntersectionPoints.AddRange(intersections);
            SceneView.Editor.ShowIntersections = true;
            
            tbIntersectionCount.Text = $"Найдено точек пересечения: {intersections.Count}";
            tbIntersectionCount.Foreground = intersections.Count > 0 ? Brushes.Green : Brushes.Red;
            tbStatusText.Text = "Пересечения найдены. Нажмите кнопку повторно, чтобы скрыть.";
            SceneView.Redraw();
        }

        private void FindIntersections_Unchecked(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;
            
            SceneView.Editor.ShowIntersections = false;
            tbIntersectionCount.Text = "";
            UpdateStatusText();
            SceneView.Redraw();
        }

        private void PointEdgeMode_Changed(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;

            bool enabled = btnPointEdgeMode.IsChecked == true;
            SceneView.Editor.SetMode(enabled ? EditorMode.PointRelativeToEdge : EditorMode.None);
            
            if (!enabled)
            {
                SceneView.Editor.ClearCheckStates();
                tbPointEdgeResult.Text = "";
                UpdateStatusText();
            }
            else
            {
                tbPointEdgeResult.Text = "";
                tbStatusText.Text = "Режим: Точка относительно ребра  |  Shift+ЛКМ — выбрать ребро/полигон, ЛКМ — проверить точку";
            }
            
            SceneView.Redraw();
        }

        private void UpdateStatusText()
        {
            var editor = SceneView?.Editor;
            if (editor == null) return;

            if (editor.IsPointInPolygonMode)
                tbStatusText.Text = "Режим: Точка в полигоне  |  Shift+ЛКМ — выбрать полигон, ЛКМ — проверить точку";
            else if (editor.IsPointRelativeToEdgeMode)
                tbStatusText.Text = "Режим: Точка относительно ребра  |  Shift+ЛКМ — выбрать ребро/полигон, ЛКМ — проверить точку";
            else if (editor.IsDrawing)
                tbStatusText.Text = "Режим рисования  |  ЛКМ — добавить точку, ПКМ — завершить";
            else
                tbStatusText.Text = "Shift+ЛКМ — выбрать объект, ЛКМ — проверить точку";
        }


        private void EvaluateChecks()
        {
            var editor = SceneView.Editor;
            if (editor == null) return;

            if (editor.IsPointInPolygonMode)
            {
                if (editor.TestPoint != null && editor.SelectedPolygonForCheck != null)
                {
                    var result = PointInPolygon(
                        editor.TestPoint.X,
                        editor.TestPoint.Y,
                        editor.SelectedPolygonForCheck,
                        5.0);

                    tbPointInPolyResult.Text = result switch
                    {
                        PointPosition.Inside => "ВНУТРИ",
                        PointPosition.Outside => "СНАРУЖИ",
                        PointPosition.OnBorder => "НА ГРАНИЦЕ",
                        _ => ""
                    };
                    tbPointInPolyResult.Foreground = result switch
                    {
                        PointPosition.Inside => Brushes.Green,
                        PointPosition.Outside => Brushes.Red,
                        PointPosition.OnBorder => Brushes.Orange,
                        _ => Brushes.Gray
                    };
                }
                else
                {
                    tbPointInPolyResult.Text = "";
                }
                SceneView.Redraw();
            }
            else if (editor.IsPointRelativeToEdgeMode)
            {
                if (editor.TestPoint != null && editor.SelectedEdgeForCheck != null)
                {
                    var result = PointRelativeToSegment(
                        editor.TestPoint,
                        editor.SelectedEdgeForCheck,
                        5.0);

                    tbPointEdgeResult.Text = result switch
                    {
                        SegmentPosition.OnSegment => "НА ОТРЕЗКЕ",
                        SegmentPosition.OnLineExtended => "НА ПРЯМОЙ",
                        SegmentPosition.Left => "СЛЕВА",
                        SegmentPosition.Right => "СПРАВА",
                        _ => ""
                    };
                    tbPointEdgeResult.Foreground = result switch
                    {
                        SegmentPosition.OnSegment => Brushes.Green,
                        SegmentPosition.OnLineExtended => Brushes.Orange,
                        SegmentPosition.Left => Brushes.Blue,
                        SegmentPosition.Right => Brushes.Red,
                        _ => Brushes.Gray
                    };
                }
                else
                {
                    tbPointEdgeResult.Text = "";
                }
                SceneView.Redraw();
            }
        }
    }
}