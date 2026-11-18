using LABA_6.Core.Models;
using System;
using System.Windows;
using System.Windows.Threading;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _cameraTimer = null!;
        private bool _isCameraRotating = false;
        private float _cameraAngle = 0f;
        private const float CameraRadius = 5f; 
        private const float CameraHeight = 2f; 
        private const float RotationSpeed = 0.02f; 

        private void OnToggleCameraRotationClick(object sender, RoutedEventArgs e)
        {
            if (_cameraTimer == null)
            {
                _cameraTimer = new DispatcherTimer();
                _cameraTimer.Interval = TimeSpan.FromMilliseconds(16); 
                _cameraTimer.Tick += CameraTimer_Tick;
            }

            _isCameraRotating = !_isCameraRotating;

            if (_isCameraRotating)
            {

                PerspectiveRadio.IsChecked = true;

                CameraRotationBtn.Content = "Остановить вращение";
                _cameraTimer.Start();
            }
            else
            {
                CameraRotationBtn.Content = "Запустить вращение камеры";
                _cameraTimer.Stop();
            }
        }

        private void CameraTimer_Tick(object? sender, EventArgs e)
        {
            _cameraAngle += RotationSpeed;

            float x = CameraRadius * (float)Math.Cos(_cameraAngle);
            float z = CameraRadius * (float)Math.Sin(_cameraAngle);

            _camera.Position = new Point3(x, CameraHeight, z);
            _camera.Target = new Point3(0, 0, 0); 

            RenderScene();
        }
    }
}