using LABA_6.Core.Factory;
using LABA_6.Core.Models;
using LABA_6.Core.Utils;
using System;
using System.Windows;

namespace LABA_6.UI
{
    public partial class MainWindow : Window
    {
        private void OnBuildGraphClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Func<float, float, float> func;

                if (FunctionComboBox.SelectedIndex == 4)
                {
                    try
                    {
                        func = ExpressionCompiler.Compile(CustomFormulaBox.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка в формуле: {ex.Message}\nПример: sin(x) * cos(y) + 0.5", "Ошибка парсинга");
                        return;
                    }
                }
                else
                {
                    func = FunctionComboBox.SelectedIndex switch
                    {
                        0 => (x, y) => x + y,
                        1 => (x, y) => x * x + y * y,
                        2 => (x, y) => x * x - y * y,
                        3 => (x, y) => (float)(Math.Sin(x) * Math.Cos(y)),
                        _ => (x, y) => 0
                    };
                }

                float xMin = GetFloat(XMinBox);
                float xMax = GetFloat(XMaxBox);
                float yMin = GetFloat(YMinBox);
                float yMax = GetFloat(YMaxBox);
                int divisions = int.Parse(GraphDivisionsBox.Text);

                Mesh graphMesh = FunctionGraphFactory.Create(func, xMin, xMax, yMin, yMax, divisions);

                LoadPolyhedron(graphMesh);
                PolyhedronComboBox.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении графика:\n{ex.Message}", "Ошибка");
            }
        }
        private void OnFunctionSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CustomFormulaBox == null) return;

            CustomFormulaBox.IsEnabled = (FunctionComboBox.SelectedIndex == 4);
        }
    }

}