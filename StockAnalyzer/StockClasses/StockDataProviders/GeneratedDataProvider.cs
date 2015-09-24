using System;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class GeneratedDataProvider : StockDataProviderBase
   {
      static private string FOLDER = @"\data\daily\Generated";
      static private string ARCHIVE_FOLDER = @"\data\archive\daily\Generated";

      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
         string line;
         string fileName = rootFolder + "\\GeneratedIndicator.txt";
         // Parse yahoo.cfg file// Create data folder if not existing
         if (!Directory.Exists(rootFolder + FOLDER))
         {
            Directory.CreateDirectory(rootFolder + FOLDER);
         }
         if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
         {
            Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
         }

         if (File.Exists(fileName))
         {
            // Parse GeneratedIndicator.txt file
            using (StreamReader sr = new StreamReader(fileName, true))
            {
               sr.ReadLine(); // Skip first line
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  if (!line.StartsWith("#"))
                  {
                     string[] row = line.Split(',');
                     stockDictionary.Add(row[0], new StockSerie(row[0], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]), StockDataProvider.Generated));
                  }
               }
            }
         }
      }
      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         // Read archive first
         string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
         string fullFileName = rootFolder + ARCHIVE_FOLDER + "\\" + fileName;
         bool res = ParseCSVFile(stockSerie, fullFileName);

         fullFileName = rootFolder + FOLDER + "\\" + fileName;
         return ParseCSVFile(stockSerie, fullFileName) || res;
      }
      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         return true;
      }
      public override bool SupportsIntradayDownload
      {
         get { return false; }
      }
   }
}
