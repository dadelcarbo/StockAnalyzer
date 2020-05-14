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


        public StockSerie FindPattern(IEnumerable<StockSerie> stockSeries, StockBarDuration duration, IStockMatchPattern patternMatch)
        {
            foreach (var stockSerie in stockSeries)
            {
                if (!stockSerie.Initialise()) continue;
                StockBarDuration previousDuration = stockSerie.BarDuration;

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

            return this.GetTypicalSerie(patternMatch);
        }

        private StockSerie GetTypicalSerie(IStockMatchPattern patternMatch)
        {
            double[] close = new double[sampleSize];
            long[] nbSample = new long[sampleSize];

            double initialValue = 100.0;

            // Calculate average daily returns
            StockSerie typicalSerie = new StockSerie("Pattern_" + patternMatch.Suffix, "Pattern_" + patternMatch.Suffix, StockSerie.Groups.NONE, StockDataProvider.Generated);

            foreach (var pattern in Patterns)
            {
                double ratio = initialValue / pattern.Serie.GetValue(StockDataType.CLOSE, pattern.Index-sampleBefore);

                for (int i = 0; i < sampleSize; i++)
                {
                    int index = pattern.Index - sampleBefore + i;
                    if (index >= 0 && index < pattern.Serie.Count)
                    {
                        double value = pattern.Serie.GetValue(StockDataType.CLOSE, index);
                        double newValue = value * ratio;
                        close[i] += newValue;
                        nbSample[i]++;
                    }
                }
            }

            DateTime date = DateTime.Today;
            for (int i = 0; i < sampleSize; i++)
            {
                close[i] = nbSample[i] != 0 ? close[i] / nbSample[i] : 0.0f;
            }

            double coef = initialValue / close[sampleBefore];
            for (int i = 0; i < sampleSize; i++)
            {
                float closeVal = (float)(close[i]*coef);

                if (closeVal != 0.0f)
                {
                    typicalSerie.Add(date, new StockDailyValue(typicalSerie.StockName, closeVal, closeVal, closeVal, closeVal, nbSample[i], date));
                    date = date.AddDays(1);
                }
            }
            return typicalSerie;
        }
        public StockSerie GenerateSerie(string name)
        {
            double[] close = new double[sampleSize];

            float initialValue = 100.0f;
            int size = 100000;

            // Calculate average daily returns
            StockSerie newSerie = new StockSerie(name, name, StockSerie.Groups.NONE, StockDataProvider.Generated);

            DateTime date = DateTime.Today;
            double closeVal = initialValue;

            Random rand = new Random();
            for (int i = 0; i < size; i++)
            {
                //closeVal = i % 2 == 0 ? closeVal * (1.0 + 5.0/95.0) : closeVal * 0.95;
                closeVal += (float)Math.Cos(0.08 * Math.PI * i)*10;
                closeVal += (float)Math.Sin(0.04 * Math.PI * i)*5;
                //closeVal *= 0.9999;
                //double percentChange = (rand.NextDouble() - 0.5) * 0.10;
                //if (percentChange > 0)
                //{
                //    closeVal /= 1.0f - percentChange;
                //}
                //else
                //{
                //    closeVal *= 1.0f + percentChange;
                //}
                if (closeVal != 0.0f)
                {
                    float closeFloat = (float)closeVal;
                    newSerie.Add(date, new StockDailyValue(newSerie.StockName, closeFloat, closeFloat, closeFloat, closeFloat, i, date));

                    date = date.AddDays(1);
                }
            }
            return newSerie;
        }

        private void Clear()
        {
            this.Patterns.Clear();
        }
    }
}
