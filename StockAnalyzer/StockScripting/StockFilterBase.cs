using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData;

namespace StockAnalyzer.StockScripting
{
    public interface IStockFilter
    {
        bool MatchFilter(DataSerie dataSerie);

        bool MatchFilter(StockInstrument instrument, BarDuration duration);
    }
    public abstract class StockFilterBase : IStockFilter
    {
        public bool MatchFilter(DataSerie dataSerie)
        {
            if (dataSerie?.Values == null || dataSerie.Values.Length == 0)
                return false;

            StockDailyValue currentBar = dataSerie.LastValue;

            return MatchFilter(dataSerie, currentBar);
        }

        public bool MatchFilter(StockInstrument instrument, BarDuration duration)
        {
            var dataSerie = instrument.GetDataSerie(duration);

            return MatchFilter(dataSerie);
        }

        protected abstract bool MatchFilter(DataSerie dataSerie, StockDailyValue lastBar);
    }
}
