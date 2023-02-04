using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateChartist.DataModels.DataProviders;

namespace UltimateChartist.DataModels
{
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

        public StockGroup Group { get; set; }

        public string SearchText => $"{Name} {Ticker} {ISIN}";

        public string Symbol { get; internal set; }
        public StockDataProvider DataProvider { get; internal set; }
    }
}
