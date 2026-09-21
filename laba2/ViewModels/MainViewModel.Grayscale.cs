using System.Windows.Media.Imaging;
using laba2.Core.Histogram;
using laba2.Core.Imaging;
using laba2.Imaging;

namespace laba2.ViewModels
{
    internal partial class MainViewModel
    {
        public BitmapImage? GrayscaleNtscImage { get; private set; }
        public BitmapImage? GrayscaleHdtvImage { get; private set; }
        public BitmapImage? GrayscaleDifferenceImage { get; private set; }

        public HistogramData? NtscHistogram { get; private set; }
        public HistogramData? HdtvHistogram { get; private set; }
        public HistogramData? DifferenceHistogram { get; private set; }

        private void Grayscale_OnImageLoaded(ImageSession session)
        {
            var ntscGray = GrayscaleConverter.ToGrayscaleNTSC(session.Bgra, session.Width, session.Height);
            var hdtvGray = GrayscaleConverter.ToGrayscaleHDTV(session.Bgra, session.Width, session.Height);
            var diffGray = GrayscaleConverter.Difference(ntscGray, hdtvGray);

            NtscHistogram = GrayscaleConverter.BuildHistogram(ntscGray, title: "NTSC Histogram");
            HdtvHistogram = GrayscaleConverter.BuildHistogram(hdtvGray, title: "HDTV Histogram");
            DifferenceHistogram = GrayscaleConverter.BuildHistogram(diffGray, title: "Difference Histogram");

            GrayscaleNtscImage = BitmapFactory.FromBgra(GrayscaleConverter.BuildBgraFromGray(ntscGray), session.Width, session.Height);
            GrayscaleHdtvImage = BitmapFactory.FromBgra(GrayscaleConverter.BuildBgraFromGray(hdtvGray), session.Width, session.Height);
            GrayscaleDifferenceImage = BitmapFactory.FromBgra(GrayscaleConverter.BuildBgraFromGray(diffGray), session.Width, session.Height);

            OnPropertyChanged(nameof(GrayscaleNtscImage));
            OnPropertyChanged(nameof(GrayscaleHdtvImage));
            OnPropertyChanged(nameof(GrayscaleDifferenceImage));
            OnPropertyChanged(nameof(NtscHistogram));
            OnPropertyChanged(nameof(HdtvHistogram));
            OnPropertyChanged(nameof(DifferenceHistogram));
        }
    }
}