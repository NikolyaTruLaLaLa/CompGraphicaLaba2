using System;

namespace LABA_6.Core.Rendering
{
    public class FrameBuffer
    {
        public int Width { get; }
        public int Height { get; }

        public int[] Color { get; }

        public float[] Depth { get; }

        public FrameBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Color = new int[width * height];
            Depth = new float[width * height];
        }

        public void Clear(int bgColor = unchecked((int)0xFF000000))
        {
            for (int i = 0; i < Color.Length; i++)
            {
                Color[i] = bgColor;
                Depth[i] = float.MaxValue;
            }
        }

        public void SetPixel(int x, int y, float z, int color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;

            int index = y * Width + x;

            if (z < Depth[index])
            {
                Depth[index] = z;
                Color[index] = color;
            }
        }
    }
}