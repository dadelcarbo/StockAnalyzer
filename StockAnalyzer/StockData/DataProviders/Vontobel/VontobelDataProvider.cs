using StockAnalyzer.StockClasses;
using System;
using System.Diagnostics;

namespace StockAnalyzer.StockData.DataProviders.Vontobel
{
    public class VontobelDataProvider : DataProviderBase
    {
        public override string DisplayName => "Vontobel";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.Vontobel;

        protected override void PreInitDictionary(bool download)
        {
            this.dataClient = new VontobelDataClient();
        }

        protected override void PostInitDictionary(bool download)
        {
        }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');

            return new StockInstrument()
            {
                Id = row[0],
                Name = row[1],
                Isin = row[0],
                Symbol = row[2],
                Group = Groups.TURBO,
                Provider = DataProvider.Vontobel,
                Market = Market.TURBO
            };
        }

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            Process.Start($"https://markets.vontobel.com/fr-fr/produits/leverage/leverage-short/{stockInstrument.Isin}");
        }

    }
}
