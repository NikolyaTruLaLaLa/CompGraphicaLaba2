using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;

namespace LABA_3.Drawing
{
    public class LineCanvas
    {
        public const int Width = 600;
        public const int Height = 450;
        private const int Stride = Width * 4;

        private readonly WriteableBitmap _bmp;
        private readonly byte[] _pixels = new byte[Stride * Height];

        public WriteableBitmap Bitmap => _bmp;

        public LineCanvas()
        {
            _bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            Clear();
        }

        public void Clear()
        {
            for (int i = 0; i < _pixels.Length; i += 4)
            {
                _pixels[i] = 255;     // B
                _pixels[i + 1] = 255; // G
                _pixels[i + 2] = 255; // R
                _pixels[i + 3] = 255; // A
            }
            UpdateBitmap();
        }

        private void UpdateBitmap()
        {
            _bmp.WritePixels(new Int32Rect(0, 0, Width, Height), _pixels, Stride, 0);
        }

        public void DrawPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color);
            UpdateBitmap();
        }

        private void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            int idx = (y * Width + x) * 4;
            _pixels[idx] = color.B;
            _pixels[idx + 1] = color.G;
            _pixels[idx + 2] = color.R;
            _pixels[idx + 3] = color.A;
        }

        private void BlendPixel(int x, int y, Color color, float alpha)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || alpha <= 0) return;
            int idx = (y * Width + x) * 4;
            byte a = (byte)(255 * alpha);
            _pixels[idx] = (byte)(color.B * alpha + _pixels[idx] * (1 - alpha));
            _pixels[idx + 1] = (byte)(color.G * alpha + _pixels[idx + 1] * (1 - alpha));
            _pixels[idx + 2] = (byte)(color.R * alpha + _pixels[idx + 2] * (1 - alpha));
            _pixels[idx + 3] = 255;
        }

        public void DrawBresenham(int x0, int y0, int x1, int y1, Color color)
        {
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(x0, y0, color);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
            UpdateBitmap();
        }

        public void DrawWu(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Swap(ref x0, ref y0); Swap(ref x1, ref y1); }
            if (x0 > x1) { Swap(ref x0, ref x1); Swap(ref y0, ref y1); }

            float dx = x1 - x0, dy = y1 - y0;
            float grad = dx == 0 ? 1 : dy / dx;

            int xend = (int)Math.Round((double)x0);
            float yend = y0 + grad * (xend - x0);
            float xgap = Rfpart(x0 + 0.5f);
            int xpxl1 = xend, ypxl1 = Ipart(yend);

            if (steep)
            {
                BlendPixel(ypxl1, xpxl1, Colors.Black, Rfpart(yend) * xgap);
                BlendPixel(ypxl1 + 1, xpxl1, Colors.Black, Fpart(yend) * xgap);
            }
            else
            {
                BlendPixel(xpxl1, ypxl1, Colors.Black, Rfpart(yend) * xgap);
                BlendPixel(xpxl1, ypxl1 + 1, Colors.Black, Fpart(yend) * xgap);
            }

            float intery = yend + grad;

            xend = (int)Math.Round((double)x1);
            yend = y1 + grad * (xend - x1);
            xgap = Fpart(x1 + 0.5f);
            int xpxl2 = xend, ypxl2 = Ipart(yend);

            if (steep)
            {
                BlendPixel(ypxl2, xpxl2, Colors.Black, Rfpart(yend) * xgap);
                BlendPixel(ypxl2 + 1, xpxl2, Colors.Black, Fpart(yend) * xgap);
            }
            else
            {
                BlendPixel(xpxl2, ypxl2, Colors.Black, Rfpart(yend) * xgap);
                BlendPixel(xpxl2, ypxl2 + 1, Colors.Black, Fpart(yend) * xgap);
            }

            if (steep)
            {
                for (int x = xpxl1 + 1; x <= xpxl2 - 1; x++)
                {
                    BlendPixel(Ipart(intery), x, Colors.Black, Rfpart(intery));
                    BlendPixel(Ipart(intery) + 1, x, Colors.Black, Fpart(intery));
                    intery += grad;
                }
            }
            else
            {
                for (int x = xpxl1 + 1; x <= xpxl2 - 1; x++)
                {
                    BlendPixel(x, Ipart(intery), Colors.Black, Rfpart(intery));
                    BlendPixel(x, Ipart(intery) + 1, Colors.Black, Fpart(intery));
                    intery += grad;
                }
            }
            UpdateBitmap();
        }

        private static void Swap<T>(ref T a, ref T b) { T t = a; a = b; b = t; }
        private static int Ipart(float x) => (int)Math.Floor(x);
        private static float Fpart(float x) => x - Ipart(x);
        private static float Rfpart(float x) => 1 - Fpart(x);
    }
}