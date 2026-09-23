namespace laba2.Core.Histogram
{
    public static class HistogramBuilder
    {

        // pixels - массив произвольной длины с числами/пикселями
        // maxVal - максимальное значение, которое есть в pixels. Надо чтобы это число делилось на binCOunt. Если что можете взять с запасом.
        // binCOunt - количество прямоугольников на графике гистограммы. Как видите, каждый прямоугольник отвечает за какой-то промежуток.
        // title - заголовок гистограммы
        public static HistogramData BuildFromBytes(byte[] pixels, int maxVal = 256, int binCount = 16, string title = "")
        {
            if ((maxVal % binCount) != 0) throw new ArgumentException($"{nameof(maxVal)} should be divided for {nameof(binCount)}");

            SortedDictionary<string, int> data = new SortedDictionary<string, int> { };
            int diap = maxVal / binCount;
            int[] count = new int[binCount];
            for (int i = 0; i < count.Length; ++i)
                count[i] = 0;

            for (int i = 0; i < pixels.Length; ++i)
            {
                count[pixels[i] / diap]++;
            }

            for (int i = 0; i < count.Length; ++i)
            {
                int start = i * diap; int end = (i + 1) * diap;
                if (start != end)
                    data[$"{start}-{end}"] = count[i];
                else
                    data[$"{start}"] = count[i];
            }

            return new HistogramData(data, 0, binCount, title);
        }
    }
}
