using System.Collections.Generic;
using System.Diagnostics;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockData;

namespace StockAnalyzerApp.StockData
{
    [DebuggerDisplay("{Id}")]
    public class StockInstrument
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Isin { get; set; }
        public string Symbol { get; set; }
        public string Ticker { get; set; }
        public Groups Group { get; set; }
        public StockDataProvider DataProvider { get; set; }
        public long SaxoId { get; set; }
        public StockAnalysis StockAnalysis { get; set; }

        public StockSerie StockSerie { get; set; }

        public StockInstrument(StockSerie serie)
        {
            this.StockSerie = serie;

            this.Id = serie.StockName;
            this.DisplayName = serie.StockName;
            this.Isin = serie.ISIN;
            this.Ticker = serie.Symbol;
            this.DataProvider = serie.DataProvider;
            this.Symbol = serie.Symbol;
            this.SaxoId = serie.SaxoId;

            this.Group = serie.StockGroup;
            this.StockAnalysis = serie.StockAnalysis;
        }

        private SortedDictionary<BarDuration, DataSerie> cache = new SortedDictionary<BarDuration, DataSerie>();

        public void ClearCache()
        {
            cache.Clear();
        }
        /// <summary>
        /// Return data from cache dictionnary
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        public DataSerie GetDataSerie(BarDuration duration)
        {
            if (!cache.ContainsKey(duration))
            {
                StockSerie.BarDuration = duration;
                if (StockSerie.Count > 0)
                {
                    cache.Add(duration, new DataSerie(this, duration, StockSerie.ValueArray));
                }
                else
                    return null;
            }
            return cache[duration];
        }


        public bool BelongsToGroup(Groups group)
        {
            return this.StockSerie.BelongsToGroup(group);
        }
    }
}
