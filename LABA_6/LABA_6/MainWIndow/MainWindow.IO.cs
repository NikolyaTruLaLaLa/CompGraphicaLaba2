using LABA_6.Core.Formats;
using LABA_6.Core.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private Texture? _currentTexture = null;

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

                    // Если путь к текстуре есть и файл реально существует
                    if (!string.IsNullOrEmpty(loadedMesh.TexturePath) && File.Exists(loadedMesh.TexturePath))
                    {
                        _currentTexture = LoadWpfTexture(loadedMesh.TexturePath);
                    }
                    // Если текстуры нет, но UV-координаты есть, натягиваем шахматку для теста
                    else if (loadedMesh.TexCoords.Count > 0)
                    {
                        _currentTexture = CreateCheckerboardTexture();
                        MessageBox.Show("Файл текстуры не найден, но UV-координаты есть. Применена тестовая текстура.");
                    }
                    else
                    {
                        _currentTexture = null;
                    }

                    LoadPolyhedron(loadedMesh);
                    PolyhedronComboBox.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла:\n{ex.Message}", "Ошибка");
                }
            }
        }

        private Texture? LoadWpfTexture(string imagePath)
        {
            if (!File.Exists(imagePath)) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] pixels = new int[width * height];

            var format = PixelFormats.Bgra32;
            if (bitmap.Format != format)
            {
                var converted = new FormatConvertedBitmap(bitmap, format, null, 0);
                converted.CopyPixels(pixels, width * 4, 0);
            }
            else
            {
                bitmap.CopyPixels(pixels, width * 4, 0);
            }

            return new Texture(width, height, pixels);
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