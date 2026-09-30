using System;
using System.Windows.Media;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Холст задания 2: два алгоритма построения отрезка поверх общего
    /// пиксельного холста <see cref="PixelCanvas"/> — целочисленный
    /// Брезенхем и сглаженный Ву.
    /// </summary>
    public class LineCanvas : PixelCanvas
    {
        /// <summary>Рисует отрезок целочисленным алгоритмом Брезенхема и показывает результат.</summary>
        /// <param name="x0">X начала отрезка.</param>
        /// <param name="y0">Y начала отрезка.</param>
        /// <param name="x1">X конца отрезка.</param>
        /// <param name="y1">Y конца отрезка.</param>
        /// <param name="color">Цвет отрезка.</param>
        public void DrawBresenham(int x0, int y0, int x1, int y1, Color color)
        {
            DrawLineBresenham(x0, y0, x1, y1, PackColor(color));
            UpdateBitmap();
        }

        /// <summary>Рисует один пиксель и показывает результат.</summary>
        /// <param name="x">Координата по горизонтали.</param>
        /// <param name="y">Координата по вертикали.</param>
        /// <param name="color">Цвет пикселя.</param>
        public void DrawPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color);
            UpdateBitmap();
        }

        /// <summary>Рисует сглаженный (мягкий) отрезок алгоритмом Ву.</summary>
        /// <param name="x0">X начала отрезка.</param>
        /// <param name="y0">Y начала отрезка.</param>
        /// <param name="x1">X конца отрезка.</param>
        /// <param name="y1">Y конца отрезка.</param>
        /// <remarks>Крайние пиксели линии смешиваются с фоном по доле перекрытия, поэтому линия выглядит ровнее.</remarks>
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

        /// <summary>Меняет два значения местами (для поворота отрезка в горизонтальное положение).</summary>
        private static void Swap<T>(ref T a, ref T b) { T t = a; a = b; b = t; }

        /// <summary>Целая часть числа (округление вниз).</summary>
        private static int Ipart(float x) => (int)Math.Floor(x);

        /// <summary>Дробная часть числа — доля перекрытия пикселя (0..1).</summary>
        private static float Fpart(float x) => x - Ipart(x);

        /// <summary>Дополнение дробной части до единицы (1 − Fpart).</summary>
        private static float Rfpart(float x) => 1 - Fpart(x);
    }
}
