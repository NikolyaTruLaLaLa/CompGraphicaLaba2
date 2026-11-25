using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using LABA_6.Core.Rendering;
using LABA_6.UI.Factory;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Camera = LABA_6.Core.Models.Camera;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private Mesh _baseMesh = null!;
        private Mesh _mesh = null!;
        private Matrix4x4 _modelMatrix = Matrix4x4.Identity;
        private Mesh? _mesh2 = null;
        private Matrix4x4 _modelMatrix2 = Matrix4x4.Identity;
        private readonly Camera _camera;

        private Texture _testTexture = null!;

        public MainWindow()
        {
            InitializeComponent();

            _camera = new Camera(
                position: new Point3(4, 4, 4),
                target: new Point3(0, 0, 0),
                up: new Point3(0, 1, 0)
            );

            _testTexture = CreateCheckerboardTexture();

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


        private Texture CreateCheckerboardTexture()
        {
            int size = 256;
            int[] pixels = new int[size * size];
            int color1 = unchecked((int)0xFFFFFFFF); // Белый
            int color2 = unchecked((int)0xFF000000); // Черный

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int blockSize = 32;
                    bool isColor1 = ((x / blockSize) + (y / blockSize)) % 2 == 0;
                    pixels[y * size + x] = isColor1 ? color1 : color2;
                }
            }
            return new Texture(size, size, pixels);
        }
    }
}