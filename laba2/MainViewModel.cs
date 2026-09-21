using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using laba2.Core.Histogram;
using laba2.Core.Imaging;
using Microsoft.Win32;

namespace laba2
{
    /// <summary>Универсальная реализация ICommand для MVVM-привязок.</summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();
        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    internal class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadImageCommand { get; }
        public ICommand SaveResultCommand { get; }
        public ICommand ResetHsvCommand { get; }

        // ---- Задание 2 ----
        public BitmapImage? OriginalImage { get; private set; }
        public BitmapImage? RedChannelImage { get; private set; }
        public BitmapImage? GreenChannelImage { get; private set; }
        public BitmapImage? BlueChannelImage { get; private set; }

        public HistogramData? RedHistogram { get; private set; }
        public HistogramData? GreenHistogram { get; private set; }
        public HistogramData? BlueHistogram { get; private set; }

        // ---- Задание 3 ----
        public BitmapImage? HsvPreview { get; private set; }

        private byte[]? _sourceBgra;
        private int _sourceWidth;
        private int _sourceHeight;

        private double _hueShift;
        public double HueShift
        {
            get => _hueShift;
            set { if (Set(ref _hueShift, value)) ApplyHsv(); }
        }

        private double _saturationShift;
        public double SaturationShift
        {
            get => _saturationShift;
            set { if (Set(ref _saturationShift, value)) ApplyHsv(); }
        }

        private double _valueShift;
        public double ValueShift
        {
            get => _valueShift;
            set { if (Set(ref _valueShift, value)) ApplyHsv(); }
        }

        public MainViewModel()
        {
            LoadImageCommand = new RelayCommand(LoadImage);
            SaveResultCommand = new RelayCommand(SaveResult, () => _sourceBgra != null);
            ResetHsvCommand = new RelayCommand(ResetHsv, () => _sourceBgra != null);
        }

        private void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение"
            };

            if (dialog.ShowDialog() == true)
                ProcessImage(dialog.FileName);
        }

        private void ProcessImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();

            var formatted = new FormatConvertedBitmap(bitmap, PixelFormats.Bgra32, null, 0);
            int width = formatted.PixelWidth;
            int height = formatted.PixelHeight;
            int stride = width * 4;
            byte[] bgra = new byte[height * stride];
            formatted.CopyPixels(bgra, stride, 0);

            var channels = RgbChannelProcessor.ExtractFromBgra(bgra, width, height);
            var histograms = RgbChannelProcessor.BuildHistograms(channels);

            RedHistogram = histograms.Red;
            GreenHistogram = histograms.Green;
            BlueHistogram = histograms.Blue;

            OriginalImage = ConvertToBitmapImage(formatted);
            RedChannelImage = CreateChannelImage(channels.R, width, height, 2);
            GreenChannelImage = CreateChannelImage(channels.G, width, height, 1);
            BlueChannelImage = CreateChannelImage(channels.B, width, height, 0);

            OnPropertyChanged(nameof(OriginalImage));
            OnPropertyChanged(nameof(RedChannelImage));
            OnPropertyChanged(nameof(GreenChannelImage));
            OnPropertyChanged(nameof(BlueChannelImage));
            OnPropertyChanged(nameof(RedHistogram));
            OnPropertyChanged(nameof(GreenHistogram));
            OnPropertyChanged(nameof(BlueHistogram));

            _sourceBgra = bgra;
            _sourceWidth = width;
            _sourceHeight = height;

            _hueShift = 0;
            _saturationShift = 0;
            _valueShift = 0;
            OnPropertyChanged(nameof(HueShift));
            OnPropertyChanged(nameof(SaturationShift));
            OnPropertyChanged(nameof(ValueShift));

            ApplyHsv();
        }

        private void ApplyHsv()
        {
            if (_sourceBgra == null) return;

            var adj = new HsvAdjustment
            {
                HueShift = HueShift,
                SaturationShift = SaturationShift,
                ValueShift = ValueShift
            };
            var adjusted = HsvImageProcessor.Adjust(_sourceBgra, adj);
            var bmp = BitmapSource.Create(_sourceWidth, _sourceHeight, 96, 96,
                PixelFormats.Bgra32, null, adjusted, _sourceWidth * 4);

            HsvPreview = ConvertToBitmapImage(bmp);
            OnPropertyChanged(nameof(HsvPreview));
        }

        private void ResetHsv()
        {
            _hueShift = 0;
            _saturationShift = 0;
            _valueShift = 0;
            OnPropertyChanged(nameof(HueShift));
            OnPropertyChanged(nameof(SaturationShift));
            OnPropertyChanged(nameof(ValueShift));
            ApplyHsv();
        }

        private void SaveResult()
        {
            if (_sourceBgra == null) return;

            var dlg = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp",
                Title = "Сохранить результат",
                FileName = "hsv_result",
                DefaultExt = ".png"
            };
            if (dlg.ShowDialog() != true) return;

            var adj = new HsvAdjustment
            {
                HueShift = HueShift,
                SaturationShift = SaturationShift,
                ValueShift = ValueShift
            };
            var adjusted = HsvImageProcessor.Adjust(_sourceBgra, adj);
            var bmp = BitmapSource.Create(_sourceWidth, _sourceHeight, 96, 96,
                PixelFormats.Bgra32, null, adjusted, _sourceWidth * 4);

            BitmapEncoder encoder = dlg.FilterIndex switch
            {
                2 => new JpegBitmapEncoder { QualityLevel = 95 },
                3 => new BmpBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            using var fs = File.Create(dlg.FileName);
            encoder.Save(fs);
        }

        private BitmapImage CreateChannelImage(byte[] channelData, int width, int height, int channelIndex)
        {
            var bgra = RgbChannelProcessor.BuildBgraFromChannel(channelData, channelIndex);
            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, bgra, width * 4);
            return ConvertToBitmapImage(bitmap);
        }

        private BitmapImage ConvertToBitmapImage(BitmapSource source)
        {
            var image = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(stream);
                stream.Position = 0;

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }
    }
}