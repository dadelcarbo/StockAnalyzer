using System.Linq;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_EMA : MovingAverageBase
{
    public override void Initialize(StockSerie stockSerie)
    {
        var values = new IndicatorLineValue[stockSerie.Bars.Count];

        decimal alpha = 2.0m / (Period + 1.0m);
        var firstBar = stockSerie.Bars.First();
        values[0] = new IndicatorLineValue() { Date = firstBar.Date, Value = firstBar.Close };
        decimal ema = firstBar.Close;

        int i = 1;
        foreach (var bar in stockSerie.Bars.Skip(1))
        {
            ema += alpha * (bar.Close - ema);
            values[i++] = new IndicatorLineValue() { Date = bar.Date, Value = ema };
        }

        this.Series.Values = values;
    }
}
