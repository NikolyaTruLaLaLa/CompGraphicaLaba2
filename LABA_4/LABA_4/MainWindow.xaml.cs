using LABA_4.Core;
using LABA_4.UI.SceneControls;
using LABA_4.Core.Models;
using System.Linq;
using System.Reflection;
using System.Windows;
using System;
using  static LABA_4.Core.Heometric.HeometricHelpers;
using LABA_4.Core.Matrix;

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
            SceneView.Redraw();
        }

        private void DrawMode_Changed(object sender, RoutedEventArgs e)
        {
            if (SceneView?.Editor == null) return;

            SceneView.Editor.IsDrawing = btnDrawMode.IsChecked == true;

            if (!SceneView.Editor.IsDrawing)
                SceneView.Editor.Cancel();

            SceneView.Redraw();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            _scene.Clear();
            SceneView.Redraw();
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

            if (!double.TryParse(tbAngle.Text, out double angleDeg))
            {
                MessageBox.Show("Некорректный угол.", "Преобразование",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            var (cx, cy) = selected.GetCenter();
            var m = MatrixAffine.RotationAroundDeg(angleDeg, cx, cy);

            selected.ApplyMatrix(m);
            SceneView.Redraw();
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            var selected = SceneView.Editor?.Selected;
            if (selected == null)
            {
                tbCheckResult.Text = "Выделите фигуру";
                tbCheckResult.Foreground = System.Windows.Media.Brushes.Gray;
                return;
            }

            if (!double.TryParse(tbCheckX.Text, out double x) ||
                !double.TryParse(tbCheckY.Text, out double y))
            {
                tbCheckResult.Text = "Неверные координаты";
                tbCheckResult.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            // Пока доступна только одна проверка — точка в полигоне
            if (selected is Pol poly)
            {
                bool inside = IsInside(x, y, poly);
                tbCheckResult.Text = inside ? "Внутри" : "Снаружи";
                tbCheckResult.Foreground = inside
                    ? System.Windows.Media.Brushes.Green
                    : System.Windows.Media.Brushes.Red;
            }
            else
            {
                tbCheckResult.Text = "Выделен не полигон";
                tbCheckResult.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}