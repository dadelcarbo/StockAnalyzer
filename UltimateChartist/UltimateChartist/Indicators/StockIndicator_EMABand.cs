using System;
using System.Linq;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_EMABand : IndicatorBase
{
    public StockIndicator_EMABand()
    {
        this.Series = new IndicatorRangeSeries();
    }

    public override DisplayType DisplayType => DisplayType.Price;

    public override string DisplayName => $"{ShortName}({Period},{Percent})";


    private int period = 20;
    [IndicatorParameterInt("Period", 1, 500)]
    public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }

    private decimal percent = 0.05m;
    [IndicatorParameterDecimal("Percent", 0, 1, 0.001, "P2")]
    public decimal Percent { get => percent; set { if (percent != value) { percent = value; RaiseParameterChanged(); } } }

    public override void Initialize(StockSerie stockSerie)
    {
        var values = new IndicatorRangeValue[stockSerie.Bars.Count];

        var alpha = 2.0m / (Period + 1.0m);
        var firstBar = stockSerie.Bars.First();
        values[0] = new IndicatorRangeValue() { Date = firstBar.Date, High = firstBar.Close, Low = firstBar.Close };
        var ema = firstBar.Close;

        int i = 1;
        var upRatio = 1 + Percent;
        var downRatio = 1 - Percent;
        foreach (var bar in stockSerie.Bars.Skip(1))
        {
            ema += alpha * (bar.Close - ema);
            var high = ema * upRatio;
            Max = Math.Max(Max, high);
            values[i++] = new IndicatorRangeValue() { Date = bar.Date, High = high, Low = ema * downRatio };
        }

        this.Series.Values = values;
    }
}
