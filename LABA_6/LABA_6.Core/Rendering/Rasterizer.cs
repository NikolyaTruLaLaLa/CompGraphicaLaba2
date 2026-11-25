using System;
using LABA_6.Core.Models;

namespace LABA_6.Core.Rendering
{
    public static class Rasterizer
    {
        public const int ColorBlack = unchecked((int)0xFF000000);
        public const int ColorCyan = unchecked((int)0xFF00FFFF);
        public const int ColorRed = unchecked((int)0xFFFF0000);
        public const int ColorLime = unchecked((int)0xFF00FF00);
        public const int ColorBlue = unchecked((int)0xFF0000FF);
        public const int ColorWhite = unchecked((int)0xFFFFFFFF);

        public static void DrawLine(FrameBuffer fb, int x0, int y0, float z0, int x1, int y1, float z1, int color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;

            int steps = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
            if (steps == 0) { fb.SetPixel(x0, y0, z0, color); return; }

            float dz = (z1 - z0) / steps;
            float currentZ = z0;

            while (true)
            {
                fb.SetPixel(x0, y0, currentZ, color);
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
                currentZ += dz;
            }
        }
        public static void DrawTriangle(FrameBuffer fb, Point3 v1, Point3 v2, Point3 v3, int color)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));


            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return; 

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0); 

                    // барицентрические координаты
                    float w0 = EdgeFunction(v2, v3, p);
                    float w1 = EdgeFunction(v3, v1, p);
                    float w2 = EdgeFunction(v1, v2, p);

                    if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0))
                    {
                        w0 /= area;
                        w1 /= area;
                        w2 /= area;
                        float z = w0 * v1.Z + w1 * v2.Z + w2 * v3.Z;
                        fb.SetPixel(x, y, z, color);
                    }
                }
            }
        }


        public static void DrawTriangleTextured(FrameBuffer fb,
                                        Point3 v1, Point2 uv1, float w1,
                                        Point3 v2, Point2 uv2, float w2,
                                        Point3 v3, Point2 uv3, float w3,
                                        Texture texture)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));

            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0);
                    float w0_raw = EdgeFunction(v2, v3, p);
                    float w1_raw = EdgeFunction(v3, v1, p);
                    float w2_raw = EdgeFunction(v1, v2, p);

                    if ((w0_raw >= 0 && w1_raw >= 0 && w2_raw >= 0) || (w0_raw <= 0 && w1_raw <= 0 && w2_raw <= 0))
                    {
                        float b0 = w0_raw / area;
                        float b1 = w1_raw / area;
                        float b2 = w2_raw / area;

                        float z = b0 * v1.Z + b1 * v2.Z + b2 * v3.Z;

                        // ПЕРСПЕКТИВНО-КОРРЕКТНАЯ ИНТЕРПОЛЯЦИЯ
                        float invW = b0 * (1f / w1) + b1 * (1f / w2) + b2 * (1f / w3);

                        float u = (b0 * (uv1.X / w1) + b1 * (uv2.X / w2) + b2 * (uv3.X / w3)) / invW;
                        float v = (b0 * (uv1.Y / w1) + b1 * (uv2.Y / w2) + b2 * (uv3.Y / w3)) / invW;

                        int color = texture.Sample(u, v);
                        fb.SetPixel(x, y, z, color);
                    }
                }
            }
        }

        public static void DrawTriangleTexturedGouraud(FrameBuffer fb,
                                        Point3 v1, Point2 uv1, float w1, float i1,
                                        Point3 v2, Point2 uv2, float w2, float i2,
                                        Point3 v3, Point2 uv3, float w3, float i3,
                                        Texture texture)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));

            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0);
                    float w0_raw = EdgeFunction(v2, v3, p);
                    float w1_raw = EdgeFunction(v3, v1, p);
                    float w2_raw = EdgeFunction(v1, v2, p);

                    if ((w0_raw >= 0 && w1_raw >= 0 && w2_raw >= 0) || (w0_raw <= 0 && w1_raw <= 0 && w2_raw <= 0))
                    {
                        float b0 = w0_raw / area;
                        float b1 = w1_raw / area;
                        float b2 = w2_raw / area;

                        float z = b0 * v1.Z + b1 * v2.Z + b2 * v3.Z;

                        // Перспективно-корректная интерполяция UV
                        float invW = b0 * (1f / w1) + b1 * (1f / w2) + b2 * (1f / w3);
                        float u = (b0 * (uv1.X / w1) + b1 * (uv2.X / w2) + b2 * (uv3.X / w3)) / invW;
                        float v = (b0 * (uv1.Y / w1) + b1 * (uv2.Y / w2) + b2 * (uv3.Y / w3)) / invW;

                        // Интерполируем интенсивность света
                        float intensity = b0 * i1 + b1 * i2 + b2 * i3;

                        int textureColor = texture.Sample(u, v);

                        // Умножаем цвет текстуры на свет
                        int a = (textureColor >> 24) & 0xFF;
                        int r = (int)(((textureColor >> 16) & 0xFF) * intensity);
                        int g = (int)(((textureColor >> 8) & 0xFF) * intensity);
                        int b = (int)((textureColor & 0xFF) * intensity);

                        r = Math.Clamp(r, 0, 255);
                        g = Math.Clamp(g, 0, 255);
                        b = Math.Clamp(b, 0, 255);

                        int finalColor = (a << 24) | (r << 16) | (g << 8) | b;
                        fb.SetPixel(x, y, z, finalColor);
                    }
                }
            }
        }
        public static void DrawTriangleGouraud(FrameBuffer fb,
            Point3 v1, int c1, Point3 v2, int c2, Point3 v3, int c3)
        {
            // ... (стандартные проверки Bounding Box и EdgeFunction) ...
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));

            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0);
                    float w0 = EdgeFunction(v2, v3, p);
                    float w1 = EdgeFunction(v3, v1, p);
                    float w2 = EdgeFunction(v1, v2, p);

                    if ((w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0))
                    {
                        w0 /= area; w1 /= area; w2 /= area;
                        float z = w0 * v1.Z + w1 * v2.Z + w2 * v3.Z;

                        // Интерполируем цвет
                        int color = InterpolateColor(c1, c2, c3, w0, w1, w2);
                        fb.SetPixel(x, y, z, color);
                    }
                }
            }
        }
        private static float EdgeFunction(Point3 a, Point3 b, Point3 c)
        {
            return (c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X);
        }

        private static int InterpolateColor(int c1, int c2, int c3, float b0, float b1, float b2)
        {
            int a = (int)(((c1 >> 24) & 0xFF) * b0 + ((c2 >> 24) & 0xFF) * b1 + ((c3 >> 24) & 0xFF) * b2);
            int r = (int)(((c1 >> 16) & 0xFF) * b0 + ((c2 >> 16) & 0xFF) * b1 + ((c3 >> 16) & 0xFF) * b2);
            int g = (int)(((c1 >> 8) & 0xFF) * b0 + ((c2 >> 8) & 0xFF) * b1 + ((c3 >> 8) & 0xFF) * b2);
            int b = (int)((c1 & 0xFF) * b0 + (c2 & 0xFF) * b1 + (c3 & 0xFF) * b2);
            return (a << 24) | (r << 16) | (g << 8) | b;
        }

        // Вспомогательный метод для расчета цвета пикселя (Тун и Фонг)
        private static int CalculatePhongColor(Point3 normal, Point3 lightPos, Point3 worldPos,
                                               float ambient, float lightIntensity, int baseColor, bool isToon)
        {
            // Нормализуем интерполированную нормаль
            normal = normal.Normalized();

            // Вектор к свету
            Point3 L = (lightPos - worldPos).Normalized();

            float dot = Point3.Dot(normal, L);
            float intensity = ambient + lightIntensity * Math.Max(0, dot);
            intensity = Math.Clamp(intensity, 0f, 1f);

            // Если это Toon-шейдинг, делаем свет ступенчатым
            if (isToon)
            {
                // Округляем до 3 ступеней (0.33, 0.66, 1.0)
                intensity = (float)Math.Round(intensity * 3f) / 3f;
            }

            int a = (baseColor >> 24) & 0xFF;
            int r = (int)(((baseColor >> 16) & 0xFF) * intensity);
            int g = (int)(((baseColor >> 8) & 0xFF) * intensity);
            int b = (int)((baseColor & 0xFF) * intensity);

            return (a << 24) | (Math.Clamp(r, 0, 255) << 16) | (Math.Clamp(g, 0, 255) << 8) | Math.Clamp(b, 0, 255);
        }

        public static void DrawTrianglePhong(FrameBuffer fb,
            Point3 v1, Point3 n1, Point3 w1,
            Point3 v2, Point3 n2, Point3 w2,
            Point3 v3, Point3 n3, Point3 w3,
            Point3 lightPos, float ambient, float lightInt, int color, bool isToon)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));

            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0);
                    float b0 = EdgeFunction(v2, v3, p) / area;
                    float b1 = EdgeFunction(v3, v1, p) / area;
                    float b2 = EdgeFunction(v1, v2, p) / area;

                    if (b0 >= 0 && b1 >= 0 && b2 >= 0) // Убрал проверку <=0 для простоты, так как нормали обычно обходятся CCW
                    {
                        float z = b0 * v1.Z + b1 * v2.Z + b2 * v3.Z;

                        // Интерполируем нормали и мировые координаты
                        Point3 interpNormal = n1 * b0 + n2 * b1 + n3 * b2;
                        Point3 interpWorld = w1 * b0 + w2 * b1 + w3 * b2;

                        int finalColor = CalculatePhongColor(interpNormal, lightPos, interpWorld, ambient, lightInt, color, isToon);
                        fb.SetPixel(x, y, z, finalColor);
                    }
                }
            }
        }

        public static void DrawTriangleTexturedPhong(FrameBuffer fb,
                                        Point3 v1, Point2 uv1, float w1, Point3 n1, Point3 wp1,
                                        Point3 v2, Point2 uv2, float w2, Point3 n2, Point3 wp2,
                                        Point3 v3, Point2 uv3, float w3, Point3 n3, Point3 wp3,
                                        Point3 lightPos, float ambient, float lightInt,
                                        Texture texture, bool isToon)
        {
            int minX = (int)Math.Max(0, Math.Floor(Math.Min(v1.X, Math.Min(v2.X, v3.X))));
            int maxX = (int)Math.Min(fb.Width - 1, Math.Ceiling(Math.Max(v1.X, Math.Max(v2.X, v3.X))));
            int minY = (int)Math.Max(0, Math.Floor(Math.Min(v1.Y, Math.Min(v2.Y, v3.Y))));
            int maxY = (int)Math.Min(fb.Height - 1, Math.Ceiling(Math.Max(v1.Y, Math.Max(v2.Y, v3.Y))));

            float area = EdgeFunction(v1, v2, v3);
            if (area == 0) return;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    var p = new Point3(x + 0.5f, y + 0.5f, 0);
                    float w0_raw = EdgeFunction(v2, v3, p);
                    float w1_raw = EdgeFunction(v3, v1, p);
                    float w2_raw = EdgeFunction(v1, v2, p);

                    if ((w0_raw >= 0 && w1_raw >= 0 && w2_raw >= 0) || (w0_raw <= 0 && w1_raw <= 0 && w2_raw <= 0))
                    {
                        float b0 = w0_raw / area;
                        float b1 = w1_raw / area;
                        float b2 = w2_raw / area;

                        float z = b0 * v1.Z + b1 * v2.Z + b2 * v3.Z;

                        // Перспективно-корректная интерполяция UV
                        float invW = b0 * (1f / w1) + b1 * (1f / w2) + b2 * (1f / w3);
                        float u = (b0 * (uv1.X / w1) + b1 * (uv2.X / w2) + b2 * (uv3.X / w3)) / invW;
                        float v = (b0 * (uv1.Y / w1) + b1 * (uv2.Y / w2) + b2 * (uv3.Y / w3)) / invW;

                        // Интерполяция нормали и мировой позиции
                        Point3 interpNormal = (n1 * b0 + n2 * b1 + n3 * b2).Normalized();
                        Point3 interpWorld = wp1 * b0 + wp2 * b1 + wp3 * b2;

                        // Расчет освещения
                        Point3 L = (lightPos - interpWorld).Normalized();
                        float dot = Point3.Dot(interpNormal, L);
                        float intensity = Math.Clamp(ambient + lightInt * Math.Max(0, dot), 0f, 1f);

                        if (isToon)
                        {
                            intensity = (float)Math.Round(intensity * 3f) / 3f; // 3 ступени
                        }

                        // Берем цвет из текстуры и умножаем на свет
                        int texColor = texture.Sample(u, v);
                        int a = (texColor >> 24) & 0xFF;
                        int r = (int)(((texColor >> 16) & 0xFF) * intensity);
                        int g = (int)(((texColor >> 8) & 0xFF) * intensity);
                        int b = (int)((texColor & 0xFF) * intensity);

                        int finalColor = (a << 24) | (Math.Clamp(r, 0, 255) << 16) | (Math.Clamp(g, 0, 255) << 8) | Math.Clamp(b, 0, 255);
                        fb.SetPixel(x, y, z, finalColor);
                    }
                }
            }
        }
    }
}