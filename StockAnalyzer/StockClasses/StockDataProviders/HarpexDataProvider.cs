using System;
using System.IO;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class HarpexDataProvider : StockDataProviderBase
   {
      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         bool res = false;
         string fileName = rootFolder + HARPEX_FILENAME;
         res = this.ParseHarpex(stockSerie, fileName);
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
         if (!stockDictionary.ContainsKey("Harpex"))
         {
            stockDictionary.Add("Harpex", new StockSerie("Harpex", "Harpex", StockSerie.Groups.INDICATOR, StockDataProvider.Harpex));
         }
      }
      static private string HARPEX_FILENAME = @"\data\weekly\Harpex\Harpex.csv";
      private bool ParseHarpex(StockSerie stockSerie, string fileName)
      {
         // Read new downloaded values
         if (!File.Exists(fileName))
         {
            return false;
         }
         bool res = false;

         StockDailyValue harpexValue = null;
         DateTime date;
         float harpex = 0;
         string line = string.Empty;
         try
         {
            using (StreamReader sr = new StreamReader(fileName, true))
            {
               // File format
               // "date";"HARPEX";"Class 1";"Class 2";"Class 3";"Class 4";"Class 5";"Class 6";"Class 7";"Class 8"
               // 14/07/07;1296.25;1196.94;1163.11;1207.74;1180.47;1406.11;1404.5;1388.44;1475.32

               sr.ReadLine(); // Skip first line
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  string[] row = line.Split(';');
                  date = DateTime.Parse(row[0], StockAnalyzerApp.Global.EnglishCulture);
                  harpex = float.Parse(row[1], StockAnalyzerApp.Global.EnglishCulture);

                  harpexValue = new StockDailyValue(stockSerie.StockName,
                      harpex,
                      harpex,
                      harpex,
                      harpex, 0, date);
                  stockSerie.Add(harpexValue.DATE, harpexValue);
                  harpexValue.Serie = stockSerie;
               }
            }
            res = true;
         }
         catch (System.Exception e)
         {
            StockLog.Write("Failed to parse Harpex file " + e.Message + "\r\r" + line);
         }
         return res;
      }
   }
}