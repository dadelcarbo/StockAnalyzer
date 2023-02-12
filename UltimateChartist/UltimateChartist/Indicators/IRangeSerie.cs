using System;
using System.Collections.Generic;
using System.Windows.Media;
using Telerik.Windows.Controls;

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
    public abstract class IndicatorSeriesBase : ViewModelBase, IIndicatorSeries
    {
        private IEnumerable<IndicatorValueBase> values;

        public IEnumerable<IndicatorValueBase> Values { get => values; set { if (values != value) { values = value; RaisePropertyChanged(); } } }

    }

    public class Curve : ViewModelBase
    {
        private string name;
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }

        private Brush brush;
        public Brush Brush { get => brush; set { if (brush != value) { brush = value; RaisePropertyChanged(); } } }

        private double thickness;
        public double Thickness { get => thickness; set { if (thickness != value) { thickness = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorLineSeries : IndicatorSeriesBase
    {
        private Curve curve;

        public IndicatorLineSeries()
        {
            this.curve = new Curve() { };
        }

        public Curve Curve { get => curve; set { if (curve != value) { curve = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorRangeSeries : IIndicatorSeries
    {
        public IndicatorRangeSeries()
        {
            this.Fill = new SolidColorBrush(Color.FromArgb(90, Colors.LightGreen.R, Colors.LightGreen.G, Colors.LightGreen.B));
            this.Stroke = Brushes.Green;
        }
        public string UpName { get; set; }
        public Brush UpBrush { get; set; }
        public double UpThickness { get; set; }

        public string DownName { get; set; }
        public Brush DownBrush { get; set; }
        public double DownThickness { get; set; }

        public Brush Fill { get; set; }
        public Brush Stroke { get; set; }

        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }

    public class IndicatorBandSeries : IndicatorRangeSeries
    {
        public IndicatorBandSeries()
        {
            this.MidThickness = 1;
            this.MidBrush = Brushes.Green;
        }
        public string MidName { get; set; }
        public Brush MidBrush { get; set; }
        public double MidThickness { get; set; }
    }

    public class IndicatorTrailValue : IndicatorValueBase
    {
        public double Long { get; set; }
        public double LongReentry { get; set; }
        public double Short { get; set; }
        public double ShortReentry { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
    }

    public class IndicatorTrailSeries : IIndicatorSeries
    {
        public IndicatorTrailSeries()
        {
            Color color = Colors.Green;
            this.LongStroke = new SolidColorBrush(color);
            color = Color.FromArgb(150, color.R, color.G, color.B);
            this.LongFill = new SolidColorBrush(color);

            color = Colors.Red;
            this.ShortStroke = new SolidColorBrush(color);
            color = Color.FromArgb(150, color.R, color.G, color.B);
            this.ShortFill = new SolidColorBrush(color);


            this.LongReentryThickness = 1;
            this.LongReentryStroke = Brushes.DarkRed;

            this.ShortReentryThickness = 1;
            this.ShortReentryStroke = Brushes.DarkGreen;
        }

        public Brush LongFill { get; set; }
        public Brush LongStroke { get; set; }

        public Brush ShortFill { get; set; }
        public Brush ShortStroke { get; set; }

        public Brush LongReentryStroke { get; set; }
        public double LongReentryThickness { get; set; }

        public Brush ShortReentryStroke { get; set; }
        public double ShortReentryThickness { get; set; }

        public IEnumerable<IndicatorValueBase> Values { get; set; }
    }
}
