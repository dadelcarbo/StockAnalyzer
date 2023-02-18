using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_MA : MovingAverageBase
{
    public override void Initialize(StockSerie stockSerie)
    {
        var values = new IndicatorLineValue[stockSerie.Bars.Count];
        var closeValues = stockSerie.CloseValues;
        var maValues = closeValues.CalculateMA(Period);

        int i = 0;
        foreach (var bar in stockSerie.Bars)
        {
            values[i] = new IndicatorLineValue()
            {
                Date = bar.Date,
                Value = maValues[i]
            };
            i++;
        }

        this.Series.Values = values;
    }
}
