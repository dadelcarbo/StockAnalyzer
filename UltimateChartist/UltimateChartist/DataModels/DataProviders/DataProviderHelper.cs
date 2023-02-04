using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltimateChartist.DataModels.DataProviders
{
    public class DataProviderHelper
    {
        public static List<StockBar> LoadData(Instrument instrument, BarDuration duration)
        {
            IStockDataProvider dataProvider = StockDataProviderBase.GetDataProvider(instrument.DataProvider);
            if (dataProvider != null)
            {
                return dataProvider.LoadData(instrument, duration);
            }
            return null;
        }
    }
}
