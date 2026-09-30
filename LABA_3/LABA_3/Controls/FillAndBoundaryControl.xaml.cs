using LABA_3.Drawing;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_3.Controls
{
    public partial class FillAndBoundaryControl : UserControl
    {
        // ===== Холст =====
        private PixelBuffer _canvas;

        // ===== Рисование многоугольника =====
        private readonly List<Point> _polygon = new List<Point>();

        private const int BlackArgb = unchecked((int)0xFF000000);
        private const int WhiteArgb = unchecked((int)0xFFFFFFFF);
        private const int HighlightArgb = unchecked((int)0xFFFF0000);
        private const int BrushThickness = 3;

        // ===== Паттерн =====
        private int[] _patternPixels;
        private int _patternWidth;
        private int _patternHeight;
        private int _seedX, _seedY;

        // ===== Цвет заливки (палитра) =====
        private static readonly Color[] Palette =
        {
            Colors.Red, Colors.Green, Colors.Blue, Colors.Orange,
            Colors.Purple, Colors.Black, Colors.Yellow, Colors.Cyan
        };
        private int _paletteIndex = 0;

        public FillAndBoundaryControl()
        {
            InitializeComponent();
            InitEmptyCanvas(800, 600);
        }

        // ============================================================
        //  ХОЛСТ
        // ============================================================
        private void InitEmptyCanvas(int w, int h)
        {
            _canvas = new PixelBuffer(w, h);
            _canvas.Clear(WhiteArgb);

            ImgCanvas.Source = _canvas.Bitmap;
            TxtCanvasSize.Text = "Холст: " + w + " × " + h;
            TxtStatus.Text = "Холст создан";
        }

        private void SetCanvas(PixelBuffer buf)
        {
            _canvas = buf;
            ImgCanvas.Source = _canvas.Bitmap;
            TxtCanvasSize.Text = "Холст: " + buf.Width + " × " + buf.Height;
        }

        // ============================================================
        //  ЗАГРУЗКА ИЗОБРАЖЕНИЯ
        // ============================================================
        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Открыть изображение",
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var wb = PixelBuffer.LoadBitmapFromFile(dlg.FileName);
                SetCanvas(new PixelBuffer(wb));
                TxtStatus.Text = "Загружено: " + Path.GetFileName(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        //  ЗАГРУЗКА ПАТТЕРНА
        // ============================================================
        private void BtnLoadPattern_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Открыть рисунок для заливки",
                Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif|Все файлы|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var wb = PixelBuffer.LoadBitmapFromFile(dlg.FileName);

                _patternWidth = wb.PixelWidth;
                _patternHeight = wb.PixelHeight;
                _patternPixels = new int[_patternWidth * _patternHeight];
                wb.CopyPixels(_patternPixels, _patternWidth * 4, 0);

                TxtStatus.Text = "Паттерн: " + Path.GetFileName(dlg.FileName) +
                                 " (" + _patternWidth + "×" + _patternHeight + ")";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось загрузить паттерн:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        //  ОЧИСТКА
        // ============================================================
        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_canvas == null) return;

            _canvas.Clear(WhiteArgb);
            _polygon.Clear();
            TxtStatus.Text = "Холст очищен";
        }

        // ============================================================
        //  СОХРАНЕНИЕ
        // ============================================================
        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_canvas == null) return;

            var dlg = new SaveFileDialog
            {
                Title = "Сохранить изображение",
                Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp",
                FileName = "result.png",
                DefaultExt = ".png"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                BitmapEncoder encoder;
                string ext = Path.GetExtension(dlg.FileName).ToLower();

                if (ext == ".jpg" || ext == ".jpeg")
                {
                    var jpeg = new JpegBitmapEncoder();
                    jpeg.QualityLevel = 95;
                    encoder = jpeg;
                }
                else if (ext == ".bmp")
                {
                    encoder = new BmpBitmapEncoder();
                }
                else
                {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(_canvas.Bitmap));

                using (var fs = File.Create(dlg.FileName))
                {
                    encoder.Save(fs);
                }

                TxtStatus.Text = "Сохранено: " + Path.GetFileName(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось сохранить:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ============================================================
        //  РЕЖИМЫ
        // ============================================================
        private void TglDrawRegion_Checked(object sender, RoutedEventArgs e)
        {
            TglFillImage.IsChecked = false;
            TglFillColor.IsChecked = false;
            TglPickBoundary.IsChecked = false;
            TxtStatus.Text = "Режим: рисование области";
        }
        private void TglDrawRegion_Unchecked(object sender, RoutedEventArgs e) { }

        private void TglFillImage_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillColor.IsChecked = false;
            TglPickBoundary.IsChecked = false;
            TxtStatus.Text = "Режим: заливка рисунком (клик внутри области)";
        }
        private void TglFillImage_Unchecked(object sender, RoutedEventArgs e) { }

        private void TglFillColor_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillImage.IsChecked = false;
            TglPickBoundary.IsChecked = false;
            TxtStatus.Text = "Режим: заливка цветом (клик внутри области)";
        }
        private void TglFillColor_Unchecked(object sender, RoutedEventArgs e) { }

        private void TglPickBoundary_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillImage.IsChecked = false;
            TglFillColor.IsChecked = false;
            TxtStatus.Text = "Режим: обход границы (клик по контуру)";
        }
        private void TglPickBoundary_Unchecked(object sender, RoutedEventArgs e) { }

        // ============================================================
        //  ЦВЕТ ЗАЛИВКИ
        // ============================================================
        private int GetFillColor()
        {
            var brush = RectFillColor.Fill as SolidColorBrush;
            Color c = brush != null ? brush.Color : Colors.Red;
            return unchecked((int)0xFF000000) | (c.R << 16) | (c.G << 8) | c.B;
        }

        private void RectFillColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _paletteIndex = (_paletteIndex + 1) % Palette.Length;
            Color c = Palette[_paletteIndex];
            RectFillColor.Fill = new SolidColorBrush(c);
            TxtStatus.Text = "Цвет заливки: " + c;
            e.Handled = true;
        }

        // ============================================================
        //  МЫШЬ
        // ============================================================
        private void ImgCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_canvas == null) return;

            var p = e.GetPosition(ImgCanvas);
            int x = (int)p.X;
            int y = (int)p.Y;

            // --- Рисование области ---
            if (TglDrawRegion.IsChecked == true)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (_polygon.Count > 0)
                    {
                        Point prev = _polygon[_polygon.Count - 1];
                        LineDrawing.DrawLine(_canvas, (int)prev.X, (int)prev.Y, x, y,
                                             BlackArgb, BrushThickness);
                    }
                    _polygon.Add(p);
                    TxtStatus.Text = "Точек: " + _polygon.Count;
                    e.Handled = true;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    if (_polygon.Count < 3)
                    {
                        _polygon.Clear();
                        TxtStatus.Text = "Нужно минимум 3 точки";
                        return;
                    }
                    Point first = _polygon[0];
                    Point last = _polygon[_polygon.Count - 1];
                    LineDrawing.DrawLine(_canvas, (int)last.X, (int)last.Y, (int)first.X, (int)first.Y,
                                         BlackArgb, BrushThickness);
                    TxtStatus.Text = "Область замкнута (" + _polygon.Count + " точек)";
                    _polygon.Clear();
                }
                return;
            }

            // --- Заливка цветом (1а) ---
            if (TglFillColor.IsChecked == true && e.ChangedButton == MouseButton.Left)
            {
                int c = GetFillColor();
                ScanlineFill.Fill(_canvas, x, y, delegate (int px, int py) { return c; });
                TxtStatus.Text = "Заливка цветом из (" + x + ", " + y + ")";
                e.Handled = true;
                return;
            }

            // --- Заливка рисунком (1б) ---
            if (TglFillImage.IsChecked == true && e.ChangedButton == MouseButton.Left)
            {
                if (_patternPixels == null)
                {
                    MessageBox.Show("Сначала загрузите паттерн (кнопка «Загрузить...» в панели «Паттерн»).",
                                    "Паттерн не задан", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                _seedX = x;
                _seedY = y;
                ScanlineFill.Fill(_canvas, x, y, GetPatternColor);
                TxtStatus.Text = "Заливка рисунком из (" + x + ", " + y + ")";
                e.Handled = true;
                return;
            }

            // --- Обход границы (1в) ---
            if (TglPickBoundary.IsChecked == true && e.ChangedButton == MouseButton.Left)
            {
                TraceBoundary(x, y);
                e.Handled = true;
                return;
            }
        }

        private void ImgCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pt = e.GetPosition(ImgCanvas);
            TxtCoords.Text = "x: " + (int)pt.X + ", y: " + (int)pt.Y;
        }

        private void ImgCanvas_MouseUp(object sender, MouseButtonEventArgs e) { }

        // ============================================================
        //  ЦВЕТ ИЗ ПАТТЕРНА (1б)
        // ============================================================
        private int GetPatternColor(int x, int y)
        {
            if (_patternPixels == null || _patternWidth == 0 || _patternHeight == 0)
                return GetFillColor();

            if (ChkCyclePattern.IsChecked == true)
            {
                int dx = x - _seedX;
                int dy = y - _seedY;
                int sx = ((dx % _patternWidth) + _patternWidth) % _patternWidth;
                int sy = ((dy % _patternHeight) + _patternHeight) % _patternHeight;
                return _patternPixels[sy * _patternWidth + sx];
            }
            else
            {
                int sx = x - _seedX;
                int sy = y - _seedY;
                if (sx < 0 || sy < 0 || sx >= _patternWidth || sy >= _patternHeight)
                    return ScanlineFill.NoColor;
                return _patternPixels[sy * _patternWidth + sx];
            }
        }

        // ============================================================
        //  ОБХОД ГРАНИЦЫ (1в)
        // ============================================================
        private void TraceBoundary(int seedX, int seedY)
        {
            int bx, by;
            if (!MooreBoundary.FindNearest(_canvas, seedX, seedY, BlackArgb, 30, out bx, out by))
            {
                TxtStatus.Text = "Граница не найдена — кликните ближе к контуру";
                return;
            }

            List<Point> contour = MooreBoundary.Trace(_canvas, bx, by, BlackArgb);
            if (contour.Count == 0)
            {
                TxtStatus.Text = "Контур пустой";
                return;
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            for (int i = 0; i < contour.Count; i++)
            {
                int cx = (int)contour[i].X;
                int cy = (int)contour[i].Y;
                _canvas.Pixels[cy * _canvas.Width + cx] = HighlightArgb;

                if (cx < minX) minX = cx;
                if (cx > maxX) maxX = cx;
                if (cy < minY) minY = cy;
                if (cy > maxY) maxY = cy;
            }

            _canvas.CommitRegion(minX, minY, maxX, maxY);

            TxtStatus.Text = "Обход границы: " + contour.Count + " точек (старт " + bx + ", " + by + ")";
        }
    }
}