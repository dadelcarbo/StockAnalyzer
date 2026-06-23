using System.Collections.Generic;

using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.StockData
{
    public class StockInstrument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Isin { get; set; }
        public string Ticker { get; set; }
        public Groups Group { get; set; }

        private StockSerie serie;

        private StockInstrument(StockSerie serie)
        {
            this.serie = serie;

            this.Id = serie.StockName;
            this.DisplayName = serie.StockName;
            this.Isin = serie.ISIN;
            this.Ticker = serie.Symbol;

            this.Group = serie.StockGroup;
        }

        private SortedDictionary<BarDuration, StockDailyValue[]> cache = new SortedDictionary<BarDuration, StockDailyValue[]>();
        /// <summary>
        /// Return data from cache dictionnary
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public StockDailyValue[] GetValues(BarDuration duration)
        {
            if (!cache.ContainsKey(duration))
            {
                serie.BarDuration = duration;
                if (serie.BarDuration > 0)
                {
                    cache.Add(duration, serie.ValueArray);
                }
                else
                    return null;
            }
            return cache[duration];
        }

        static public SortedDictionary<string, StockInstrument> Instruments { get; private set; } = new SortedDictionary<string, StockInstrument>();

        static public void Initialize(IEnumerable<StockSerie> stockSeries)
        {
            Instruments.Clear();
            foreach (var serie in stockSeries)
            {
                var instrument = new StockInstrument(serie);
                Instruments[instrument.Id] = instrument;
            }
        }
    }
}
