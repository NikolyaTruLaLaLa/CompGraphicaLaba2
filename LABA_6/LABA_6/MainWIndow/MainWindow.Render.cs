using LABA_6.Core.Models;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private void OnProjectionChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void RenderScene()
        {
            if (DrawCanvas.ActualWidth == 0 || _mesh == null) return;

            DrawCanvas.Children.Clear();

            double width = DrawCanvas.ActualWidth;
            double height = DrawCanvas.ActualHeight;
            float aspect = (float)(width / height);

            Matrix4x4 view = _camera.GetViewMatrix();
            Matrix4x4 proj = PerspectiveRadio.IsChecked == true
                ? _camera.GetProjectionMatrix(aspect)
                : Matrix4x4.Scale(0.4f, 0.4f, 0.4f);

            Matrix4x4 viewProj = proj * view;

            DrawAxes(viewProj, width, height);

            var screenPoints = new Point?[_mesh.VertexCount];

            for (int i = 0; i < _mesh.VertexCount; i++)
            {
                var projectedV = viewProj * _mesh.Vertices[i];

                if (float.IsNaN(projectedV.X) || float.IsNaN(projectedV.Y) ||
                    float.IsInfinity(projectedV.X) || float.IsInfinity(projectedV.Y))
                {
                    screenPoints[i] = null; 
                }
                else
                {
                    screenPoints[i] = new Point(
                        (projectedV.X + 1.0) * 0.5 * width,
                        (1.0 - projectedV.Y) * 0.5 * height);
                }
            }

            var edgeBrush = new SolidColorBrush(Colors.Cyan);
            foreach (var face in _mesh.Faces)
            {
                for (int i = 0; i < face.Count; i++)
                {
                    var p1 = screenPoints[face.Indices[i]];
                    var p2 = screenPoints[face.Indices[(i + 1) % face.Count]];

                    if (p1.HasValue && p2.HasValue)
                    {
                        DrawCanvas.Children.Add(new Line
                        {
                            X1 = p1.Value.X,
                            Y1 = p1.Value.Y,
                            X2 = p2.Value.X,
                            Y2 = p2.Value.Y,
                            Stroke = edgeBrush,
                            StrokeThickness = 1.5
                        });
                    }
                }
            }
        }

        private void DrawAxes(Matrix4x4 viewProj, double width, double height)
        {
            Point Project(Point3 v)
            {
                var p = viewProj * v;
                return new Point((p.X + 1.0) * 0.5 * width, (1.0 - p.Y) * 0.5 * height);
            }

            float len = 2.5f;
            Point origin = Project(new Point3(0, 0, 0));

            var px = Project(new Point3(len, 0, 0));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = px.X, Y2 = px.Y, Stroke = Brushes.Red, StrokeThickness = 2 });

            var py = Project(new Point3(0, len, 0));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = py.X, Y2 = py.Y, Stroke = Brushes.Lime, StrokeThickness = 2 });

            var pz = Project(new Point3(0, 0, len));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = pz.X, Y2 = pz.Y, Stroke = Brushes.Blue, StrokeThickness = 2 });
        }
    }
}