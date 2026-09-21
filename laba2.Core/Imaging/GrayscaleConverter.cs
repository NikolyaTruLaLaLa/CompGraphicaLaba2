using laba2.Core.Histogram;

namespace laba2.Core.Imaging
{
    /// <summary>
    /// Преобразования RGB в оттенки серого.
    /// </summary>
    public static class GrayscaleConverter
    {
        /// <summary>
        /// NTSC формула: Y = 0.299*R + 0.587*G + 0.114*B
        /// </summary>
        public static byte[] ToGrayscaleNTSC(byte[] bgra, int width, int height)
        {
            if (bgra == null) throw new ArgumentNullException(nameof(bgra));
            int pixelCount = width * height;
            if (bgra.Length < pixelCount * 4)
                throw new ArgumentException("Размер массива меньше ожидаемого для BGRA.", nameof(bgra));

            var gray = new byte[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                int a = i * 4;
                byte b = bgra[a];
                byte g = bgra[a + 1];
                byte r = bgra[a + 2];
                gray[i] = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
            }
            return gray;
        }

        /// <summary>
        /// HDTV формула: Y = 0.2126*R + 0.7152*G + 0.0722*B
        /// </summary>
        public static byte[] ToGrayscaleHDTV(byte[] bgra, int width, int height)
        {
            if (bgra == null) throw new ArgumentNullException(nameof(bgra));
            int pixelCount = width * height;
            if (bgra.Length < pixelCount * 4)
                throw new ArgumentException("Размер массива меньше ожидаемого для BGRA", nameof(bgra));

            var gray = new byte[pixelCount];
            for (int i = 0; i < pixelCount; i++)
            {
                int a = i * 4;
                byte b = bgra[a];
                byte g = bgra[a + 1];
                byte r = bgra[a + 2];
                gray[i] = (byte)(0.2126 * r + 0.7152 * g + 0.0722 * b);
            }
            return gray;
        }

        public static byte[] Difference(byte[] gray1, byte[] gray2)
        {
            if (gray1 == null) throw new ArgumentNullException(nameof(gray1));
            if (gray2 == null) throw new ArgumentNullException(nameof(gray2));
            if (gray1.Length != gray2.Length)
                throw new ArgumentException("Массивы должны иметь одинаковую длину");

            var diff = new byte[gray1.Length];
            for (int i = 0; i < gray1.Length; i++)
            {
                diff[i] = (byte)Math.Abs(gray1[i] - gray2[i]);
            }
            return diff;
        }

        public static HistogramData BuildHistogram(byte[] gray, int maxVal = 256, int binCount = 16, string title = "")
        {
            if (gray == null) throw new ArgumentNullException(nameof(gray));
            return HistogramBuilder.BuildFromBytes(gray, maxVal, binCount, title);
        }

        public static byte[] BuildBgraFromGray(byte[] gray, byte alpha = 255)
        {
            if (gray == null) throw new ArgumentNullException(nameof(gray));

            var bgra = new byte[gray.Length * 4];
            for (int i = 0; i < gray.Length; i++)
            {
                bgra[i * 4] = gray[i];     // B
                bgra[i * 4 + 1] = gray[i]; // G
                bgra[i * 4 + 2] = gray[i]; // R
                bgra[i * 4 + 3] = alpha;   // A
            }
            return bgra;
        }
    }
}