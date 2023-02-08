using System;
using System.Windows.Media;

namespace UltimateChartist.Indicators
{
    public abstract class IndicatorValueBase
    {
        public DateTime Date { get; set; }
    }
    public class IndicatorLineValue : IndicatorValueBase
    {
        public double Value { get; set; }
    }
    public class IndicatorRangeValue : IndicatorValueBase
    {
        public double Low { get; set; }

        public double High { get; set; }
    }

    public class IndicatorLineSeries
    {
        public string Name { get; set; }

        public IndicatorLineValue[] Values { get; set; }

        public Brush Brush { get; set; }

        public double Thickness { get; set; }
    }
}
