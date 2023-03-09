using MediaFoundation;
using System;
using System.Linq;
using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_ATRBand : IndicatorBase
{
    public StockIndicator_ATRBand()
    {
        this.Series = new IndicatorBandSeries();
    }

    public override DisplayType DisplayType => DisplayType.Price;

    public override string DisplayName => $"{ShortName}({Period},{AtrPeriod},{UpWidth},{downWidth})";

    private int period = 20;
    [IndicatorParameterInt("Period", 1, 500)]
    public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }

    private int atrPeriod = 20;
    [IndicatorParameterInt("ATR Period", 1, 500)]
    public int AtrPeriod { get => atrPeriod; set { if (atrPeriod != value) { atrPeriod = value; RaiseParameterChanged(); } } }

    private decimal upWidth = 1;
    [IndicatorParameterDecimal("Up Width", 0, 50, 0.1, "{0:F2}")]
    public decimal UpWidth { get => upWidth; set { if (upWidth != value) { upWidth = value; RaiseParameterChanged(); } } }

    private decimal downWidth = 1;
    [IndicatorParameterDecimal("Down Width", 0, 50, 0.1, "{0:F2}")]
    public decimal DownWidth { get => downWidth; set { if (downWidth != value) { downWidth = value; RaiseParameterChanged(); } } }

    public override void Initialize(StockSerie stockSerie)
    {
        var values = new IndicatorBandValue[stockSerie.Bars.Count];
        var atrSerie = stockSerie.Bars.CalculateATR().CalculateEMA(AtrPeriod);

        decimal alpha = 2.0m / (Period + 1.0m);
        var firstBar = stockSerie.Bars.First();
        values[0] = new IndicatorBandValue() { Date = firstBar.Date, High = firstBar.Close + upWidth * atrSerie[0], Mid = firstBar.Close, Low = firstBar.Close - downWidth * atrSerie[0] };
        decimal ema = firstBar.Close;

        int i = 1;
        foreach (var bar in stockSerie.Bars.Skip(1))
        {
            ema += alpha * (bar.Close - ema);
            var high = ema + upWidth * atrSerie[i];
            Max = Math.Max(Max, high);
            values[i] = new IndicatorBandValue() { Date = bar.Date, High = high, Mid = ema, Low = ema - downWidth * atrSerie[i] };
            i++;
        }

        this.Series.Values = values;
    }
}
