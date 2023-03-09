using System.Windows.Media;

namespace UltimateChartist.Indicators.Display;

public class IndicatorRangeValue : IndicatorValueBase
{
    public decimal Low { get; set; }
    public decimal High { get; set; }
}
public class IndicatorRangeSeries : IndicatorSeriesBase
{
    public IndicatorRangeSeries()
    {
        Area = new Area()
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
