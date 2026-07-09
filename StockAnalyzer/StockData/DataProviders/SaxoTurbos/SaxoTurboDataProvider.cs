using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.StockData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataProvider : DataProviderBase
    {
        public override string DisplayName => "Saxo Turbos";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SaxoTurbo;

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');

            return new StockInstrument()
            {
                Id = row[1],
                Name = row[2],
                Isin = row[1],
                Symbol = string.Empty,
                Group = Groups.TURBO,
                Provider = DataProvider.SaxoTurbo
            };
        }
    }
}
