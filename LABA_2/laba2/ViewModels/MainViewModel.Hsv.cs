using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using laba2.Core.Imaging;
using laba2.Imaging;
using Microsoft.Win32;

namespace laba2.ViewModels
{
    internal partial class MainViewModel
    {
        public BitmapImage? HsvPreview { get; private set; }

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

        /// <summary>Сброс слайдеров и обновление превью при загрузке новой картинки.</summary>
        private void Hsv_OnImageLoaded(ImageSession session)
        {
            _hueShift = 0;
            _saturationShift = 0;
            _valueShift = 0;
            OnPropertyChanged(nameof(HueShift));
            OnPropertyChanged(nameof(SaturationShift));
            OnPropertyChanged(nameof(ValueShift));

            ApplyHsv();
        }

        /// <summary>Собирает текущий сдвиг H/S/V из значений слайдеров.</summary>
        private HsvAdjustment BuildAdjustment() => new HsvAdjustment
        {
            HueShift = HueShift,
            SaturationShift = SaturationShift,
            ValueShift = ValueShift
        };

        /// <summary>
        /// Возвращает BGRA-массив с применённым текущим HSV-сдвигом.
        /// Единственная точка применения сдвига — используется и превью, и сохранением.
        /// </summary>
        private byte[] GetAdjustedBgra()
        {
            if (Session == null)
                throw new InvalidOperationException("Изображение не загружено.");

            return HsvImageProcessor.Adjust(Session.Bgra, BuildAdjustment());
        }

        private void ApplyHsv()
        {
            if (Session == null) return;

            var adjusted = GetAdjustedBgra();
            HsvPreview = BitmapFactory.FromBgra(adjusted, Session.Width, Session.Height);
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
            if (Session == null) return;

            var dlg = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP (*.bmp)|*.bmp",
                Title = "Сохранить результат",
                FileName = "hsv_result",
                DefaultExt = ".png"
            };
            if (dlg.ShowDialog() != true) return;

            var adjusted = GetAdjustedBgra();
            var bmp = BitmapSource.Create(Session.Width, Session.Height, 96, 96,
                PixelFormats.Bgra32, null, adjusted, Session.Width * 4);

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
    }
}