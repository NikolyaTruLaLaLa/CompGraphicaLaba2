using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using laba2.Core.Imaging;

namespace laba2.Imaging
{
    /// <summary>
    /// WPF-хелперы для работы с BitmapSource/BitmapImage.
    /// Никакой логики предметной области — только конвертация и упаковка.
    /// </summary>
    public static class BitmapFactory
    {
        /// <summary>Загружает изображение с диска, не блокируя файл, и замораживает его.</summary>
        public static BitmapImage FromFile(string path)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }

        /// <summary>Конвертирует BitmapSource в замороженный BitmapImage (для биндинга).</summary>
        public static BitmapImage ToBitmapImage(BitmapSource source)
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

        /// <summary>Создаёт BitmapImage из BGRA-массива.</summary>
        public static BitmapImage FromBgra(byte[] bgra, int width, int height)
        {
            var src = BitmapSource.Create(width, height, 96, 96,
                PixelFormats.Bgra32, null, bgra, width * 4);
            return ToBitmapImage(src);
        }

        /// <summary>
        /// Создаёт монохромное изображение по одному каналу.
        /// channelIndex: 0 — Blue, 1 — Green, 2 — Red (порядок BGRA).
        /// </summary>
        public static BitmapImage ChannelImage(byte[] channelData, int width, int height, int channelIndex)
        {
            var bgra = RgbChannelProcessor.BuildBgraFromChannel(channelData, channelIndex);
            return FromBgra(bgra, width, height);
        }
    }
}