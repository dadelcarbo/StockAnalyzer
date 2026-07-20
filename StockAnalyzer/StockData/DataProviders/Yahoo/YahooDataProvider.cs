using StockAnalyzer.StockClasses;
using System;
using System.Diagnostics;

namespace StockAnalyzer.StockData.DataProviders.Yahoo
{
    public class YahooDataProvider : DataProviderBase
    {
        public override string DisplayName => "Yahoo";

        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
        public override BarDuration DefaultDuration => BarDuration.Daily;
        public override DataProvider Provider => DataProvider.Yahoo;

        protected override void PreInitDictionary(bool download)
        {
            dataClient = new YahooDataClient();
        }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');
            var stockName = row[1];
            return new StockInstrument()
            {
                Id = row.Length > 3 ? row[3] : row[0],
                Name = row[1],
                Symbol = row[0],
                Group = (Groups)Enum.Parse(typeof(Groups), row[2]),
                Provider = DataProvider.Yahoo
            };
        }

        protected override void PostInitDictionary(bool download) { }
        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            Process.Start($"https://finance.yahoo.com/quote/{stockInstrument.StockSerie.Symbol}");
        }

    }
}
