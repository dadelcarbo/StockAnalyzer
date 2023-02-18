using System.Windows.Media;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public enum EmaType
{
    EMA,
    MA,
    MID
}

public abstract class MovingAverageBase : IndicatorBase
{
    public MovingAverageBase()
    {
        var series = new IndicatorLineSeries();
        series.Curve.Stroke = Brushes.Blue;
        series.Curve.Thickness = 1;
        series.Curve.Name= "Moving Average";

        this.Series = series;
    }

    public override DisplayType DisplayType => DisplayType.Price;
    public override string DisplayName => $"{ShortName}({Period})";

    private int period = 20;
    [IndicatorParameterInt("Period", 1, 500)]
    public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }
}