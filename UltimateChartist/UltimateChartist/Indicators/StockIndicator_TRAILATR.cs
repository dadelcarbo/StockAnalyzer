using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_TrailATR : IndicatorBase
{
    public StockIndicator_TrailATR()
    {
        this.Series = new IndicatorTrailSeries();
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
        var values = new IndicatorTrailValue[stockSerie.Bars.Count];
        var atrSerie = stockSerie.Bars.CalculateATR().CalculateEMA(AtrPeriod).Mult(upWidth);
        var emaSerie = stockSerie.CloseValues.CalculateEMA(Period);
        var lowerBand = emaSerie.Sub(atrSerie.Mult(downWidth));
        var upperBand = emaSerie.Add(atrSerie.Mult(upWidth));

        stockSerie.Bars.CalculateBandTrailStop(lowerBand, upperBand, out decimal?[] longStop, out decimal?[] shortStop);

        int i = 0;
        foreach (var bar in stockSerie.Bars)
        {
            values[i] = new IndicatorTrailValue()
            {
                Date = bar.Date,
                High = longStop[i] == null ? null: bar.Close,
                Low = shortStop[i] == null ? null : bar.Close,
                Long = longStop[i],
                Short = shortStop[i],
                LongReentry = null,
                ShortReentry = null
            };
            i++;
        }

        this.Series.Values = values;
    }
}
