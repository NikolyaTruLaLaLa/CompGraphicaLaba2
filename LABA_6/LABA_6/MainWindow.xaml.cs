using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using LABA_6.UI.Factory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
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

        private float GetFloat(TextBox tb, float defaultVal = 0f)
        {
            if (float.TryParse(tb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out float res))
                return res;
            return defaultVal;
        }
    }
}