using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockData.DataProviders
{
    public enum Market
    {
        EURONEXT,
        XETRA,
        NYSE,
        MIXED,
        TURBO,
        FUTURE
    }
    public class MarketHours
    {
        public TimeSpan Open { get; set; }
        public TimeSpan Close { get; set; }

        /// <summary>
        /// Specifyies what time download is available. For US time is 24h + 6h (as data available at 6:00 am the next day)
        /// </summary>
        public TimeSpan DownloadAvailability { get; set; }

        public bool IsOpened => DateTime.Now.TimeOfDay >= Open && DateTime.Now.TimeOfDay <= Close;

        static SortedDictionary<Market, MarketHours> marketHoursTable => new SortedDictionary<Market, MarketHours>() {
            { Market.EURONEXT , new MarketHours() { Open = new TimeSpan(9,0,0), Close = new TimeSpan(17,35,00), DownloadAvailability = new TimeSpan(18,0,0)} },
            { Market.XETRA , new MarketHours() { Open = new TimeSpan(9,0,0), Close = new TimeSpan(17,35,00), DownloadAvailability = new TimeSpan(21,0,0)} },
            { Market.NYSE,  new MarketHours() { Open = new TimeSpan(15,30,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(30,0,0)} },
            { Market.TURBO ,  new MarketHours() { Open = new TimeSpan(8,0,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(22,0,0)} },
            { Market.MIXED ,  new MarketHours() { Open = new TimeSpan(8,0,0), Close = new TimeSpan(22,00,00), DownloadAvailability = new TimeSpan(21,0,0)} },
            { Market.FUTURE ,  new MarketHours() { Open = new TimeSpan(2,0,0), Close = new TimeSpan(23,00,00), DownloadAvailability = new TimeSpan(30,0,0)} }
        };

        static public SortedDictionary<Market, MarketHours> MarketHoursTable => marketHoursTable;
    }
}
