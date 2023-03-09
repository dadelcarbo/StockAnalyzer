using System;
using System.Collections.Generic;
using System.Linq;
using UltimateChartist.DataModels.DataProviders;

namespace UltimateChartist.DataModels;

public class StockSerie
{
    public StockSerie(Instrument instrument, BarDuration barDuration)
    {
        Instrument = instrument;
        BarDuration = barDuration;
    }
    public Instrument Instrument { get; }
    public BarDuration BarDuration { get; } = BarDuration.Daily;

    public List<StockBar> Bars { get; set; }

    public void LoadData(IStockDataProvider dataProvider, Instrument instrument, BarDuration barDuration)
    {
        if (barDuration == BarDuration.Weekly)
        {
            this.Bars = StockBar.GenerateWeeklyBarsFomDaily(instrument.GetStockSerie(BarDuration.Daily).Bars);
            return;
        }
        else if (barDuration == BarDuration.Monthly)
        {
            this.Bars = StockBar.GenerateMonthlyBarsFomDaily(instrument.GetStockSerie(BarDuration.Daily).Bars);
            return;
        }
        this.Bars = dataProvider.LoadData(instrument, barDuration);
    }

    #region Bar value series
    private DateTime[] dateValues;
    public DateTime[] DateValues => dateValues ??= this.Bars.Select(b => b.Date).ToArray();

    private decimal[] closeValues;
    public decimal[] CloseValues => closeValues ??= this.Bars.Select(b => b.Close).ToArray();

    private decimal[] highValues;
    public decimal[] HighValues => highValues ??= this.Bars.Select(b => b.High).ToArray();

    private decimal[] bodyHighValues;
    public decimal[] BodyHighValues => bodyHighValues ??= this.Bars.Select(b => b.BodyHigh).ToArray();

    private decimal[] lowValues;
    public decimal[] LowValues => lowValues ??= this.Bars.Select(b => b.Low).ToArray();

    private decimal[] bodyLowValues;
    public decimal[] BodyLowValues => bodyLowValues ??= this.Bars.Select(b => b.BodyLow).ToArray();

    private decimal[] openValues;
    public decimal[] OpenValues => openValues ??= this.Bars.Select(b => b.Open).ToArray();

    private long[] volumeValues;
    public long[] VolumeValues => volumeValues ??= this.Bars.Select(b => b.Volume).ToArray();
    #endregion
}
