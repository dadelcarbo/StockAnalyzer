using System;
using System.Collections;
using System.Collections.Generic;
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
        public double Down { get; set; }

        public double Up { get; set; }
    }
    public class IndicatorBandValue : IndicatorRangeValue
    {
        public double Mid { get; set; }
    }


    public interface IIndicatorSeries
    {
        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }
    public class IndicatorLineSeries : IIndicatorSeries
    {
        public IndicatorLineSeries()
        {
            this.Thickness = 1;
            this.Brush = Brushes.Red;
        }
        public string Name { get; set; }

        public Brush Brush { get; set; }

        public double Thickness { get; set; }

        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }

    public class IndicatorRangeSeries : IIndicatorSeries
    {
        public IndicatorRangeSeries()
        {
            this.RangeBrush = new SolidColorBrush(Color.FromArgb(127, Colors.LightGreen.R, Colors.LightGreen.G, Colors.LightGreen.B));
        }
        public string UpName { get; set; }
        public Brush UpBrush { get; set; }
        public double UpThickness { get; set; }

        public string DownName { get; set; }
        public Brush DownBrush { get; set; }
        public double DownThickness { get; set; }

        public Brush RangeBrush { get; set; }

        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }

    public class IndicatorBandSeries : IndicatorRangeSeries
    {
        public IndicatorBandSeries()
        {
            this.MidThickness = 1;
            this.MidBrush = Brushes.LightGreen;
        }
        public string MidName { get; set; }
        public Brush MidBrush { get; set; }
        public double MidThickness { get; set; }
    }
}
