using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LABA_3.Drawing
{
    /// <summary>
    /// Общий пиксельный холст для всех заданий. Хранит картинку в виде массива
    /// байтов BGRA и показывает её через <see cref="WriteableBitmap"/>.
    /// Конкретная фигура и её алгоритмы (отрезок, треугольник, заливка ...)
    /// добавляются в классе-наследнике.
    /// </summary>
    /// <remarks>
    /// Формат пикселя — BGRA (синий, зелёный, красный, альфа) по 1 байту на канал,
    /// ровно то, что ожидает <see cref="PixelFormats.Bgra32"/>. Порядок байтов
    /// важен: индекс пикселя (x, y) в массиве равен <c>(y * Width + x) * 4</c>.
    /// Наследники работают с буфером через <see cref="Pixels"/>, <see cref="SetPixel(int,int,Color)"/>
    /// и <see cref="BlendPixel"/>, а на экран выводят одним вызовом <see cref="UpdateBitmap"/>.
    /// </remarks>
    public abstract class PixelCanvas
    {
        /// <summary>Ширина холста в пикселях (общая для всех заданий).</summary>
        public const int Width = 600;

        /// <summary>Высота холста в пикселях (общая для всех заданий).</summary>
        public const int Height = 450;

        /// <summary>Количество байт в одной строке пикселей (ширина × 4 канала).</summary>
        protected const int Stride = Width * 4;

        /// <summary>Общее число пикселей на холсте (для массивов того же размера).</summary>
        protected const int PixelCount = Width * Height;

        /// <summary>Маркер «пиксель не закрашен». Цветом быть не может: упакованный цвет неотрицателен.</summary>
        protected const int NoPixel = -1;

        private readonly WriteableBitmap _bmp;
        private readonly byte[] _pixels = new byte[Stride * Height];

        /// <summary>Готовая картинка для показа в <c>Image</c>/<c>Canvas</c>.</summary>
        public WriteableBitmap Bitmap => _bmp;

        /// <summary>Пиксельный буфер BGRA — прямой доступ для алгоритмов наследника.</summary>
        protected byte[] Pixels => _pixels;

        /// <summary>Создаёт холст, заливает его белым и готовит картинку к показу.</summary>
        protected PixelCanvas()
        {
            _bmp = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            FillWhite();
            UpdateBitmap();
        }

        /// <summary>Заливает весь холст белым фоном и показывает результат.</summary>
        /// <remarks>Виртуальный: наследники могут сбросить вместе с фоном свои счётчики.</remarks>
        public virtual void Clear()
        {
            FillWhite();
            UpdateBitmap();
        }

        /// <summary>Копирует пиксельный буфер в картинку (одна операция на кадр, а не на пиксель).</summary>
        protected void UpdateBitmap()
        {
            _bmp.WritePixels(new Int32Rect(0, 0, Width, Height), _pixels, Stride, 0);
        }

        /// <summary>Записывает пиксель заданным цветом. За пределами холста — ничего не делает.</summary>
        /// <param name="x">Координата по горизонтали.</param>
        /// <param name="y">Координата по вертикали.</param>
        /// <param name="color">Цвет пикселя (канал альфа переносится как есть).</param>
        protected void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int index = (y * Width + x) * 4;
            _pixels[index] = color.B;
            _pixels[index + 1] = color.G;
            _pixels[index + 2] = color.R;
            _pixels[index + 3] = color.A;
        }

        /// <summary>Записывает пиксель упакованным значением AARRGGBB. За пределами холста — ничего не делает.</summary>
        /// <param name="x">Координата по горизонтали.</param>
        /// <param name="y">Координата по вертикали.</param>
        /// <param name="argb">Цвет в упаковке AARRGGBB (как отдаёт <see cref="PackColor"/>).</param>
        protected void SetPixel(int x, int y, int argb)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int index = (y * Width + x) * 4;
            _pixels[index] = (byte)(argb & 0xFF);
            _pixels[index + 1] = (byte)((argb >> 8) & 0xFF);
            _pixels[index + 2] = (byte)((argb >> 16) & 0xFF);
            _pixels[index + 3] = (byte)((argb >> 24) & 0xFF);
        }

        /// <summary>
        /// Смешивает цвет пикселя с тем, что уже нарисовано, по заданной
        /// прозрачности. Используется для сглаживания (алгоритм Ву).
        /// </summary>
        /// <param name="x">Координата по горизонтали.</param>
        /// <param name="y">Координата по вертикали.</param>
        /// <param name="color">Накладываемый цвет.</param>
        /// <param name="alpha">Доля нового цвета: 0 — прозрачный, 1 — полностью перекрывает.</param>
        protected void BlendPixel(int x, int y, Color color, float alpha)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height || alpha <= 0) return;

            int index = (y * Width + x) * 4;
            _pixels[index] = (byte)(color.B * alpha + _pixels[index] * (1 - alpha));
            _pixels[index + 1] = (byte)(color.G * alpha + _pixels[index + 1] * (1 - alpha));
            _pixels[index + 2] = (byte)(color.R * alpha + _pixels[index + 2] * (1 - alpha));
            _pixels[index + 3] = 255;
        }

        /// <summary>
        /// Рисует отрезок целочисленным алгоритмом Брезенхема (те же точки,
        /// что в задании 2). Двигается по целым пикселям без дробных вычислений.
        /// </summary>
        /// <param name="x0">X начала отрезка.</param>
        /// <param name="y0">Y начала отрезка.</param>
        /// <param name="x1">X конца отрезка.</param>
        /// <param name="y1">Y конца отрезка.</param>
        /// <param name="argb">Цвет отрезка в упаковке AARRGGBB.</param>
        /// <remarks>Рисует в буфер, но не показывает его: вызывающий сам решает, когда вызвать <see cref="UpdateBitmap"/>.</remarks>
        protected void DrawLineBresenham(int x0, int y0, int x1, int y1, int argb)
        {
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(x0, y0, argb);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>Упаковывает <see cref="Color"/> в одно число AARRGGBB — как хранит упакованный <see cref="SetPixel(int,int,int)"/>.</summary>
        /// <param name="color">Цвет для упаковки.</param>
        /// <returns>Значение AARRGGBB.</returns>
        protected static int PackColor(Color color)
            => unchecked((int)((uint)color.A << 24 | (uint)color.R << 16 | (uint)color.G << 8 | color.B));

        /// <summary>Заливает буфер белым (все каналы по 255), не показывая результат.</summary>
        private void FillWhite()
        {
            for (int i = 0; i < _pixels.Length; i += 4)
            {
                _pixels[i] = 255;     // B
                _pixels[i + 1] = 255; // G
                _pixels[i + 2] = 255; // R
                _pixels[i + 3] = 255; // A
            }
        }
    }
}
