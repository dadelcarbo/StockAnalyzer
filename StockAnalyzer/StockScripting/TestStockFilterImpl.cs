using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockScripting
{
    public class TestStockFilterImpl : StockFilterBase
    {
        protected override bool MatchFilter(DataSerie dataSerie, StockDailyValue lastBar)
        {
            var ema175 = dataSerie.GetIndicator("EMA(175)").Series[0][dataSerie.LastIndex];
            if (lastBar.CLOSE < ema175)
                return false;

            var c1 = lastBar.VARIATION > 0.04f;
            var c2 = false;
            var c3 = false;

            if (c1)
            {
                var closeSerie = dataSerie.GetSerie(StockDataType.CLOSE);
                var highest = closeSerie.GetHighestIn(dataSerie.LastIndex);
                c2 = highest > 20;
                if (c2)
                {
                    c3 = closeSerie.GetHighestIn(dataSerie.LastIndex - 1) > (highest + 1);
                }
            }

            return c1 && c2 && c3;
        }
    }
}