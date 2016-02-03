using System;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class BreadthDataProvider : StockDataProviderBase
   {
      static private string FOLDER = @"\data\daily\Breadth";
      static private string ARCHIVE_FOLDER = @"\data\archive\daily\Breadth";

      private static StockDictionary stockDictionary = null;

      public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
      {
         string line;
         string fileName = rootFolder + "\\BreadthCfg.txt";
         // Parse yahoo.cfg file// Create data folder if not existing
         if (!Directory.Exists(rootFolder + FOLDER))
         {
            Directory.CreateDirectory(rootFolder + FOLDER);
         }
         if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
         {
            Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
         }

         BreadthDataProvider.stockDictionary = stockDictionary;
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
                     if (!stockDictionary.ContainsKey(row[0]))
                     {
                        stockDictionary.Add(row[0], new StockSerie(row[0], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]), StockDataProvider.Breadth));
                     }
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
         res = ParseCSVFile(stockSerie, fullFileName) || res;

         res |= GenerateBreadthData(rootFolder, stockSerie);

         return res;
      }

      private bool GenerateBreadthData(string rootFolder, StockSerie stockSerie)
      {
         string[] row = stockSerie.StockName.Split('.');
         StockSerie.Groups group = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]);
         switch (row[0].Split('_')[0])
         {
            case "AD":
               return stockDictionary.GenerateAdvDeclSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "TB":
               return stockDictionary.GenerateHigherThanTrailVolSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "HL":
               return stockDictionary.GenerateHigherThanHLTrailSerie(stockSerie, row[1], StockSerie.StockBarDuration.Daily, rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "ER":
               return stockDictionary.GenerateERBreadthSerie(stockSerie, row[1], StockSerie.StockBarDuration.Daily, rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "EMA":
               return stockDictionary.GenerateEMABreadthSerie(stockSerie, row[1], StockSerie.StockBarDuration.Daily, rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            //case "HLEMA20":
            //   return stockDictionary.GenerateHigherThanHLTrailSerie(stockSerie, row[1], StockSerie.StockBarDuration.Bar_1_EMA20, rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            //case "HLEMA5":
            //   return stockDictionary.GenerateHigherThanHLTrailSerie(stockSerie, row[1], StockSerie.StockBarDuration.Bar_1_EMA5, rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "ROCEX":
               return stockDictionary.GenerateBullishROCEXSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "MM":
               return stockDictionary.GenerateHigherThanMMSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "MYOSC":
               return stockDictionary.GenerateMyOscBreadth(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "LD":
               return stockDictionary.GenerateLowestInDays(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "HD":
               return stockDictionary.GenerateHighestInDays(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "RHI":
               return stockDictionary.GenerateRecordHighIndexInDays(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "CORREL":
               return stockDictionary.GenerateCorrelationSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
            case "TRIN":
               return stockDictionary.GenerateTrinSerie(stockSerie, row[1], rootFolder + FOLDER, rootFolder + ARCHIVE_FOLDER);
         }
         return false;
      }
      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         return true;
      }
      public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
      {
         if (stockSerie.StockName.Contains("CAC40") || stockSerie.StockName.Contains("SBF120"))
         {
            StockDataProviderBase.DownloadSerieData(rootFolder, stockDictionary["CAC40"]);
            StockDataProviderBase.DownloadSerieData(rootFolder, stockDictionary["ACCOR"]);
            stockSerie.IsInitialised = false;

            stockSerie.ClearBarDurationCache();

            return stockSerie.Initialise();
         }
         return true;
      }
      public override bool SupportsIntradayDownload
      {
         get { return true; }
      }
   }
}
