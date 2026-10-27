using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using LABA_6.UI.Factory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Camera = LABA_6.Core.Models.Camera;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private Mesh _baseMesh = null!;
        private Mesh _mesh = null!;
        private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private readonly Camera _camera;

        public MainWindow()
        {
            InitializeComponent();

            _camera = new Camera(
                position: new Point3(4, 4, 4),
                target: new Point3(0, 0, 0),
                up: new Point3(0, 1, 0)
            );

            LoadPolyhedron(PolyhedronFactory.CreateTetrahedron());
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e) => RenderScene();
        private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e) => RenderScene();
        private void OnProjectionChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void LoadPolyhedron(Mesh mesh)
        {
            _baseMesh = mesh;
            _modelMatrix = Matrix4x4.Identity;
            UpdateMesh();
        }

        private void UpdateMesh()
        {
            _mesh = _baseMesh.Clone().ApplyTransform(_modelMatrix);
            RenderScene();
        }

        private void ApplyTransform(Matrix4x4 transform)
        {
            _modelMatrix = transform * _modelMatrix;
            UpdateMesh();
        }

        private float GetFloat(TextBox tb, float defaultVal = 0f)
        {
            if (float.TryParse(tb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float res))
                return res;
            return defaultVal;
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            _modelMatrix = Matrix4x4.Identity;
            UpdateMesh();
        }

        private void OnPolyhedronChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_baseMesh == null) return;
            Mesh newMesh = PolyhedronComboBox.SelectedIndex switch
            {
                0 => PolyhedronFactory.CreateTetrahedron(),
                1 => PolyhedronFactory.CreateCube(),
                2 => PolyhedronFactory.CreateOctahedron(),
                3 => PolyhedronFactory.CreateIcosahedron(),
                4 => PolyhedronFactory.CreateDodecahedron(),
                _ => PolyhedronFactory.CreateCube()
            };
            LoadPolyhedron(newMesh);
        }

        // ... (Остальные обработчики кнопок OnTranslateClick, OnRotateClick, OnScaleClick, 
        // OnReflectClick, OnCenterScaleClick, OnCenterRotateClick, OnLineRotateClick 
        // остаются точно такими же, как у тебя были. Не меняй их.)

        private void OnTranslateClick(object sender, RoutedEventArgs e) => ApplyTransform(Matrix4x4.Translation(GetFloat(TxBox), GetFloat(TyBox), GetFloat(TzBox)));

        private void OnRotateClick(object sender, RoutedEventArgs e)
        {
            float rx = TransformFactory.DegToRad(GetFloat(RxBox));
            float ry = TransformFactory.DegToRad(GetFloat(RyBox));
            float rz = TransformFactory.DegToRad(GetFloat(RzBox));
            ApplyTransform(Matrix4x4.RotationX(rx) * Matrix4x4.RotationY(ry) * Matrix4x4.RotationZ(rz));
        }

        private void OnScaleClick(object sender, RoutedEventArgs e) => ApplyTransform(Matrix4x4.Scale(GetFloat(SxBox, 1f), GetFloat(SyBox, 1f), GetFloat(SzBox, 1f)));

        private void OnReflectClick(object sender, RoutedEventArgs e)
        {
            var refl = ReflectionComboBox.SelectedIndex switch
            {
                0 => TransformFactory.Reflection(TransformFactory.CoordinatePlane.XY),
                1 => TransformFactory.Reflection(TransformFactory.CoordinatePlane.XZ),
                2 => TransformFactory.Reflection(TransformFactory.CoordinatePlane.YZ),
                _ => Matrix4x4.Identity
            };
            ApplyTransform(refl);
        }

        private void OnCenterScaleClick(object sender, RoutedEventArgs e) => ApplyTransform(TransformFactory.ScaleAboutCenter(_mesh, GetFloat(CxBox, 1f), GetFloat(CyBox, 1f), GetFloat(CzBox, 1f)));

        private void OnCenterRotateClick(object sender, RoutedEventArgs e)
        {
            var axis = CenterAxisCombo.SelectedIndex switch
            {
                0 => TransformFactory.CoordinateAxis.X,
                1 => TransformFactory.CoordinateAxis.Y,
                2 => TransformFactory.CoordinateAxis.Z,
                _ => TransformFactory.CoordinateAxis.Y
            };
            ApplyTransform(TransformFactory.RotationAboutCenterParallelToAxis(_mesh, axis, TransformFactory.DegToRad(GetFloat(CenterAngleBox))));
        }

        private void OnLineRotateClick(object sender, RoutedEventArgs e)
        {
            var p1 = new Point3(GetFloat(P1x), GetFloat(P1y), GetFloat(P1z));
            var p2 = new Point3(GetFloat(P2x), GetFloat(P2y), GetFloat(P2z));
            try { ApplyTransform(TransformFactory.RotationAboutLine(p1, p2, TransformFactory.DegToRad(GetFloat(LineAngleBox)))); }
            catch (ArgumentException) { MessageBox.Show("Точки прямой P1 и P2 не должны совпадать!", "Ошибка"); }
        }

        // ================= РЕНДЕР ЧЕРЕЗ CANVAS =================

        private void RenderScene()
        {
            if (DrawCanvas.ActualWidth == 0 || _mesh == null) return;

            DrawCanvas.Children.Clear();

            double width = DrawCanvas.ActualWidth;
            double height = DrawCanvas.ActualHeight;
            float aspect = (float)(width / height);

            // 1. Матрицы (Камера * Проекция)
            Matrix4x4 view = _camera.GetViewMatrix();
            Matrix4x4 proj = PerspectiveRadio.IsChecked == true
                ? _camera.GetProjectionMatrix(aspect)
                : Matrix4x4.Scale(0.4f, 0.4f, 0.4f);

            Matrix4x4 viewProj = proj * view;

            // 2. Рисуем оси координат (Они статичны, не зависят от матрицы модели)
            DrawAxes(viewProj, width, height);

            // 3. Отрисовка самого многогранника
            var screenPoints = new Point[_mesh.VertexCount];
            for (int i = 0; i < _mesh.VertexCount; i++)
            {
                var projectedV = viewProj * _mesh.Vertices[i];
                screenPoints[i] = new Point(
                    (projectedV.X + 1.0) * 0.5 * width,
                    (1.0 - projectedV.Y) * 0.5 * height);
            }

            var edgeBrush = new SolidColorBrush(Colors.Cyan);
            foreach (var face in _mesh.Faces)
            {
                for (int i = 0; i < face.Count; i++)
                {
                    var p1 = screenPoints[face.Indices[i]];
                    var p2 = screenPoints[face.Indices[(i + 1) % face.Count]];

                    DrawCanvas.Children.Add(new Line
                    {
                        X1 = p1.X,
                        Y1 = p1.Y,
                        X2 = p2.X,
                        Y2 = p2.Y,
                        Stroke = edgeBrush,
                        StrokeThickness = 1.5
                    });
                }
            }
        }

        private void DrawAxes(Matrix4x4 viewProj, double width, double height)
        {
            // Функция для перевода 3D точки в 2D координаты Canvas
            Point Project(Point3 v)
            {
                var p = viewProj * v;
                return new Point((p.X + 1.0) * 0.5 * width, (1.0 - p.Y) * 0.5 * height);
            }

            // Длина осей
            float len = 2.5f;
            Point origin = Project(new Point3(0, 0, 0));

            // Ось X (Красная)
            var px = Project(new Point3(len, 0, 0));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = px.X, Y2 = px.Y, Stroke = Brushes.Red, StrokeThickness = 2 });

            // Ось Y (Зеленая)
            var py = Project(new Point3(0, len, 0));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = py.X, Y2 = py.Y, Stroke = Brushes.Lime, StrokeThickness = 2 });

            // Ось Z (Синяя)
            var pz = Project(new Point3(0, 0, len));
            DrawCanvas.Children.Add(new Line { X1 = origin.X, Y1 = origin.Y, X2 = pz.X, Y2 = pz.Y, Stroke = Brushes.Blue, StrokeThickness = 2 });
        }
    }
}