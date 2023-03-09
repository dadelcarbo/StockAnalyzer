using System.Windows.Media;

namespace UltimateChartist.Indicators.Display;

public class IndicatorLineValue : IndicatorValueBase
{
    public decimal Value { get; set; }
}
public class IndicatorLineSeries : IndicatorSeriesBase
{
    private Curve curve;
    public IndicatorLineSeries()
    {
        curve = new Curve()
        {
            Stroke = Brushes.Black,
            Thickness = 1,
            Name = string.Empty
        };
    }

    public Curve Curve { get => curve; set { if (curve != value) { curve = value; RaisePropertyChanged(); } } }
}
