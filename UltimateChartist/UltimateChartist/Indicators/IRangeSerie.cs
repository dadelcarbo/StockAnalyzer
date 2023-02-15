using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
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
        public double Low { get; set; }

        public double High { get; set; }
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

        private Brush stroke;
        public Brush Stroke { get => stroke; set { if (stroke != value) { stroke = value; RaisePropertyChanged(); } } }

        private double thickness;
        public double Thickness { get => thickness; set { if (thickness != value) { thickness = value; RaisePropertyChanged(); } } }
    }
    public class Area : ViewModelBase
    {
        private string name;
        public string Name { get => name; set { if (name != value) { name = value; RaisePropertyChanged(); } } }

        private Brush stroke;
        public Brush Stroke { get => stroke; set { if (stroke != value) { stroke = value; RaisePropertyChanged(); } } }

        private double thickness;
        public double Thickness { get => thickness; set { if (thickness != value) { thickness = value; RaisePropertyChanged(); } } }

        private Brush fill;
        public Brush Fill { get => fill; set { if (fill != value) { fill = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorLineSeries : IndicatorSeriesBase
    {
        private Curve curve;
        public IndicatorLineSeries()
        {
            this.curve = new Curve()
            {
                Stroke = Brushes.Black,
                Thickness = 1,
                Name = string.Empty
            };
        }

        public Curve Curve { get => curve; set { if (curve != value) { curve = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorRangeSeries : IndicatorSeriesBase
    {
        public IndicatorRangeSeries()
        {
            this.Area = new Area()
            {
                Fill = new SolidColorBrush(Color.FromArgb(90, Colors.LightGray.R, Colors.LightGray.G, Colors.LightGray.B)),
                Stroke = Brushes.Black,
                Thickness = 1,
                Name = string.Empty
            };
        }

        Area area;
        public Area Area { get => area; set { if (area != value) { area = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorBandSeries : IndicatorRangeSeries
    {
        public IndicatorBandSeries()
        {
            this.MidLine = new Curve
            {
                Stroke = Brushes.Black,
                Thickness = 1,
                Name = string.Empty
            };
        }
        Curve midLine;
        public Curve MidLine { get => midLine; set { if (midLine != value) { midLine = value; RaisePropertyChanged(); } } }
    }

    public class IndicatorTrailValue : IndicatorValueBase
    {
        public double Long { get; set; }
        public double LongReentry { get; set; }
        public double Short { get; set; }
        public double ShortReentry { get; set; }
        public double High { get; set; } // Higher band for Long stop
        public double Low { get; set; } // // Lower band for Short stop
    }

    public class IndicatorTrailSeries : IndicatorSeriesBase
    {
        public IndicatorTrailSeries()
        {
            Color color = Colors.Green;
            this.Long = new Area
            {
                Name = "Long",
                Stroke = new SolidColorBrush(color),
                Thickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
            };

            color = Colors.Red;
            this.Short = new Area
            {
                Name = "Long",
                Stroke = new SolidColorBrush(color),
                Thickness = 1,
                Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
            };

            this.LongReentry = new Curve
            {
                Name = "LongReentry",
                Stroke = Brushes.DarkRed,
                Thickness = 1
            };

            this.ShortReentry = new Curve
            {
                Name = "ShortReentry",
                Stroke = Brushes.DarkGreen,
                Thickness = 1
            };
        }

        private Area @long;
        public Area Long { get => @long; set { if (@long != value) { @long = value; RaisePropertyChanged(); } } }

        private Area @short;
        public Area Short { get => @short; set { if (@short != value) { @short = value; RaisePropertyChanged(); } } }

        private Curve longReentry;
        public Curve LongReentry { get => longReentry; set { if (longReentry != value) { longReentry = value; RaisePropertyChanged(); } } }

        private Curve shortReentry;
        public Curve ShortReentry { get => shortReentry; set { if (shortReentry != value) { shortReentry = value; RaisePropertyChanged(); } } }
    }
}
