using System;

namespace LABA_3.Drawing
{
    /// <summary>Возвращает цвет пикселя (x, y) во время заливки.</summary>
    public delegate int ColorSource(int x, int y);

    /// <summary>
    /// Рекурсивная заливка по сериям (scanline seed fill).
    /// Работает и для сплошного цвета, и для паттерна — через делегат ColorSource.
    /// </summary>
    public static class ScanlineFill
    {
        /// <summary>Спецзначение «этот пиксель не заливать».</summary>
        public const int NoColor = int.MinValue;

        public static void Fill(PixelBuffer buf, int x, int y, ColorSource source)
        {
            if (!buf.InBounds(x, y)) return;

            int oldColor = buf.Pixels[y * buf.Width + x];
            var ctx = new FillContext
            {
                Buf = buf,
                Source = source,
                Visited = new byte[buf.Width * buf.Height],
                MinX = int.MaxValue,
                MinY = int.MaxValue,
                MaxX = int.MinValue,
                MaxY = int.MinValue
            };

            FillSpan(ctx, x, y, oldColor);

            if (ctx.MaxX >= ctx.MinX)
                buf.CommitRegion(ctx.MinX, ctx.MinY, ctx.MaxX, ctx.MaxY);
        }

        private class FillContext
        {
            public PixelBuffer Buf;
            public byte[] Visited;
            public ColorSource Source;
            public int MinX, MinY, MaxX, MaxY;
        }

        private static void FillSpan(FillContext ctx, int x, int y, int oldColor)
        {
            int w = ctx.Buf.Width;
            int h = ctx.Buf.Height;
            var buf = ctx.Buf;

            if (x < 0 || y < 0 || x >= w || y >= h) return;

            int idx = y * w + x;
            if (ctx.Visited[idx] != 0) return;
            if (buf.Pixels[idx] != oldColor) return;

            int x1 = x;
            while (x1 >= 0 && buf.Pixels[y * w + x1] == oldColor && ctx.Visited[y * w + x1] == 0) x1--;
            x1++;

            int x2 = x;
            while (x2 < w && buf.Pixels[y * w + x2] == oldColor && ctx.Visited[y * w + x2] == 0) x2++;
            x2--;

            for (int i = x1; i <= x2; i++)
            {
                int c = ctx.Source(i, y);
                if (c != NoColor) buf.Pixels[y * w + i] = c;
                ctx.Visited[y * w + i] = 1;
            }

            if (x1 < ctx.MinX) ctx.MinX = x1;
            if (x2 > ctx.MaxX) ctx.MaxX = x2;
            if (y < ctx.MinY) ctx.MinY = y;
            if (y > ctx.MaxY) ctx.MaxY = y;

            ScanAdjacentRow(ctx, x1, x2, y - 1, oldColor);
            ScanAdjacentRow(ctx, x1, x2, y + 1, oldColor);
        }

        private static void ScanAdjacentRow(FillContext ctx, int x1, int x2, int y, int oldColor)
        {
            if (y < 0 || y >= ctx.Buf.Height) return;

            int w = ctx.Buf.Width;
            int x = x1;

            while (x <= x2)
            {
                int idx = y * w + x;
                if (ctx.Buf.Pixels[idx] == oldColor && ctx.Visited[idx] == 0)
                {
                    FillSpan(ctx, x, y, oldColor);
                    while (x <= x2 && !(ctx.Buf.Pixels[y * w + x] == oldColor && ctx.Visited[y * w + x] == 0)) x++;
                }
                else
                {
                    x++;
                }
            }
        }
    }
}