using System;

namespace LABA_6.Core.Models
{
    public class Texture
    {
        public int Width { get; }
        public int Height { get; }
        public int[] Pixels { get; }

        public Texture(int width, int height, int[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Sample(float u, float v)
        {
            u = u - (float)Math.Floor(u);
            v = v - (float)Math.Floor(v);

            int x = (int)(u * (Width - 1));
            int y = (int)((1.0f - v) * (Height - 1));

            x = Math.Clamp(x, 0, Width - 1);
            y = Math.Clamp(y, 0, Height - 1);

            return Pixels[y * Width + x];
        }
    }
}