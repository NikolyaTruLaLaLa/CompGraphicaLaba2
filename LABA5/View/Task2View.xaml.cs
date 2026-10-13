using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BezierSpline.Math;

namespace BezierSpline.Views
{
    public partial class Task2View : UserControl
    {
        private List<Point> currentPoints = new List<Point>();
        private List<List<Point>> allSteps = new List<List<Point>>();
        private bool showingSteps = false;
        private int currentStepIndex = 0;

        public Task2View()
        {
            InitializeComponent();
            iterationsSlider.ValueChanged += (s, e) => iterationsValue.Text = ((int)iterationsSlider.Value).ToString();
            roughnessSlider.ValueChanged += (s, e) => roughnessValue.Text = roughnessSlider.Value.ToString("F1");
            displacementSlider.ValueChanged += (s, e) => displacementValue.Text = displacementSlider.Value.ToString("F2");
            Generate();
        }

        private void Generate_Click(object sender, RoutedEventArgs e) => Generate();

        private void ShowSteps_Click(object sender, RoutedEventArgs e)
        {
            showingSteps = !showingSteps;
            if (showingSteps && allSteps.Count > 0)
            {
                currentStepIndex = 0;
                nextStepBtn.Visibility = Visibility.Visible;
                showStepsBtn.Content = "Скрыть шаги";
                DrawStep(0);
            }
            else
            {
                nextStepBtn.Visibility = Visibility.Collapsed;
                showStepsBtn.Content = "Показать шаги";
                DrawFinal();
            }
        }

        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            if (allSteps.Count == 0) return;

            currentStepIndex++;
            if (currentStepIndex >= allSteps.Count)
                currentStepIndex = 0;

            DrawStep(currentStepIndex);
        }

        private void Generate()
        {
            if (!int.TryParse(seedBox.Text, out int seed))
                seed = 12345;

            int iterations = (int)iterationsSlider.Value;
            double roughness = roughnessSlider.Value;
            double displacementFactor = displacementSlider.Value;

            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 400;

            allSteps = MidpointDisplacement.Generate(iterations, roughness, displacementFactor, seed, width, height);
            currentPoints = allSteps[allSteps.Count - 1];

            showingSteps = false;
            nextStepBtn.Visibility = Visibility.Collapsed;
            showStepsBtn.Content = "Показать шаги";
            DrawFinal();
        }

        private void DrawFinal()
        {
            canvas.Children.Clear();
            DrawPolyline(currentPoints, Brushes.SaddleBrown, 2);
            DrawFill(currentPoints, canvas.ActualHeight);
        }

        private void DrawStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= allSteps.Count) return;

            canvas.Children.Clear();
            var points = allSteps[stepIndex];
            DrawPolyline(points, Brushes.SaddleBrown, 2);
            DrawFill(points, canvas.ActualHeight);

            var stepText = new TextBlock
            {
                Text = $"Шаг {stepIndex} / {allSteps.Count - 1} (точек: {points.Count})",
                FontSize = 14,
                Foreground = Brushes.DarkSlateGray,
                Margin = new Thickness(10)
            };
            canvas.Children.Add(stepText);
            Canvas.SetLeft(stepText, 10);
            Canvas.SetTop(stepText, 10);
        }

        private void DrawPolyline(List<Point> points, Brush brush, double thickness)
        {
            var polyline = new Polyline
            {
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round
            };
            foreach (var p in points)
                polyline.Points.Add(p);
            canvas.Children.Add(polyline);
        }

        private void DrawFill(List<Point> points, double canvasHeight)
        {
            var polygon = new Polygon
            {
                Fill = new SolidColorBrush(Color.FromArgb(60, 139, 69, 19)),
                Stroke = Brushes.Transparent
            };

            polygon.Points.Add(new Point(0, canvasHeight));
            foreach (var p in points)
                polygon.Points.Add(p);
            polygon.Points.Add(new Point(points[points.Count - 1].X, canvasHeight));

            canvas.Children.Insert(0, polygon);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (currentPoints.Count > 0)
                Generate();
        }
    }
}