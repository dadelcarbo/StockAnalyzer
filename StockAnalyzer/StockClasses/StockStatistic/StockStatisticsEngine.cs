using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockStatistic.MatchPatterns;

namespace StockAnalyzer.StockClasses.StockStatistic
{
    public class StockStatisticsEngine
    {
        private int sampleBefore = 0;
        private int sampleAfter = 0;
        private int sampleSize = 0;

        public StockStatisticsEngine(int before, int after)
        {
            this.Patterns = new List<StockPatternIndex>();
            sampleBefore = before;
            sampleAfter = after;
            sampleSize = before + after;
        }
        private List<StockPatternIndex> Patterns { get; set; }


        public StockSerie FindPattern(IEnumerable<StockSerie> stockSeries, StockSerie.StockBarDuration duration, IStockMatchPattern patternMatch)
        {
            foreach (var stockSerie in stockSeries)
            {
                if (!stockSerie.Initialise()) continue;
                StockSerie.StockBarDuration previousDuration = stockSerie.BarDuration;

                stockSerie.BarDuration = duration;

                for (int i = sampleBefore; i < stockSerie.Count - sampleAfter - 1; i++)
                {
                    if (patternMatch.MatchPattern(stockSerie, i))
                    {
                        this.Patterns.Add(new StockPatternIndex()
                        {
                            Serie = stockSerie,
                            Index = i,
                            Duration = duration
                        });
                    }
                }

                stockSerie.BarDuration = previousDuration;
            }

            return this.GetTypicalSerie();
        }

        private StockSerie GetTypicalSerie()
        {
            float[] close = new float[sampleSize];
            long[] nbSample = new long[sampleSize];

            float initialValue = 100.0f;

            // Calculate average daily returns
            StockSerie typicalSerie = new StockSerie("Typical", "Typical", StockSerie.Groups.NONE, StockDataProvider.Generated);

            foreach (var pattern in Patterns)
            {
                float ratio = 100.0f / pattern.Serie.GetValue(StockDataType.CLOSE, pattern.Index + 1);

                for (int i = 0; i < sampleSize; i++)
                {
                    if (pattern.Index + i > 0 && pattern.Index + i < pattern.Serie.Count)
                    {
                        close[i] += pattern.Serie.GetValue(StockDataType.CLOSE, pattern.Index + i) * ratio;
                        nbSample[i]++;
                    }
                }
            }


            DateTime date = DateTime.Today;
            for (int i = 0; i < sampleSize; i++)
            {
                float closeVal = nbSample[i] != 0 ? close[i] /= nbSample[i] : 0.0f;

                if (closeVal != 0.0f)
                {
                    typicalSerie.Add(date,
                        new StockDailyValue(typicalSerie.StockName, closeVal, closeVal, closeVal,
                            closeVal, nbSample[i], date));

                    date = date.AddDays(1);
                }
            }
            return typicalSerie;
        }

        private void Clear()
        {
            this.Patterns.Clear();
        }
    }
}
