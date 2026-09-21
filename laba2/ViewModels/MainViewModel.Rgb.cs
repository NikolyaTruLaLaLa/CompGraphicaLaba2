using System.Windows.Media.Imaging;
using laba2.Core.Histogram;
using laba2.Core.Imaging;
using laba2.Imaging;

namespace laba2.ViewModels
{
    internal partial class MainViewModel
    {
        public BitmapImage? OriginalImage { get; private set; }
        public BitmapImage? RedChannelImage { get; private set; }
        public BitmapImage? GreenChannelImage { get; private set; }
        public BitmapImage? BlueChannelImage { get; private set; }

        public HistogramData? RedHistogram { get; private set; }
        public HistogramData? GreenHistogram { get; private set; }
        public HistogramData? BlueHistogram { get; private set; }

        /// <summary>Обновление данных задания 2 при загрузке новой картинки.</summary>
        private void Rgb_OnImageLoaded(ImageSession session)
        {
            var channels = RgbChannelProcessor.ExtractFromBgra(session.Bgra, session.Width, session.Height);
            var histograms = RgbChannelProcessor.BuildHistograms(channels);

            RedHistogram = histograms.Red;
            GreenHistogram = histograms.Green;
            BlueHistogram = histograms.Blue;

            OriginalImage = session.Original;
            RedChannelImage = BitmapFactory.ChannelImage(channels.R, session.Width, session.Height, 2);
            GreenChannelImage = BitmapFactory.ChannelImage(channels.G, session.Width, session.Height, 1);
            BlueChannelImage = BitmapFactory.ChannelImage(channels.B, session.Width, session.Height, 0);

            OnPropertyChanged(nameof(OriginalImage));
            OnPropertyChanged(nameof(RedChannelImage));
            OnPropertyChanged(nameof(GreenChannelImage));
            OnPropertyChanged(nameof(BlueChannelImage));
            OnPropertyChanged(nameof(RedHistogram));
            OnPropertyChanged(nameof(GreenHistogram));
            OnPropertyChanged(nameof(BlueHistogram));
        }
    }
}