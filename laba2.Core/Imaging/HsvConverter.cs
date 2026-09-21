using System;

namespace laba2.Core.Imaging
{
    /// <summary>
    /// Преобразования RGB ↔ HSV.
    /// R/G/B ∈ [0, 255], H ∈ [0, 360), S ∈ [0, 1], V ∈ [0, 1].
    /// </summary>
    public static class HsvConverter
    {
        public static (double H, double S, double V) RgbToHsv(byte r, byte g, byte b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double delta = max - min;

            double h = 0.0;
            if (delta > 1e-9)
            {
                if (max == rd) h = 60 * (((gd - bd) / delta) % 6);
                else if (max == gd) h = 60 * (((bd - rd) / delta) + 2);
                else h = 60 * (((rd - gd) / delta) + 4);
            }
            if (h < 0) h += 360;

            double s = max <= 1e-9 ? 0.0 : delta / max;
            double v = max;
            return (h, s, v);
        }

        public static (byte R, byte G, byte B) HsvToRgb(double h, double s, double v)
        {
            h = ((h % 360.0) + 360.0) % 360.0;
            s = Clamp01(s);
            v = Clamp01(v);

            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = v - c;

            double rd, gd, bd;
            if (h < 60) { rd = c; gd = x; bd = 0; }
            else if (h < 120) { rd = x; gd = c; bd = 0; }
            else if (h < 180) { rd = 0; gd = c; bd = x; }
            else if (h < 240) { rd = 0; gd = x; bd = c; }
            else if (h < 300) { rd = x; gd = 0; bd = c; }
            else { rd = c; gd = 0; bd = x; }

            byte R = (byte)Math.Round((rd + m) * 255);
            byte G = (byte)Math.Round((gd + m) * 255);
            byte B = (byte)Math.Round((bd + m) * 255);
            return (R, G, B);
        }

        private static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);
    }
}