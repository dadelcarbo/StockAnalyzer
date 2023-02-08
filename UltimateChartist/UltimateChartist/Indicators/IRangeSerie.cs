using System;
using System.Windows.Media;

namespace UltimateChartist.Indicators
{
    public interface IRangeReentrySerie
    {
        double[] High { get; }
        double[] Low { get; }
        double[] Reentry { get; }
    }
    public interface IRangeSerie
    {
        double[] High { get; }
        double[] Low { get; }
    }
    public class ValueSerie
    {
        public DateTime[] Dates { get; set; }
        public string Name { get; set; }
        public double[] Values { get; set; }
        public Brush Brush { get; set; }
    }
}
