namespace laba2.Core.Histogram
{
    public static class HistogramBuilder
    {
        public static HistogramData BuildFromBytes(byte[] pixels, int elCount = 256, int binCount = 16, string title = "")
        {
            if ((elCount % binCount) != 0) throw new ArgumentException($"{nameof(elCount)} should be divided for {nameof(binCount)}");

            SortedDictionary<string, int> data = new SortedDictionary<string, int> { };
            int diap = elCount / binCount;
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
