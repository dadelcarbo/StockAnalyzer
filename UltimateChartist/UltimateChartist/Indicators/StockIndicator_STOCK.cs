using UltimateChartist.DataModels;
using UltimateChartist.Indicators.Display;

namespace UltimateChartist.Indicators;

public class StockIndicator_STOCK : IndicatorBase, IRangedIndicator
{
    public StockIndicator_STOCK()
    {
        this.Series = new IndicatorLineSignalSeries();
    }
    public override DisplayType DisplayType => DisplayType.Ranged;
    public decimal Minimum => 0;

    public decimal Maximum => 100;

    public override string DisplayName => $"{ShortName}({Period},{SignalPeriod})";


    private int period = 14;
    [IndicatorParameterInt("Fast %K", 1, 500)]
    public int Period { get => period; set { if (period != value) { period = value; RaiseParameterChanged(); } } }

    private int signalPeriod = 3;
    [IndicatorParameterInt("Slow %D", 1, 500)]
    public int SignalPeriod { get => signalPeriod; set { if (signalPeriod != value) { signalPeriod = value; RaiseParameterChanged(); } } }

    private bool useBody = false;
    [IndicatorParameterBool("Use Body")]
    public bool UseBody { get => useBody; set { if (useBody != value) { useBody = value; RaiseParameterChanged(); } } }

    public override void Initialize(StockSerie stockSerie)
    {
        var values = new IndicatorLineSignalValue[stockSerie.Bars.Count];
        var stockf = stockSerie.CalculateFastOscillator(this.period, false);

        var signalAlpha = 2.0m / (SignalPeriod + 1.0m);

        int i = 0;
        var signal = stockf[0];
        foreach (var bar in stockSerie.Bars)
        {
            signal += signalAlpha * (stockf[i] - signal);
            values[i] = new IndicatorLineSignalValue() { Date = bar.Date, Value = stockf[i], Signal = signal };
            i++;
        }

        this.Series.Values = values;
    }
}
