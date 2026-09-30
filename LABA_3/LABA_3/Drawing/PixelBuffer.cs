using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Обёртка над WriteableBitmap: держит копию пикселей в int[] (ARGB),
    /// умеет читать файл, стирать, писать изменения обратно (весь холст или регион).
    /// Все алгоритмы из Drawing работают именно с этим классом.
    /// </summary>
    public class PixelBuffer
    {
        public WriteableBitmap Bitmap { get; private set; }
        public int[] Pixels { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        private int _stride;

        public PixelBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _stride = width * 4;
            Bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            Pixels = new int[width * height];
            Bitmap.CopyPixels(Pixels, _stride, 0);
        }

        public PixelBuffer(WriteableBitmap wb)
        {
            Width = wb.PixelWidth;
            Height = wb.PixelHeight;
            _stride = Width * 4;
            Bitmap = wb;
            Pixels = new int[Width * Height];
            wb.CopyPixels(Pixels, _stride, 0);
        }

        public bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public void Clear(int argb)
        {
            for (int i = 0; i < Pixels.Length; i++) Pixels[i] = argb;
            CommitFull();
        }

        public void CommitFull()
        {
            Bitmap.WritePixels(new Int32Rect(0, 0, Width, Height), Pixels, _stride, 0);
        }

        public void CommitRegion(int minX, int minY, int maxX, int maxY)
        {
            minX = Math.Max(0, minX);
            minY = Math.Max(0, minY);
            maxX = Math.Min(Width - 1, maxX);
            maxY = Math.Min(Height - 1, maxY);
            if (maxX < minX || maxY < minY) return;

            int w = maxX - minX + 1;
            int h = maxY - minY + 1;
            int[] region = new int[w * h];

            for (int y = 0; y < h; y++)
                Array.Copy(Pixels, (minY + y) * Width + minX, region, y * w, w);

            Bitmap.WritePixels(new Int32Rect(minX, minY, w, h), region, w * 4, 0);
        }

        public static WriteableBitmap LoadBitmapFromFile(string fileName)
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
    }
}