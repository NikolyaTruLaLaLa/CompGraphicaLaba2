using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using LABA_3.Drawing;

namespace LABA_3.Controls
{
    /// <summary>
    /// Задание 3: градиентное окрашивание произвольного треугольника
    /// с тремя разноцветными вершинами (затенение Гуро поверх
    /// алгоритма растеризации треугольника).
    /// </summary>
    /// <remarks>
    /// Пользователь ставит три точки кликами по холсту, для каждой выбирает
    /// цвет. Заливка выполняет <see cref="GradientCanvas.RasterizeTriangle"/>.
    /// Вся работа с пикселями — в <see cref="GradientCanvas"/>, здесь только
    /// ввод, выбор алгоритма/цвета и обновление подписей.
    /// </remarks>
    public partial class GradientTriangleControl : UserControl
    {
        /// <summary>Пункт выпадающего списка цветов: подпись и сам цвет.</summary>
        private sealed record ColorOption(string Name, Color Color);

        /// <summary>Доступные цвета вершин (показываются в трёх ComboBox).</summary>
        private static readonly ColorOption[] Palette =
        {
            new("Красный", Colors.Red),
            new("Зелёный", Colors.Lime),
            new("Синий", Colors.Blue),
            new("Жёлтый", Colors.Yellow),
            new("Голубой", Colors.Cyan),
            new("Пурпурный", Colors.Magenta),
            new("Оранжевый", Colors.Orange),
            new("Индиго", Colors.Indigo),
            new("Бирюзовый", Colors.Turquoise),
            new("Розовый", Colors.HotPink),
            new("Серебряный", Colors.Silver),
            new("Чёрный", Colors.Black)
        };

        /// <summary>Минимальная удвоенная площадь случайного треугольника — отсекает «слишком тонкие».</summary>
        private const double MinRandomDoubleArea = 30000.0;

        /// <summary>Холст, на котором рисуется и заливается треугольник.</summary>
        private readonly GradientCanvas _canvas = new();

        /// <summary>Заданные вершины (0..3): три последние образуют текущий треугольник.</summary>
        private readonly List<GradientVertex> _vertices = new();

        /// <summary>Генератор случайных чисел для кнопки «Случайный».</summary>
        private readonly Random _random = new();

        /// <summary>Идёт программная смена цветов — события выбора не должны вызывать перерисовку.</summary>
        private bool _syncingColors;

        /// <summary>Контрол полностью загружен; до этого события выбора игнорируются.</summary>
        private bool _ready;

        /// <summary>Создаёт вкладку: готовит холст, палитры цветов и начальные подписи.</summary>
        public GradientTriangleControl()
        {
            InitializeComponent();

            // картинка холста показывается через элемент Image
            CanvasImage.Source = _canvas.Bitmap;

            foreach (TextBlock label in new[] { LabelA, LabelB, LabelC })
                label.Effect = new DropShadowEffect { BlurRadius = 3, ShadowDepth = 1, Opacity = 0.9 };

            ColorComboA.ItemsSource = Palette;
            ColorComboB.ItemsSource = Palette;
            ColorComboC.ItemsSource = Palette;
            ColorComboA.SelectedIndex = 0;
            ColorComboB.SelectedIndex = 1;
            ColorComboC.SelectedIndex = 2;

            UpdateSwatches();
            UpdateLabels();
            UpdateStatus();
            _ready = true; // только теперь события выбора должны вызывать перерисовку
        }

        /// <summary>Клик по холсту: координата берётся в системе холста и передаётся в <see cref="AddVertex"/>.</summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            => AddVertex(e.GetPosition(DrawingCanvas));

        /// <summary>Добавляет вершину треугольника; четвёртая точка начинает новый треугольник.</summary>
        /// <param name="position">Координата клика в системе холста.</param>
        private void AddVertex(Point position)
        {
            int x = (int)Math.Round(position.X);
            int y = (int)Math.Round(position.Y);

            if (x < 0 || x >= GradientCanvas.Width || y < 0 || y >= GradientCanvas.Height) return;

            if (_vertices.Count == 3) _vertices.Clear();

            _vertices.Add(new GradientVertex(new Point(x, y), VertexColor(_vertices.Count)));
            Render();
        }

        /// <summary>Кнопка «Залить градиентом»: перерисовывает треугольник по текущим данным.</summary>
        private void Fill_Click(object sender, RoutedEventArgs e) => Render();

        /// <summary>Кнопка «Случайный»: ставит случайный крупный треугольник и раздаёт три разных цвета.</summary>
        private void Random_Click(object sender, RoutedEventArgs e)
        {
            var a = new GradientVertex(RandomPoint(), Colors.Black);
            var b = new GradientVertex(RandomPoint(), Colors.Black);

            // третью точку подбираем так, чтобы треугольник не оказался вырожденным
            var c = new GradientVertex(RandomPoint(), Colors.Black);
            for (int attempt = 0; attempt < 50; attempt++)
            {
                if (Math.Abs(DoubleArea(a.Position, b.Position, c.Position)) >= MinRandomDoubleArea) break;

                c = new GradientVertex(RandomPoint(), Colors.Black);
            }

            _vertices.Clear();
            _vertices.Add(a);
            _vertices.Add(b);
            _vertices.Add(c);

            RandomizeColors();
            Render();
        }

        /// <summary>Кнопка «Убрать точку»: убирает последнюю поставленную вершину.</summary>
        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            if (_vertices.Count == 0) return;

            _vertices.RemoveAt(_vertices.Count - 1);
            Render();
        }

        /// <summary>Кнопка «Очистить»: убирает все вершины и очищает холст.</summary>
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _vertices.Clear();
            Render();
        }

        /// <summary>Кнопка «Сравнить алгоритмы»: считает расхождения двух способов растеризации.</summary>
        private void Compare_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetTriangle(out GradientVertex v0, out GradientVertex v1, out GradientVertex v2)) return;

            var watch = Stopwatch.StartNew();
            int differences = _canvas.CompareRasterizers(v0, v1, v2);
            watch.Stop();

            InfoText.Text = $"сравнение: расхождений {differences} " +
                            (differences == 0 ? "(оба алгоритма идентичны)" : "(расхождение требует разбора)") +
                            $", {watch.Elapsed.TotalMilliseconds:0.###} мс";
            StatusText.Text = "Сканлайн с барицентрическими весами против трёх функций-полуплоскостей";
        }

        /// <summary>Смена алгоритма растеризации: перерисовывает треугольник выбранным способом.</summary>
        private void AlgorithmCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => Render();

        /// <summary>Включение/выключение контура и маркеров вершин: перерисовывает холст.</summary>
        private void OverlayCheck_Click(object sender, RoutedEventArgs e) => Render();

        /// <summary>Смена цвета вершины: обновляет плашку и, если это не программная смена, перезаливает.</summary>
        private void ColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSwatches();
            if (_syncingColors) return;

            Render();
        }

        /// <summary>
        /// Главный метод перерисовки: при трёх вершинах заливает треугольник,
        /// иначе показывает ломаную предпросмотра. Обновляет подписи и статус.
        /// </summary>
        private void Render()
        {
            if (!_ready) return; // события приходят и во время InitializeComponent

            if (_vertices.Count == 3)
            {
                SyncVertexColors();

                var watch = Stopwatch.StartNew();
                _canvas.RasterizeTriangle(_vertices[0], _vertices[1], _vertices[2],
                                         SelectedRasterizer, OverlayCheck.IsChecked == true);
                watch.Stop();

                int total = GradientCanvas.Width * GradientCanvas.Height;
                InfoText.Text = $"алгоритм: {SelectedRasterizerName}; залито {_canvas.FilledPixelCount} из {total} " +
                                $"пикселей ({100.0 * _canvas.FilledPixelCount / total:0.00}%); " +
                                $"время {watch.Elapsed.TotalMilliseconds:0.###} мс";
            }
            else
            {
                _canvas.Clear();
                if (_vertices.Count > 0)
                    _canvas.DrawPolyline(_vertices.Select(v => v.Position).ToList(), Colors.Black);

                InfoText.Text = string.Empty;
            }

            UpdateLabels();
            UpdateStatus();
        }

        /// <summary>
        /// Пытается получить три вершины, синхронизируя их цвета с ComboBox.
        /// Если вершин не три — пишет подсказку в статус и возвращает <c>false</c>.
        /// </summary>
        private bool TryGetTriangle(out GradientVertex v0, out GradientVertex v1, out GradientVertex v2)
        {
            SyncVertexColors();

            if (_vertices.Count == 3)
            {
                v0 = _vertices[0];
                v1 = _vertices[1];
                v2 = _vertices[2];
                return true;
            }

            v0 = v1 = v2 = default;
            StatusText.Text = "Сначала задайте треугольник: три клика по холсту";
            InfoText.Text = string.Empty;
            return false;
        }

        /// <summary>Переносит выбранные в ComboBox цвета в текущие вершины (struct → создаём новую).</summary>
        private void SyncVertexColors()
        {
            for (int i = 0; i < _vertices.Count && i < 3; i++)
                _vertices[i] = new GradientVertex(_vertices[i].Position, VertexColor(i));
        }

        /// <summary>Красит цветные плашки A/B/C текущими выбранными цветами.</summary>
        private void UpdateSwatches()
        {
            SwatchA.Background = new SolidColorBrush(VertexColor(0));
            SwatchB.Background = new SolidColorBrush(VertexColor(1));
            SwatchC.Background = new SolidColorBrush(VertexColor(2));
        }

        /// <summary>Обновляет положение и видимость подписей A/B/C.</summary>
        private void UpdateLabels()
        {
            PlaceLabel(LabelA, 0);
            PlaceLabel(LabelB, 1);
            PlaceLabel(LabelC, 2);
        }

        /// <summary>Ставит подпись у вершины со сдвигом; скрывает подпись, если вершины нет.</summary>
        private void PlaceLabel(TextBlock label, int index)
        {
            if (index >= _vertices.Count)
            {
                label.Visibility = Visibility.Collapsed;
                return;
            }

            Point p = _vertices[index].Position;
            label.Visibility = Visibility.Visible;
            Canvas.SetLeft(label, p.X + 7);
            Canvas.SetTop(label, p.Y - 20);
        }

        /// <summary>Пишет в статус подсказку по числу уже поставленных вершин.</summary>
        private void UpdateStatus()
        {
            StatusText.Text = _vertices.Count switch
            {
                0 => "Кликните на холсте, чтобы задать вершину A",
                1 => "Вершина A задана. Кликните, чтобы задать вершину B",
                2 => "Заданы вершины A и B. Кликните, чтобы задать вершину C",
                _ => "Треугольник залит градиентом. Кликните, чтобы начать новый"
            };
        }

        /// <summary>Выбранный в ComboBox алгоритм растеризации (читается из Tag пункта).</summary>
        private TriangleRasterizer SelectedRasterizer
            => AlgorithmCombo.SelectedItem is ComboBoxItem item
               && Enum.TryParse(item.Tag as string, out TriangleRasterizer rasterizer)
                ? rasterizer
                : TriangleRasterizer.ScanLineBarycentric;

        /// <summary>Человекочитаемое имя выбранного алгоритма для строки состояния.</summary>
        private string SelectedRasterizerName
            => SelectedRasterizer == TriangleRasterizer.EdgeFunctions
                ? "полуплоскости (edge functions)"
                : "сканлайн + барицентрические";

        /// <summary>Цвет вершины по её индексу (0=A, 1=B, 2=C).</summary>
        private Color VertexColor(int index)
            => (ComboOf(index).SelectedItem as ColorOption)?.Color ?? Colors.Black;

        /// <summary>Возвращает ComboBox цвета по индексу вершины.</summary>
        private ComboBox ComboOf(int index) => index switch
        {
            0 => ColorComboA,
            1 => ColorComboB,
            _ => ColorComboC
        };

        /// <summary>Раздаёт трём вершинам три разных случайных цвета (тасование палитры).</summary>
        private void RandomizeColors()
        {
            var indexes = Enumerable.Range(0, Palette.Length).ToArray();
            for (int i = indexes.Length - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (indexes[i], indexes[j]) = (indexes[j], indexes[i]);
            }

            _syncingColors = true;
            for (int i = 0; i < 3; i++)
                ComboOf(i).SelectedIndex = indexes[i];
            _syncingColors = false;
        }

        /// <summary>Случайная точка с отступом 20 px от краёв холста.</summary>
        private Point RandomPoint() => new(_random.Next(20, GradientCanvas.Width - 20),
                                          _random.Next(20, GradientCanvas.Height - 20));

        /// <summary>Удвоенная площадь треугольника через крестовое произведение — мера «невырожденности».</summary>
        private static double DoubleArea(Point a, Point b, Point c)
            => (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
    }
}
