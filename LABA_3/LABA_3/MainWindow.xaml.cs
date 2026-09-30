using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_3
{
    public enum EditorMode { None, DrawRegion, FillColor, FillImage, Boundary }

    public partial class MainWindow : Window
    {
        // ===== Холст =====
        private WriteableBitmap _bitmap;
        private int[] _pixels;
        private int _stride;

        // ===== Режим =====
        private EditorMode _mode = EditorMode.None;

        // ===== Палитра для выбора цвета заливки =====
        private static readonly Color[] Palette = new Color[]
        {
            Colors.Red,
            Colors.Green,
            Colors.Blue,
            Colors.Orange,
            Colors.Purple,
            Colors.Black,
            Colors.Yellow,
            Colors.Cyan
        };
        private int _paletteIndex = 0;

        // ===== Паттерн для заливки рисунком (1б) =====
        private int[] _patternPixels;
        private int _patternWidth;
        private int _patternHeight;

        public MainWindow()
        {
            InitializeComponent();
            InitEmptyCanvas(800, 600);
        }

        // ============================================================
        //  ХОЛСТ
        // ============================================================
        private void InitEmptyCanvas(int w, int h)
        {
            var bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            SetBitmap(bmp);

            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = unchecked((int)0xFFFFFFFF);

            CommitFull();
            TxtStatus.Text = "Холст создан";
        }

        private void SetBitmap(WriteableBitmap wb)
        {
            _bitmap = wb;
            _stride = wb.PixelWidth * 4;
            _pixels = new int[wb.PixelWidth * wb.PixelHeight];
            wb.CopyPixels(_pixels, _stride, 0);

            ImgCanvas.Source = _bitmap;
            TxtCanvasSize.Text = "Холст: " + wb.PixelWidth + " × " + wb.PixelHeight;
        }

        private void CommitFull()
        {
            if (_bitmap == null || _pixels == null) return;
            _bitmap.WritePixels(
                new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight),
                _pixels, _stride, 0);
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
                var wb = LoadBitmapFromFile(dlg.FileName);
                SetBitmap(wb);
                TxtStatus.Text = "Загружено: " + Path.GetFileName(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось загрузить:\n" + ex.Message,
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
                var wb = LoadBitmapFromFile(dlg.FileName);

                _patternWidth = wb.PixelWidth;
                _patternHeight = wb.PixelHeight;
                _patternPixels = new int[_patternWidth * _patternHeight];
                wb.CopyPixels(_patternPixels, _patternWidth * 4, 0);

                TxtStatus.Text = "Паттерн: " + Path.GetFileName(dlg.FileName) +
                                 " (" + _patternWidth + "×" + _patternHeight + ")";
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось загрузить паттерн:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private WriteableBitmap LoadBitmapFromFile(string fileName)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.UriSource = new Uri(fileName, UriKind.Absolute);
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();

            var converted = new FormatConvertedBitmap(img, PixelFormats.Bgra32, null, 0);
            converted.Freeze();

            return new WriteableBitmap(converted);
        }

        // ============================================================
        //  ОЧИСТКА
        // ============================================================
        private void BtnClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null || _pixels == null) return;

            for (int i = 0; i < _pixels.Length; i++)
                _pixels[i] = unchecked((int)0xFFFFFFFF);

            CommitFull();

            _polygon.Clear();
            TxtStatus.Text = "Холст очищен";
        }

        // ============================================================
        //  СОХРАНЕНИЕ
        // ============================================================
        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            if (_bitmap == null) return;

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

                encoder.Frames.Add(BitmapFrame.Create(_bitmap));

                using (var fs = File.Create(dlg.FileName))
                {
                    encoder.Save(fs);
                }

                TxtStatus.Text = "Сохранено: " + Path.GetFileName(dlg.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Не удалось сохранить:\n" + ex.Message,
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
            _mode = EditorMode.DrawRegion;
            SetStatus("Режим: рисование области");
        }
        private void TglDrawRegion_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mode == EditorMode.DrawRegion) _mode = EditorMode.None;
        }

        private void TglFillImage_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillColor.IsChecked = false;
            TglPickBoundary.IsChecked = false;
            _mode = EditorMode.FillImage;
            SetStatus("Режим: заливка рисунком (клик внутри области)");
        }
        private void TglFillImage_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mode == EditorMode.FillImage) _mode = EditorMode.None;
        }

        private void TglFillColor_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillImage.IsChecked = false;
            TglPickBoundary.IsChecked = false;
            _mode = EditorMode.FillColor;
            SetStatus("Режим: заливка цветом (клик внутри области)");
        }
        private void TglFillColor_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mode == EditorMode.FillColor) _mode = EditorMode.None;
        }

        private void TglPickBoundary_Checked(object sender, RoutedEventArgs e)
        {
            TglDrawRegion.IsChecked = false;
            TglFillImage.IsChecked = false;
            TglFillColor.IsChecked = false;
            _mode = EditorMode.Boundary;
            SetStatus("Режим: обход границы (клик по контуру)");
        }
        private void TglPickBoundary_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_mode == EditorMode.Boundary) _mode = EditorMode.None;
        }

        // ============================================================
        //  ЦВЕТ ЗАЛИВКИ (палитра)
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
        //  ВСПОМОГАТЕЛЬНОЕ
        // ============================================================
        private void SetStatus(string text) { TxtStatus.Text = text; }
    }
}