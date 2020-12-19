using System;
using System.IO;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class AAIIDataProvider : StockDataProviderBase
   {
      private static StockDictionary stockDictionary;
      // IStockDataProvider Implemetation
      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         bool res = false;
         string fileName = rootFolder + BULLBEARRATIO_FILENAME;
         if (stockSerie.StockName == bullBearLogRatioName)
         {
            stockDictionary[bearBullRatioName].Initialise();
            return this.GenerateBullBearLogRatio(stockSerie, true);
         }
         if (stockSerie.StockName == bearBullLogRatioName)
         {
            stockDictionary[bearBullRatioName].Initialise();
            return this.GenerateBullBearLogRatio(stockSerie, false);
         }
         if (stockSerie.StockName == bearBullRatioName)
         {
            return this.ParseBullBearRatio(stockSerie, fileName);
         }
         return res;
      }

      private bool GenerateBearBullLogRatio(StockSerie stockSerie)
      {
         throw new NotImplementedException();
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
         AAIIDataProvider.stockDictionary = stockDictionary;
         if (!stockDictionary.ContainsKey(bearBullRatioName))
         {
            stockDictionary.Add(bearBullRatioName, new StockSerie(bearBullRatioName, bearBullRatioName, StockSerie.Groups.INDICATOR, StockDataProvider.AAII, BarDuration.Weekly));
         }
         if (!stockDictionary.ContainsKey(bearBullLogRatioName))
         {
            stockDictionary.Add(bearBullLogRatioName, new StockSerie(bearBullLogRatioName, bearBullLogRatioName, StockSerie.Groups.INDICATOR, StockDataProvider.AAII, BarDuration.Weekly));
         }
         if (!stockDictionary.ContainsKey(bullBearLogRatioName))
         {
            stockDictionary.Add(bullBearLogRatioName, new StockSerie(bullBearLogRatioName, bullBearLogRatioName, StockSerie.Groups.INDICATOR, StockDataProvider.AAII, BarDuration.Weekly));
         }
      }

      static private string BULLBEARRATIO_FILENAME = @"\data\weekly\AAII\BullBearRatio.csv";
      static private string bearBullRatioName = "BEAR/BULL Ratio";
      static private string bearBullLogRatioName = "BEAR/BULL Log Ratio";
      static private string bullBearLogRatioName = "BULL/BEAR Log Ratio";

      static private char[] percent = new char[] { '%' };

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

                  string[] row = line.Replace(":","").Split(new []{',',':','\t'});
                  date = DateTime.Parse(row[0], usCulture);
                  if (row[1].Contains("%"))
                  {
                     bullish = float.Parse(row[1].TrimEnd(percent), usCulture) / 100f;
                     neutral = float.Parse(row[2].TrimEnd(percent), usCulture) / 100f;
                     bearish = float.Parse(row[3].TrimEnd(percent), usCulture) / 100f;
                  }
                  else
                  {
                     bullish = float.Parse(row[1], usCulture);
                     neutral = float.Parse(row[2], usCulture);
                     bearish = float.Parse(row[3], usCulture);
                  }
                  ratio = bearish / bullish;

                  bearBullRatioValue = new StockDailyValue(ratio,
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
      private bool GenerateBullBearLogRatio(StockSerie stockSerie, bool inverse)
      {
         StockSerie ratioSerie = AAIIDataProvider.stockDictionary[bearBullRatioName];
         
         StockDailyValue bearBullLogRatioValue = null;
         float ratio;
         foreach (StockDailyValue dailyValue in ratioSerie.Values)
         {
            ratio = (float) Math.Log10(dailyValue.CLOSE);
            if (inverse) ratio = -ratio;

            bearBullLogRatioValue = new StockDailyValue(ratio,
               ratio,
               ratio,
               ratio, 0, dailyValue.DATE);
            stockSerie.Add(bearBullLogRatioValue.DATE, bearBullLogRatioValue);
            bearBullLogRatioValue.Serie = stockSerie;
         }
         return true;
      }
   }
}