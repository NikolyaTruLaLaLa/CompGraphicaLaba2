using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using BezierSpline.Math;

namespace BezierSpline.Views
{
    public partial class Task1View : UserControl
    {
        private enum Preset
        {
            Koch,
            Sierpinski,
            Dragon,
            Plant,
            Tree,
        }

        private const double StepLength = 10.0;
        private const double FitMargin = 20.0;

        private static readonly (string Name, string Text, int Iterations)[] Presets =
        {
            ("Коха (снежинка)", "F 60 0\nF F+F--F+F", 4),
            ("Серпинского (треугольник)", "F-G-G 120 0\nF F-G+F+G-F\nG GG", 6),
            ("Дракон", "FX 90 0\nX X+YF+\nY -FX-Y", 12),
            ("Растение", "X 25 0\nX F+[[X]-X]-F[-FX]+X\nF FF", 5),
            ("Дерево", "F 25 0\nF FF+[+F-F-F]-[-F+F+F]", 4),
        };

        private readonly Random _random = new Random();
        private string _definitionText = "";

        public Task1View()
        {
            InitializeComponent();

            iterationsSlider.ValueChanged += (s, e) => iterationsValue.Text = ((int)iterationsSlider.Value).ToString();
            randomnessSlider.ValueChanged += (s, e) => randomnessValue.Text = $"{randomnessSlider.Value * 100:0}%";
            jitterSlider.ValueChanged += (s, e) => jitterValue.Text = $"{jitterSlider.Value:0.#}°";

            LoadPreset(0);
        }

        private void LoadPreset(int index)
        {
            var preset = Presets[index];
            _definitionText = preset.Text;
            fileBox.Text = preset.Name;
            iterationsSlider.Value = preset.Iterations;
            treeModeCheck.IsChecked = index == (int)Preset.Tree;
            jitterSlider.Value = index == (int)Preset.Tree ? 5 : 0;
            Build();
        }

        private void Preset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (presetCombo.IsLoaded)
                LoadPreset(presetCombo.SelectedIndex);
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true)
                return;

            try
            {
                _definitionText = File.ReadAllText(dialog.FileName);
                fileBox.Text = dialog.FileName;
                Build();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить описание L-системы:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Build_Click(object sender, RoutedEventArgs e) => Build();

        private void Build()
        {
            LSystemDefinition definition;
            try
            {
                definition = LSystemDefinition.Parse(_definitionText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка описания",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int iterations = (int)iterationsSlider.Value;
            double randomness = randomnessSlider.Value;
            double angleJitter = jitterSlider.Value;

            string expanded = LSystem.Expand(definition, iterations, randomness, _random);
            var segments = LSystem.Interpret(expanded, definition.Angle, definition.InitialDirection,
                StepLength, angleJitter, _random);

            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;
            segments = LSystem.ScaleToFit(segments, width, height, FitMargin);

            canvas.SetSegments(segments, treeModeCheck.IsChecked == true);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (canvas != null && canvas.ActualWidth > 0)
                Build();
        }
    }
}
