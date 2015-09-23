using System;
using StockAnalyzer.StockLogging;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class AAIIDataProvider : StockDataProviderBase
    {
        // IStockDataProvider Implemetation
        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            bool res = false;
            string fileName = rootFolder + BULLBEARRATIO_FILENAME;
            res = this.ParseBullBearRatio(stockSerie, fileName);
            return res;
        }
        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }
        public override bool DownloadDailyData(string rootFolder, StockSerie serie)
        {
            throw new NotImplementedException();
        }
        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            if (!stockDictionary.ContainsKey(bearBullRatioName))
            {
                stockDictionary.Add(bearBullRatioName, new StockSerie(bearBullRatioName, bearBullRatioName, StockSerie.Groups.INDICATOR, StockDataProvider.AAII));
            }
        }

        static private string BULLBEARRATIO_FILENAME = @"\data\weekly\AAII\BullBearRatio.csv";
        static private string bearBullRatioName = "BEAR/BULL Ratio";

        private bool ParseBullBearRatio(StockSerie stockSerie, string fileName)
        {
            // Read new downloaded values

            StockDailyValue bearBullRatioValue = null;
            DateTime date;
            float bearish = 0;
            float bullish = 0;
            float neutral = 0;
            float ratio = 0;
            string line = string.Empty;
            bool res = false;
            try
            {
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    // File format
                    // Date,Bullish,Neutral,Bearish
                    // 9-11-87,50.0,23.0,27.0
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }
                        string[] row = line.Split(',');
                        date = DateTime.Parse(row[0], usCulture);
                        bullish = float.Parse(row[1], usCulture);
                        neutral = float.Parse(row[2], usCulture);
                        bearish = float.Parse(row[3], usCulture);
                        ratio = bearish / bullish;

                        bearBullRatioValue = new StockDailyValue(bearBullRatioName,
                            ratio,
                            ratio,
                            ratio,
                            ratio, 0, date);
                        stockSerie.Add(bearBullRatioValue.DATE, bearBullRatioValue);
                        bearBullRatioValue.Serie = stockSerie;
                    }
                }
                res = true;
            }
            catch (System.Exception e)
            {
                StockLog.Write("Failed to parse Bull/Bear ratio file: " + e.Message + "\r\r" + line);
            }
            return res;
        }

    }
}