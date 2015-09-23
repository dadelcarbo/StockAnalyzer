using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace StockAnalyzer.StockClasses
{
    class StockStatistics
    {
        public const int NB_VALUES = 10;

        public float[] variationClose = new float[NB_VALUES];
        public float[] variationLow = new float[NB_VALUES];
        public float[] variationHigh = new float[NB_VALUES];

        public float avgVariationClose = 0.0f;
        public float avgVariationLow = 0.0f;
        public float avgVariationHigh = 0.0f;

        public float minVariationClose = float.MaxValue;
        public float minVariationLow = float.MaxValue;
        public float minVariationHigh = float.MaxValue;

        public float maxVariationClose = float.MinValue;
        public float maxVariationLow = float.MinValue;
        public float maxVariationHigh = float.MinValue;

        public int nbVariationValue = 0;

        public void AddStock(int index, StockSerie stockSerie)
        {
            StockDailyValue dailyValue = stockSerie.ValueArray[index];
            StockDailyValue futureValue = null;
            float dailyVariationClose = 0.0f;
            float dailyVariationLow = 0.0f;
            float dailyVariationHigh = 0.0f;

            for (int i = 0; i < NB_VALUES; i++)
            {
                futureValue = stockSerie.Values.ElementAt(index + i + 1);

                dailyVariationClose = (futureValue.CLOSE - dailyValue.CLOSE) / dailyValue.CLOSE;
                dailyVariationLow = (futureValue.LOW - dailyValue.CLOSE) / dailyValue.CLOSE;
                dailyVariationHigh = (futureValue.HIGH - dailyValue.CLOSE) / dailyValue.CLOSE;

                variationClose[i] += dailyVariationClose;
                variationLow[i] += dailyVariationLow;
                variationHigh[i] += dailyVariationHigh;

                minVariationClose = Math.Min(minVariationClose, dailyVariationClose);
                minVariationLow = Math.Min(minVariationLow, dailyVariationLow);
                minVariationHigh = Math.Min(minVariationHigh, dailyVariationHigh);

                maxVariationClose = Math.Max(minVariationClose, dailyVariationClose);
                maxVariationLow = Math.Max(minVariationLow, dailyVariationLow);
                maxVariationHigh = Math.Max(minVariationHigh, dailyVariationHigh);
            }
            nbVariationValue++;
        }
        public void GenerateAverage()
        {
            // AVG
            float sumClose = 0.0f;
            float sumLow = 0.0f;
            float sumHigh = 0.0f;
            for (int i = 0; i < NB_VALUES; i++)
            {
                sumClose += variationClose[i];
                sumLow += variationLow[i];
                sumHigh += variationHigh[i];
            }
            avgVariationClose = (sumClose / (float)nbVariationValue / (float)NB_VALUES);
            avgVariationLow = (sumLow / (float)nbVariationValue / (float)NB_VALUES);
            avgVariationHigh = (sumHigh / (float)nbVariationValue / (float)NB_VALUES);
        }
        public override string ToString()
        {
            string value = string.Empty;

            if (nbVariationValue == 0)
            {
                value = "Empty";
            }
            else
            {
                value += nbVariationValue + ",";
                int i;

                value += avgVariationClose.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += avgVariationLow.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += avgVariationHigh.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";

                value += minVariationClose.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += minVariationLow.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += minVariationHigh.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";

                value += maxVariationClose.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += maxVariationLow.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                value += maxVariationHigh.ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";

                for (i = 0; i < NB_VALUES; i++)
                {
                    value += (variationClose[i] / (float)nbVariationValue).ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                }
                for (i = 0; i < NB_VALUES; i++)
                {
                    value += (variationLow[i] / (float)nbVariationValue).ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                }
                for (i = 0; i < NB_VALUES; i++)
                {
                    value += (variationHigh[i] / (float)nbVariationValue).ToString(StockAnalyzerApp.Global.EnglishCulture) + ",";
                }
            }
            return value;
        }

        static public SortedDictionary<string, StockStatistics> FromFile(string fileName)
        {
            SortedDictionary<string, StockStatistics> stats = null;
            string line;
            string []fields;
            StockStatistics statistic;
            try
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    stats = new SortedDictionary<string, StockStatistics>();
                    // Skip headers
                    line = sr.ReadLine();

                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        fields = line.Split(',');
                        statistic = new StockStatistics();

                        statistic.minVariationClose = float.Parse(fields[5], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.minVariationLow = float.Parse(fields[6], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.minVariationHigh = float.Parse(fields[7], StockAnalyzerApp.Global.EnglishCulture);

                        statistic.maxVariationClose = float.Parse(fields[8], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.maxVariationLow = float.Parse(fields[9], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.maxVariationHigh = float.Parse(fields[10], StockAnalyzerApp.Global.EnglishCulture);

                        statistic.avgVariationClose = float.Parse(fields[2], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.avgVariationLow = float.Parse(fields[3], StockAnalyzerApp.Global.EnglishCulture);
                        statistic.avgVariationHigh = float.Parse(fields[4], StockAnalyzerApp.Global.EnglishCulture);
                        
                        statistic.nbVariationValue = int.Parse(fields[1]);

                        stats.Add(fields[0], statistic);
                    }
                }
            }
            catch (System.Exception)
            {
                stats = null;
            }

            return stats;
        }
    }
}
