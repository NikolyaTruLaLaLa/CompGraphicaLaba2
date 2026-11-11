using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {

        private void OnProfileChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileTextBox == null) return;

            if (ProfileComboBox.SelectedIndex == 4) 
            {
                ProfileTextBox.IsEnabled = true;
            }
            else
            {
                ProfileTextBox.IsEnabled = false;
                ProfileTextBox.Text = ProfileComboBox.SelectedIndex switch
                {
                    0 => "1,0; 1,2",                                     
                    1 => "1,0; 0,2",                                     
                    2 => "1,0; 1,1; 1,1.5; 0.5,2; 0.8,2.5",                
                    3 => "0,-1; 0.5,-0.866; 0.866,-0.5; 1,0; 0.866,0.5; 0.5,0.866; 0,1", 
                    _ => "1,0; 1,1"
                };
            }
        }

        private void OnBuildRevolutionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var profile = new List<Point3>();
                string[] pairs = ProfileTextBox.Text.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var pair in pairs)
                {
                    string[] coords = pair.Split(',');
                    if (coords.Length == 2)
                    {
                        float x = float.Parse(coords[0].Trim(), CultureInfo.InvariantCulture);
                        float y = float.Parse(coords[1].Trim(), CultureInfo.InvariantCulture);
                        profile.Add(new Point3(x, y, 0)); // Z = 0
                    }
                }

                if (profile.Count < 2)
                {
                    MessageBox.Show("Введите минимум 2 точки профиля!", "Ошибка");
                    return;
                }

                int divisions = int.Parse(DivisionsTextBox.Text);
                var axis = RevAxisComboBox.SelectedIndex switch
                {
                    0 => TransformFactory.CoordinateAxis.X,
                    2 => TransformFactory.CoordinateAxis.Z,
                    _ => TransformFactory.CoordinateAxis.Y
                };

                Mesh revMesh = RevolutionFactory.Create(profile, divisions, axis);
                LoadPolyhedron(revMesh);

                PolyhedronComboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении:\n{ex.Message}", "Ошибка");
            }
        }
    }
}