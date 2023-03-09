using System.Windows.Media;

namespace UltimateChartist.Indicators.Display;

public class IndicatorBandValue : IndicatorRangeValue
{
    public decimal Mid { get; set; }
}
public class IndicatorBandSeries : IndicatorRangeSeries
{
    public IndicatorBandSeries()
    {
        MidLine = new Curve
        {
            Stroke = Brushes.Black,
            Thickness = 1,
            Name = string.Empty
        };
    }
    Curve midLine;
    public Curve MidLine { get => midLine; set { if (midLine != value) { midLine = value; RaisePropertyChanged(); } } }
}
