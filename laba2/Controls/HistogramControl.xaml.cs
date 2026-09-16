using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using laba2.Core.Histogram;

namespace laba2.Controls
{
    public partial class HistogramControl : UserControl
    {
        static int deltaForSign = 20;

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(
                nameof(Data), typeof(HistogramData), typeof(HistogramControl),
                new PropertyMetadata(null, onDataChanged));

        public HistogramData? Data
        {
            get => (HistogramData?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        private static void onDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((HistogramControl)d).Redraw();

        public HistogramControl()
        {
            InitializeComponent();
        }

        private void PlotCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
            => Redraw();

        private void Redraw()
        {
            PlotCanvas.Children.Clear();
            var data = Data;
            if (data is null || data.BinCount == 0) return;

            double w = PlotCanvas.ActualWidth;
            double h = PlotCanvas.ActualHeight;
            if (w <= 1 || h <= 1) return;

            double leftPad = 44;    // место под числа оси Oy
            double topPad = 24;     // место под заголовок
            double bottomPad = deltaForSign; // место под подписи оси Ox

            double plotW = w - leftPad;
            double plotH = h - topPad - bottomPad;
            if (plotW <= 1 || plotH <= 1) return;

            var ordered = data.Data.OrderBy(kv => int.Parse(kv.Key.Split('-')[0])).ToList();
            int n = ordered.Count;
            double binW = plotW / n;
            int maxCount = data.MaxData;
            if (maxCount == 0) return;

            double axisY = topPad + plotH;  

            for (int i = 0; i < n; ++i)
            {
                double barH = ordered[i].Value / (double)maxCount * plotH;
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = Math.Max(1, binW - 1),
                    Height = Math.Max(0, barH),
                    Fill = Brushes.SteelBlue
                };
                Canvas.SetLeft(rect, leftPad + i * binW);
                Canvas.SetTop(rect, axisY - barH);
                PlotCanvas.Children.Add(rect);
            }

            var xAxis = new System.Windows.Shapes.Line
            {
                X1 = leftPad,
                Y1 = axisY,
                X2 = w,
                Y2 = axisY,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            PlotCanvas.Children.Add(xAxis);

            var yAxis = new System.Windows.Shapes.Line
            {
                X1 = leftPad,
                Y1 = topPad,
                X2 = leftPad,
                Y2 = axisY,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            PlotCanvas.Children.Add(yAxis);

            int yTicks = 5;
            for (int t = 0; t <= yTicks; t++)
            {
                double frac = t / (double)yTicks;
                double yPos = axisY - frac * plotH;
                int value = (int)Math.Round(maxCount * frac);

                var tick = new System.Windows.Shapes.Line
                {
                    X1 = leftPad - 4,
                    Y1 = yPos,
                    X2 = leftPad,
                    Y2 = yPos,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(tick);

                var lbl = new TextBlock { Text = value.ToString(), FontSize = 10 };
                lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(lbl, leftPad - 6 - lbl.DesiredSize.Width);
                Canvas.SetTop(lbl, yPos - lbl.DesiredSize.Height / 2);
                PlotCanvas.Children.Add(lbl);
            }

            int labelEvery = Math.Max(1, n / 10);
            for (int i = 0; i < n; i += labelEvery)
            {
                double xCenter = leftPad + i * binW + binW / 2;

                var tick = new System.Windows.Shapes.Line
                {
                    X1 = xCenter,
                    Y1 = axisY,
                    X2 = xCenter,
                    Y2 = axisY + 4,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                PlotCanvas.Children.Add(tick);

                var tb = new TextBlock
                {
                    Text = ordered[i].Key,
                    FontSize = 10,
                    Foreground = Brushes.Black
                };
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Canvas.SetLeft(tb, xCenter - tb.DesiredSize.Width / 2);
                Canvas.SetTop(tb, axisY + 6);
                PlotCanvas.Children.Add(tb);
            }

            var title = new TextBlock
            {
                Text = data.Title ?? "",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            title.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Canvas.SetLeft(title, (w - title.DesiredSize.Width) / 2);
            Canvas.SetTop(title, 2);
            PlotCanvas.Children.Add(title);
        }

    }
}
