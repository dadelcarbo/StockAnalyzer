using System;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class BreadthDataProvider : StockDataProviderBase
    {
        static private string FOLDER = @"\data\daily\Breadth";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Breadth";

        private static StockDictionary stockDictionary = null;

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            string line;
            string fileName = RootFolder + "\\BreadthCfg.txt";
            // Parse yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(RootFolder + FOLDER))
            {
                Directory.CreateDirectory(RootFolder + FOLDER);
            }
            if (!Directory.Exists(RootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ARCHIVE_FOLDER);
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
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(',');
                            string longName = row[0];
                            if (row[0] == "EQW.SRD") longName = "SRD";
                            if (row[0] == "EQW.CACALL") longName = "CACALL";
                            
                            if (!stockDictionary.ContainsKey(longName))
                            {
                                stockDictionary.Add(longName, new StockSerie(longName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]), StockDataProvider.Breadth, BarDuration.Daily));
                            }
                        }
                    }
                }
            }
        }
        public override bool LoadData(StockSerie stockSerie)
        {
            // Read archive first
            string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            string fullFileName = RootFolder + ARCHIVE_FOLDER + "\\" + fileName;
            bool res = ParseCSVFile(stockSerie, fullFileName);

            fullFileName = RootFolder + FOLDER + "\\" + fileName;
            res = ParseCSVFile(stockSerie, fullFileName) || res;

            res |= GenerateBreadthData(stockSerie);

            return res;
        }
        private bool GenerateBreadthData(StockSerie stockSerie)
        {
            string[] row = stockSerie.ShortName.Split('.');
            StockSerie.Groups group = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]);
            switch (row[0].Split('_')[0])
            {
                case "AD":
                    return stockDictionary.GenerateAdvDeclSerie(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "EQW":
                    return stockDictionary.GenerateIndiceEqualWeight(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "HL":
                    return stockDictionary.GenerateHigherThanHLTrailSerie(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
               case "ER":
                    return stockDictionary.GenerateERBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "EMA":
                    return stockDictionary.GenerateEMABreadthSerie(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "STOKF":
                    return stockDictionary.GenerateSTOKFBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "STOKS":
                    return stockDictionary.GenerateSTOKSBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "McClellan":
                    return stockDictionary.GenerateMcClellanSerie(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "McClellanSum":
                    return stockDictionary.GenerateMcClellanSumSerie(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "BBWIDTH":
                    return stockDictionary.GenerateBBWidthBreadth(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "MM":
                    return stockDictionary.GenerateHigherThanMMSerie(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
                case "MYOSC":
                    return stockDictionary.GenerateMyOscBreadth(stockSerie, row[1], RootFolder + FOLDER, RootFolder + ARCHIVE_FOLDER);
            }
            return false;
        }
        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            if (stockSerie.StockName.Contains("CAC40") || stockSerie.StockName.Contains("SBF120"))
            {
                StockDataProviderBase.DownloadSerieData(stockDictionary["CAC40"]);
                StockDataProviderBase.DownloadSerieData(stockDictionary["ACCOR"]);
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
