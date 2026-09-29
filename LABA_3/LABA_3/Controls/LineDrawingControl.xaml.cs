using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LABA_3.Drawing;

namespace LABA_3.Controls
{
    public partial class LineDrawingControl : UserControl
    {
        private readonly LineCanvas _canvas = new();
        private Point? _firstPoint;

        public LineDrawingControl()
        {
            InitializeComponent();
            CanvasImage.Source = _canvas.Bitmap;
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DrawingCanvas);
            int x = (int)p.X;
            int y = (int)p.Y;

            if (x < 0 || x >= LineCanvas.Width || y < 0 || y >= LineCanvas.Height) return;

            if (_firstPoint == null)
            {
                _firstPoint = new Point(x, y);
                StatusText.Text = $"Начало: ({x}, {y}). Кликните для конца";
            }
            else
            {
                int x0 = (int)_firstPoint.Value.X;
                int y0 = (int)_firstPoint.Value.Y;
                int x1 = x;
                int y1 = y;

                string algo = ((ComboBoxItem)AlgorithmCombo.SelectedItem).Content.ToString();

                if (algo == "Брезенхем")
                    _canvas.DrawBresenham(x0, y0, x1, y1, Colors.Black);
                else
                    _canvas.DrawWu(x0, y0, x1, y1);

                StatusText.Text = $"Отрезок: ({x0},{y0})-({x1},{y1}) [{algo}]";
                _firstPoint = null;
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _canvas.Clear();
            _firstPoint = null;
            StatusText.Text = "Кликните на холсте для начала отрезка";
        }
    }
}