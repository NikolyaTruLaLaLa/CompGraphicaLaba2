using LABA_6.Core.Formats;
using LABA_6.Core.Models;
using Microsoft.Win32;
using System;
using System.Windows;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private void OnLoadObjClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj|Все файлы (*.*)|*.*",
                Title = "Загрузить 3D модель"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    Mesh loadedMesh = ObjHandler.Load(openFileDialog.FileName);
                    LoadPolyhedron(loadedMesh);
                    PolyhedronComboBox.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла:\n{ex.Message}", "Ошибка");
                }
            }
        }

        private void OnSaveObjClick(object sender, RoutedEventArgs e)
        {
            if (_mesh == null) return;

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Wavefront OBJ (*.obj)|*.obj",
                Title = "Сохранить 3D модель",
                FileName = "model.obj"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ObjHandler.Save(_mesh, saveFileDialog.FileName);
                    MessageBox.Show("Модель успешно сохранена!", "Успех");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}", "Ошибка");
                }
            }
        }
    }
}