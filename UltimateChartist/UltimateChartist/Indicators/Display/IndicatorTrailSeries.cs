using System.Windows.Media;

namespace UltimateChartist.Indicators.Display;

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
        Long = new Area
        {
            Name = "Long",
            Stroke = new SolidColorBrush(color),
            Thickness = 1,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        color = Colors.Red;
        Short = new Area
        {
            Name = "Short",
            Stroke = new SolidColorBrush(color),
            Thickness = 1,
            Fill = new SolidColorBrush(Color.FromArgb(150, color.R, color.G, color.B))
        };

        LongReentry = new Curve
        {
            Name = "Long Reentry",
            Stroke = Brushes.DarkRed,
            Thickness = 1
        };

        ShortReentry = new Curve
        {
            Name = "Short Reentry",
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
