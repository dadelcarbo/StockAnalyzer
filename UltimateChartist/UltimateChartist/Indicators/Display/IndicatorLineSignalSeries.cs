﻿using System.Windows.Media;

namespace UltimateChartist.Indicators.Display;

public class IndicatorLineSignalValue : IndicatorValueBase
{
    public double Value { get; set; }
    public double Signal { get; set; }
}
public class IndicatorLineSignalSeries : IndicatorLineSeries
{
    private Curve signal;
    public IndicatorLineSignalSeries()
    {
        signal = new Curve()
        {
            Stroke = Brushes.DarkRed,
            Thickness = 1,
            Name = "Signal"
        };
    }

    public Curve Signal { get => signal; set { if (signal != value) { signal = value; RaisePropertyChanged(); } } }
}