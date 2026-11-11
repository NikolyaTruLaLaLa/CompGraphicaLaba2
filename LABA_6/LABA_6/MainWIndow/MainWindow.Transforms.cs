using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using LABA_6.UI.Factory;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
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

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            _modelMatrix = Matrix4x4.Identity;
            UpdateMesh();
        }

        private void OnPolyhedronChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PolyhedronComboBox.SelectedIndex < 0) return;
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

        private void OnTranslateClick(object sender, RoutedEventArgs e)
            => ApplyTransform(Matrix4x4.Translation(GetFloat(TxBox), GetFloat(TyBox), GetFloat(TzBox)));

        private void OnRotateClick(object sender, RoutedEventArgs e)
        {
            float rx = TransformFactory.DegToRad(GetFloat(RxBox));
            float ry = TransformFactory.DegToRad(GetFloat(RyBox));
            float rz = TransformFactory.DegToRad(GetFloat(RzBox));
            ApplyTransform(Matrix4x4.RotationX(rx) * Matrix4x4.RotationY(ry) * Matrix4x4.RotationZ(rz));
        }

        private void OnScaleClick(object sender, RoutedEventArgs e)
            => ApplyTransform(Matrix4x4.Scale(GetFloat(SxBox, 1f), GetFloat(SyBox, 1f), GetFloat(SzBox, 1f)));

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

        private void OnCenterScaleClick(object sender, RoutedEventArgs e)
            => ApplyTransform(TransformFactory.ScaleAboutCenter(_mesh, GetFloat(CxBox, 1f), GetFloat(CyBox, 1f), GetFloat(CzBox, 1f)));

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
            try
            {
                ApplyTransform(TransformFactory.RotationAboutLine(p1, p2, TransformFactory.DegToRad(GetFloat(LineAngleBox))));
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Точки прямой P1 и P2 не должны совпадать!", "Ошибка");
            }
        }
    }
}