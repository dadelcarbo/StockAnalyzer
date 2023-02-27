using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using UltimateChartist.DataModels.DataProviders;

namespace UltimateChartist.DataModels;

public enum StockGroup
{
    NONE = 0,
    COUNTRY,
    PEA,
    CAC40,
    SBF120,
    CACALL,
    EURO_A,
    EURO_A_B,
    EURO_B,
    EURO_A_B_C,
    EURO_C,
    ALTERNEXT,
    BELGIUM,
    HOLLAND,
    PORTUGAL,
    EUROPE,
    //ITALIA,
    //SPAIN,
    USA,
    SAXO,
    TURBO,
    INDICES,
    INDICATOR,
    SECTORS,
    SECTORS_CAC,
    SECTORS_CALC,
    CURRENCY,
    COMMODITY,
    FOREX,
    FUND,
    RATIO,
    BREADTH,
    PTF,
    BOND,
    INTRADAY,
    Portfolio,
    Replay,
    ALL
}

public class Instrument
{
    public string Name { get; set; }
    public string ISIN { get; set; }
    public string Ticker { get; set; }
    public string Exchange { get; set; }
    public long Uic { get; set; }
    public string Symbol { get; set; }

    public string Country { get; set; }

    public StockGroup Group { get; set; }

    public string SearchText => $"{Name} {Ticker} {ISIN}";

    SortedDictionary<BarDuration, StockSerie> Series { get; set; } = new SortedDictionary<BarDuration, StockSerie>();

    public IStockDataProvider DataProvider { get; set; }
    public IStockDataProvider RealTimeDataProvider { get; set; }

    public BarDuration[] SupportedBarDurations
    {
        get
        {
            var barDurations = DataProvider.BarDurations;
            if (RealTimeDataProvider != null)
            {
                barDurations = barDurations.Union(RealTimeDataProvider.BarDurations).ToArray();
            }
            return barDurations;
        }
    }

    public StockSerie GetStockSerie(BarDuration barDuration)
    {
        if (Series.ContainsKey(barDuration))
            return Series[barDuration];

        StockSerie stockSerie = null;
        if (DataProvider.BarDurations.Contains(barDuration))
        {
            stockSerie = new StockSerie(this, barDuration);
            stockSerie.LoadData(DataProvider, this, barDuration);
            Series.Add(barDuration, stockSerie);
            return Series[barDuration];
        }

        if (RealTimeDataProvider != null && RealTimeDataProvider.BarDurations.Contains(barDuration))
        {
            stockSerie = new StockSerie(this, barDuration);
            stockSerie.LoadData(RealTimeDataProvider, this, barDuration);
            Series.Add(barDuration, stockSerie);
            return Series[barDuration];
        }

        return null;
    }
}
