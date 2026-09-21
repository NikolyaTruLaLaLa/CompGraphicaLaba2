using System;
using laba2.Core.Histogram;

namespace laba2.Core.Imaging
{
    /// <summary>
    /// Результат разделения интерливного BGRA-массива на независимые каналы R, G, B.
    /// </summary>
    public sealed class RgbChannels
    {
        public byte[] R { get; }
        public byte[] G { get; }
        public byte[] B { get; }
        public int Width { get; }
        public int Height { get; }

        public RgbChannels(byte[] r, byte[] g, byte[] b, int width, int height)
        {
            R = r ?? throw new ArgumentNullException(nameof(r));
            G = g ?? throw new ArgumentNullException(nameof(g));
            B = b ?? throw new ArgumentNullException(nameof(b));
            Width = width;
            Height = height;
        }
    }

    /// <summary>
    /// Тройка гистограмм по каналам R, G, B.
    /// </summary>
    public sealed class RgbHistograms
    {
        public HistogramData Red { get; }
        public HistogramData Green { get; }
        public HistogramData Blue { get; }

        public RgbHistograms(HistogramData red, HistogramData green, HistogramData blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }
    }

    /// <summary>
    /// Сервис выделения цветовых каналов из изображения и построения гистограмм.
    /// Не зависит от WPF — используется как из ViewModel, так и в тестах.
    /// </summary>
    public static class RgbChannelProcessor
    {
        /// <summary>
        /// Разбирает интерливный массив BGRA (4 байта на пиксель) на три независимых канала.
        /// </summary>
        public static RgbChannels ExtractFromBgra(byte[] bgra, int width, int height)
        {
            if (bgra == null) throw new ArgumentNullException(nameof(bgra));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            int pixelCount = width * height;
            if (bgra.Length < pixelCount * 4)
                throw new ArgumentException("Размер массива меньше ожидаемого для BGRA.", nameof(bgra));

            var r = new byte[pixelCount];
            var g = new byte[pixelCount];
            var b = new byte[pixelCount];

            for (int i = 0; i < pixelCount; i++)
            {
                int o = i * 4;
                b[i] = bgra[o];     // Blue
                g[i] = bgra[o + 1]; // Green
                r[i] = bgra[o + 2]; // Red
            }

            return new RgbChannels(r, g, b, width, height);
        }

        /// <summary>
        /// Строит гистограммы по каждому из каналов R, G, B.
        /// </summary>
        public static RgbHistograms BuildHistograms(RgbChannels channels, int maxVal = 256, int binCount = 16)
        {
            if (channels == null) throw new ArgumentNullException(nameof(channels));

            var red = HistogramBuilder.BuildFromBytes(channels.R, maxVal, binCount, "Red Channel");
            var green = HistogramBuilder.BuildFromBytes(channels.G, maxVal, binCount, "Green Channel");
            var blue = HistogramBuilder.BuildFromBytes(channels.B, maxVal, binCount, "Blue Channel");

            return new RgbHistograms(red, green, blue);
        }

        /// <summary>
        /// Формирует BGRA-массив, в котором заполнен только один канал,
        /// остальные — нули, alpha = 255. Готов для BitmapSource.Create.
        /// </summary>
        /// <param name="channel">Данные одного канала.</param>
        /// <param name="channelIndex">0 — Blue, 1 — Green, 2 — Red (порядок BGRA).</param>
        /// <param name="alpha">Значение альфа-канала.</param>
        public static byte[] BuildBgraFromChannel(byte[] channel, int channelIndex, byte alpha = 255)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (channelIndex < 0 || channelIndex > 2)
                throw new ArgumentOutOfRangeException(nameof(channelIndex));

            var bgra = new byte[channel.Length * 4];
            for (int i = 0; i < channel.Length; i++)
            {
                bgra[i * 4 + channelIndex] = channel[i];
                bgra[i * 4 + 3] = alpha;
            }
            return bgra;
        }
    }
}