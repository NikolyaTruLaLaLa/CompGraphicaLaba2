using System.Security.Cryptography;

namespace laba2.Core.Histogram
{
    public sealed class HistogramData
    {
        public IReadOnlyDictionary<string, int> Data { get; }
        public double MinVal { get; }
        public double MaxVal { get; }
        public string Title { get; }
        public int MaxData => Data.Values.Max();
        public int BinCount => Data.Count;
        public double BinWidth => (MaxVal - MinVal) / BinCount;  


        public HistogramData(SortedDictionary<string, int> data, double minVal, double maxVal, string title = "")
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (data.Count == 0) throw new ArgumentException("Empty dictionary", nameof(data));
            if (minVal >= maxVal) throw new ArgumentException($"{nameof(minVal)} is more or equal than {nameof(maxVal)}");

            Data = data;
            MinVal = minVal;
            MaxVal = maxVal;
            Title = title;
        }

        // 0 to BinCount-1
        public double BinStart(int i) 
        { if ((i < 0) && (BinCount <= i)) throw new ArgumentException(nameof(i)); 
          return MinVal + BinWidth*i; }
        public double BinEnd(int i)
        {
            if ((i < 0) && (BinCount <= i)) throw new ArgumentException(nameof(i));
            return MinVal + BinWidth * (i+1);
        }
        public double BinMiddle(int i)
        {
            if ((i < 0) && (BinCount <= i)) throw new ArgumentException(nameof(i));
            return MinVal + BinWidth * (i+0.5);
        }

    }
}
