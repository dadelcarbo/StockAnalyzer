using StockAnalyzer.StockClasses;
using System.Diagnostics;

namespace StockAnalyzer.StockData.DataProviders.SocGen
{
    public class SocGenDataProvider : DataProviderBase
    {
        public override string DisplayName => "Soc Gen";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SocGen;

        protected override void PreInitDictionary(bool download)
        {
            this.dataClient = new SocGenDataClient();
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
                Name = row[3],
                Isin = row[0],
                Symbol = row[1],
                Ticker = long.Parse(row[2]),
                Group = Groups.TURBO,
                Provider = DataProvider.SocGen,
                Market = Market.TURBO
            };
        }

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            var url = $"https://bourse.societegenerale.fr/product-details/{stockInstrument.Symbol.ToLower()}";
            Process.Start(url);
        }
    }
}
