using System;

namespace laba2.Core.Imaging
{
    /// <summary>Параметры сдвига в пространстве HSV.</summary>
    public sealed class HsvAdjustment
    {
        /// <summary>Сдвиг оттенка в градусах, обычно [-180, 180].</summary>
        public double HueShift { get; init; }

        /// <summary>Сдвиг насыщенности, [-1, 1].</summary>
        public double SaturationShift { get; init; }

        /// <summary>Сдвиг яркости, [-1, 1].</summary>
        public double ValueShift { get; init; }

        public static HsvAdjustment Identity => new HsvAdjustment();
    }

    /// <summary>
    /// Применяет сдвиги H/S/V к BGRA-изображению.
    /// </summary>
    public static class HsvImageProcessor
    {
        public static byte[] Adjust(byte[] bgra, HsvAdjustment adj)
        {
            if (bgra == null) throw new ArgumentNullException(nameof(bgra));
            if (adj == null) throw new ArgumentNullException(nameof(adj));

            if (adj.HueShift == 0 && adj.SaturationShift == 0 && adj.ValueShift == 0)
                return (byte[])bgra.Clone(); 

            var result = new byte[bgra.Length];
            for (int i = 0; i < bgra.Length; i += 4)
            {
                byte b = bgra[i];
                byte g = bgra[i + 1];
                byte r = bgra[i + 2];
                byte a = bgra[i + 3];

                var (h, s, v) = HsvConverter.RgbToHsv(r, g, b);

                h += adj.HueShift;
                s += adj.SaturationShift;
                v += adj.ValueShift;

                var (nr, ng, nb) = HsvConverter.HsvToRgb(h, s, v);

                result[i] = nb;
                result[i + 1] = ng;
                result[i + 2] = nr;
                result[i + 3] = a;
            }
            return result;
        }
    }
}