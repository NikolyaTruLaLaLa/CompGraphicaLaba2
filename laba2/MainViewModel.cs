using System.ComponentModel;
using System.Runtime.CompilerServices;
using laba2.Core.Histogram;

namespace laba2
{
    class MainViewModel : INotifyPropertyChanged
    {
        public HistogramData GrayHistogram { get; }
        public HistogramData GrayHistogram2 { get; }
        public HistogramData GrayHistogram3 { get; }

        public MainViewModel()
        {
            var px1 = new byte[256];
            for (int i = 0; i < 256; i++) px1[i] = (byte)i;

            var px2 = new byte[256];
            for (int i = 0; i < 256; i++) px2[i] = (byte)(128 + 64 * Math.Sin(i / 10.0));

            var px3 = new byte[256];
            for (int i = 0; i < 256; i++) px3[i] = (byte)(128 + 127 * Math.Cos(i / 10.0));

            GrayHistogram = HistogramBuilder.BuildFromBytes(px1, 256, 16, "Linear");
            GrayHistogram2 = HistogramBuilder.BuildFromBytes(px2, 256, 16, "Sin");
            GrayHistogram3 = HistogramBuilder.BuildFromBytes(px3, 256, 16, "Cos");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
