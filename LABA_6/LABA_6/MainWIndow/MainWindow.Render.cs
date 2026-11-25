using LABA_6.Core.Models;
using LABA_6.Core.Rendering;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap _bitmap = null!;
        private FrameBuffer _frameBuffer = null!;

        private void OnProjectionChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void OnCullingChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void OnRenderSettingsChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void OnViewVectorChanged(object sender, RoutedEventArgs e) => RenderScene();

        private void OnOverlapChanged(object sender, RoutedEventArgs e)
        {
            if (OverlapCheckBox.IsChecked == true)
            {
                var profile = new System.Collections.Generic.List<Point3>();
                int segments = 16;
                for (int i = 0; i <= segments; i++)
                {
                    float angle = (float)(-Math.PI / 2 + Math.PI * i / segments);
                    profile.Add(new Point3((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
                }
                _mesh2 = LABA_6.Core.Factory.RevolutionFactory.Create(profile, 24, LABA_6.Core.Factory.TransformFactory.CoordinateAxis.Y);

                _modelMatrix2 = Matrix4x4.Translation(0.8f, 0.8f, 0f);
            }
            else
            {
                _mesh2 = null;
            }
            RenderScene();
        }

        private void RenderScene()
        {
            try
            {
                int w = (int)RenderBorder.ActualWidth;
                int h = (int)RenderBorder.ActualHeight;

                if (w <= 0 || h <= 0 || _mesh == null) return;

                if (_bitmap == null || _bitmap.PixelWidth != w || _bitmap.PixelHeight != h)
                {
                    _bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
                    RenderImage.Source = _bitmap;
                    _frameBuffer = new FrameBuffer(w, h);
                }

                bool isPerspective = PerspectiveRadio.IsChecked == true;
                bool doCulling = CullingCheckBox.IsChecked == true;
                bool showNormals = ShowNormalsCheckBox.IsChecked == true;

                Point3 customViewDir = new Point3(GetFloat(ViewXBox, 0f), GetFloat(ViewYBox, 0f), GetFloat(ViewZBox, -1f)).Normalized();

                var sceneObjects = new System.Collections.Generic.List<SceneObject>
                {
                    new SceneObject(_mesh, _modelMatrix, Rasterizer.ColorCyan)
                };

                if (_mesh2 != null)
                {
                    sceneObjects.Add(new SceneObject(_mesh2, _modelMatrix2, unchecked((int)0xFFFF00FF)));
                }

                Point3 lightPos = new Point3(GetFloat(LightXBox, 2f), GetFloat(LightYBox, 3f), GetFloat(LightZBox, 2f));
                Light light = new Light(lightPos);
                bool useGouraud = GouraudCheckBox.IsChecked == true;
                int objColor = Rasterizer.ColorCyan;

                SoftwareRenderer.Render(_frameBuffer, sceneObjects, _camera, isPerspective, doCulling,
                                        customViewDir, showNormals, _currentTexture, light, useGouraud, objColor, out int renderedFaces);
                _bitmap.Lock();
                Marshal.Copy(_frameBuffer.Color, 0, _bitmap.BackBuffer, _frameBuffer.Color.Length);
                _bitmap.AddDirtyRect(new Int32Rect(0, 0, w, h));
                _bitmap.Unlock();

                int totalFaces = _mesh.FaceCount + (_mesh2?.FaceCount ?? 0);
                FaceCountText.Text = $"Отрисовано граней: {renderedFaces} / {totalFaces}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Ошибка в RenderScene");
            }
        }
    }
}