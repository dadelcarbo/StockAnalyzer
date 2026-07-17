using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace StockAnalyzer.StockData.DataProviders.Cnn
{
    public class CnnDataProvider : DataProviderBase
    {
        public override string DisplayName => "CNN";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
        public override BarDuration DefaultDuration => BarDuration.Daily;
        public override DataProvider Provider => DataProvider.Cnn;

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            Process.Start("https://edition.cnn.com/markets/fear-and-greed");
        }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var fields = line.Split(',');
            var longName = fields[0];
            return new StockInstrument
            {
                Name = longName,
                Id = longName,
                Group = (Groups)Enum.Parse(typeof(Groups), fields[1]),
                Provider = DataProvider.Cnn,
                Market = (Market)Enum.Parse(typeof(Market), fields[2])
            };
        }

        protected override void PostInitDictionary(bool download)
        {
        }

        protected override void PreInitDictionary(bool download)
        {
            this.dataClient = new CnnDataClient();
        }

    }
}
