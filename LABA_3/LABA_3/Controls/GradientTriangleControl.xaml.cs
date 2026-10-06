using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LABA_3.Drawing;

namespace LABA_3.Controls
{
    /// <summary>
    /// Задание 3: градиентное окрашивание треугольника с тремя разноцветными
    /// вершинами. Три клика ставят вершины A, B, C — на третьем треугольник
    /// сразу заливается (алгоритм — в <see cref="GradientCanvas"/>).
    /// </summary>
    public partial class GradientTriangleControl : UserControl
    {
        private static readonly Color[] Palette =
        {
            Colors.Red, Colors.Lime, Colors.Blue,
            Colors.Yellow, Colors.Cyan, Colors.Magenta
        };

        private readonly GradientCanvas _canvas = new();
        private readonly List<GradientVertex> _vertices = new();

        public GradientTriangleControl()
        {
            InitializeComponent();
            CanvasImage.Source = _canvas.Bitmap;

            ColorComboA.ItemsSource = Palette;
            ColorComboB.ItemsSource = Palette;
            ColorComboC.ItemsSource = Palette;
            ColorComboA.SelectedIndex = 0; // красный
            ColorComboB.SelectedIndex = 1; // зелёный
            ColorComboC.SelectedIndex = 2; // синий
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(DrawingCanvas);
            int x = (int)p.X, y = (int)p.Y;
            if (x < 0 || x >= GradientCanvas.Width || y < 0 || y >= GradientCanvas.Height) return;

            // Четвёртый клик начинает новый треугольник.
            if (_vertices.Count == 3) _vertices.Clear();

            _vertices.Add(new GradientVertex(new Point(x, y), VertexColor(_vertices.Count)));
            Render();
        }

        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_vertices.Count == 3) Render(); // цвета поменяли — перезалить готовый треугольник
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _vertices.Clear();
            _canvas.Clear();
            StatusText.Text = "Кликните на холсте, чтобы задать вершину A";
        }

        private void Render()
        {
            SyncVertexColors();

            _canvas.Clear();
            if (_vertices.Count == 3)
            {
                _canvas.RasterizeTriangle(_vertices[0], _vertices[1], _vertices[2]);
                StatusText.Text = "Треугольник залит градиентом. Кликните, чтобы начать новый";
            }
            else
            {
                if (_vertices.Count > 0)
                    _canvas.DrawContour(_vertices.Select(v => v.Position).ToList(), Colors.Black);

                StatusText.Text = _vertices.Count switch
                {
                    1 => "Вершина A задана. Кликните, чтобы задать вершину B",
                    _ => "Заданы вершины A и B. Кликните, чтобы задать вершину C"
                };
            }
        }

        // Цвета ComboBox -> вершины (struct неизменяемый, создаём заново).
        private void SyncVertexColors()
        {
            for (int i = 0; i < _vertices.Count; i++)
                _vertices[i] = new GradientVertex(_vertices[i].Position, VertexColor(i));
        }

        private Color VertexColor(int index) => index switch
        {
            0 => (Color)ColorComboA.SelectedItem,
            1 => (Color)ColorComboB.SelectedItem,
            _ => (Color)ColorComboC.SelectedItem
        };
    }
}
