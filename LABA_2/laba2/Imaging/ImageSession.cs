using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace laba2.Imaging
{
    /// <summary>
    /// Загруженное изображение вместе с его BGRA-представлением.
    /// Один источник правды о картинке, разделяемый между фичами.
    /// </summary>
    public sealed class ImageSession
    {
        public byte[] Bgra { get; }
        public int Width { get; }
        public int Height { get; }
        public BitmapImage Original { get; }

        public ImageSession(byte[] bgra, int width, int height, BitmapImage original)
        {
            Bgra = bgra ?? throw new ArgumentNullException(nameof(bgra));
            Width = width;
            Height = height;
            Original = original ?? throw new ArgumentNullException(nameof(original));
        }

        /// <summary>
        /// Загружает изображение с диска, приводит к Bgra32 и выгружает пиксели.
        /// </summary>
        public static ImageSession Load(string path)
        {
            var source = BitmapFactory.FromFile(path);
            var formatted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = formatted.PixelWidth;
            int height = formatted.PixelHeight;
            int stride = width * 4;
            byte[] bgra = new byte[height * stride];
            formatted.CopyPixels(bgra, stride, 0);

            var original = BitmapFactory.ToBitmapImage(formatted);
            return new ImageSession(bgra, width, height, original);
        }
    }
}